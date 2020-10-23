

using Client;
using Newtonsoft.Json.Linq;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static SharedClientServer.JSONConvert;

namespace Server.Models
{
    public delegate void Callback();
    class ServerClient : ObservableObject
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private byte[] buffer = new byte[2048];
        private byte[] totalBuffer = new byte[2048];
        private string _randomWord = "";
        private int totalBufferReceived = 0;
        private Dictionary<System.Timers.Timer, int> lobbyTimers;
        public User User { get; set; }
        private ServerCommunication serverCom = ServerCommunication.INSTANCE;
        private Callback OnMessageReceivedOk;


        /// <summary>
        /// Constructor that creates a new serverclient object with the given tcp client.
        /// </summary>
        /// <param name="client">the TcpClient object to use</param>
        public ServerClient(TcpClient client)
        {
            lobbyTimers = new Dictionary<System.Timers.Timer, int>();
            Debug.WriteLine("[SERVERCLIENT] making new instance and starting");
            tcpClient = client;
            stream = tcpClient.GetStream();
            Debug.WriteLine("[SERVERCLIENT] starting read");
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        /// <summary>
        /// callback method that gets called when the stream has finished reading a message from the stream.
        /// </summary>
        /// <param name="ar">the async result status</param>
        private void OnRead(IAsyncResult ar)
        {
            if (ar == null || (!ar.IsCompleted) || (!this.stream.CanRead) || !this.tcpClient.Client.Connected)
                return;

            try
            {
                int bytesReceived = this.stream.EndRead(ar);

                if (totalBufferReceived + bytesReceived > 2048)
                {
                    throw new OutOfMemoryException("buffer is too small!");
                }


                // copy the received bytes into the buffer
                Array.Copy(buffer, 0, totalBuffer, totalBufferReceived, bytesReceived);
                // add the bytes we received to the total amount
                totalBufferReceived += bytesReceived;

                // calculate the expected length of the message
                int expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);

                while (totalBufferReceived >= expectedMessageLength)
                {
                    // we have received the full packet
                    byte[] message = new byte[expectedMessageLength];
                    // copy the total buffer contents into the message array so we can pass it to the handleIncomingMessage method
                    Array.Copy(totalBuffer, 0, message, 0, expectedMessageLength);
                    HandleIncomingMessage(message);

                    // move the contents of the totalbuffer to the start of the array
                    Array.Copy(totalBuffer, expectedMessageLength, totalBuffer, 0, (totalBufferReceived - expectedMessageLength));

                    // remove the length of the expected message from the total buffer
                    totalBufferReceived -= expectedMessageLength;
                    // and set the new expected length to the rest that is still in the buffer
                    expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);

                    if (expectedMessageLength == 0)
                    {
                        break;
                    }


                }

                ar.AsyncWaitHandle.WaitOne();
                // start reading for a new message
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
            }
            catch (IOException e)
            {
                Debug.WriteLine("[SERVERCLIENT] Client disconnected! exception was " + e.Message);
                tcpClient.Close();
                ServerCommunication.INSTANCE.ServerClientDisconnect(this);
            }


        }

        /// <summary>
        /// Method to handle incoming message data
        /// </summary>
        /// <param name="message">the incoming message</param>
        private void HandleIncomingMessage(byte[] message)
        {
            Debug.WriteLine($"Got message : {Encoding.ASCII.GetString(message)}");
            byte id = message[4];
            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);
            Debug.WriteLine("[SERVERCLIENT] GOT STRING" + Encoding.ASCII.GetString(payload));
            switch (id)
            {

                case JSONConvert.LOGIN:
                    // json log in username data
                    string uName = JSONConvert.GetUsernameLogin(payload);

                    if (uName != null)
                    {
                        User = new User(uName);
                        User.Username = uName;
                        Debug.WriteLine("[SERVERCLIENT] set username to " + uName);

                    }
                    break;
                case JSONConvert.MESSAGE:
                    // json message data
                    (string, string) combo = JSONConvert.GetUsernameAndMessage(payload);
                    string textUsername = combo.Item1;
                    string textMsg = combo.Item2;

                    //Takes the data sent from the client, and then sets it in a data packet to be sent.
                    dynamic packet = new
                    {
                        username = textUsername,
                        message = textMsg
                    };

                    if (textMsg == _randomWord && !string.IsNullOrEmpty(_randomWord))
                    {
                        Debug.WriteLine($"[SERVERCLIENT] word has been guessed! {User.Username} + Word: {_randomWord}");
                    }

                    //Sends the incomming message to be broadcast to all of the clients inside the current lobby.
                    serverCom.SendToLobby(serverCom.GetLobbyForUser(User), JSONConvert.GetMessageToSend(JSONConvert.MESSAGE, packet));
                    break;

                case JSONConvert.LOBBY:
                    // lobby data
                    LobbyIdentifier l = JSONConvert.GetLobbyIdentifier(payload);
                    handleLobbyMessage(payload, l);
                    break;

                case JSONConvert.CANVAS:
                    
                    int typeToCheck = JSONConvert.GetCanvasMessageType(payload);
                    switch (typeToCheck)
                    {
                        case JSONConvert.CANVAS_WRITING:
                            dynamic canvasData = new
                            {
                                canvasType = typeToCheck,
                                coords = JSONConvert.getCoordinates(payload),
                                color = JSONConvert.getCanvasDrawingColor(payload)
                            };
                            //serverCom.SendToLobby(serverCom.GetLobbyForUser(User),JSONConvert.GetMessageToSend(JSONConvert.CANVAS,canvasData));
                            serverCom.SendCanvasDataToLobby(serverCom.GetLobbyForUser(User), User.Username, JSONConvert.GetMessageToSend(JSONConvert.CANVAS, canvasData));
                            break;

                        case JSONConvert.CANVAS_RESET:
                            dynamic canvasDataForReset = new
                            {
                                canvasType = JSONConvert.GetCanvasMessageType(payload)
                            };
                            serverCom.SendToLobby(serverCom.GetLobbyForUser(User), JSONConvert.GetMessageToSend(CANVAS, canvasDataForReset));
                            break;
                    }
                    

                    // canvas data
                    // todo send canvas data to all other serverclients in lobby
                    break;

                case JSONConvert.GAME:
                    Debug.WriteLine("[SERVERCLIENT] Got a message about the game logic");
                    GameCommand command = JSONConvert.GetGameCommand(payload);
                    switch (command)
                    {
                        case GameCommand.START_GAME:
                            int lobbyID = JSONConvert.GetStartGameLobbyID(payload);
                            serverCom.CloseALobby(lobbyID);
                            //todo start a timer for this lobby
                            Debug.WriteLine("[SERVERCLIENT] making timer for lobby " + lobbyID);
                            System.Timers.Timer lobbyTimer = new System.Timers.Timer(60 * 1000);
                            this.lobbyTimers.Add(lobbyTimer, lobbyID);
                            lobbyTimer.Elapsed += LobbyTimer_Elapsed;
                            lobbyTimer.Start();
                            ServerCommunication.INSTANCE.sendToAll(JSONConvert.ConstructLobbyListMessage(ServerCommunication.INSTANCE.lobbies.ToArray()));
                            break;
                        case GameCommand.TIMER_ELAPSED:
                            
                            break;
                        case GameCommand.NEXT_ROUND:
                            // The next round has been started, so we can start the timer again
                            lobbyID = JSONConvert.GetLobbyID(payload);
                            foreach (System.Timers.Timer timer in lobbyTimers.Keys)
                            {
                                if (lobbyTimers[timer] == lobbyID)
                                {
                                    timer.Start();
                                    break;
                                }
                            }
                            break;
                    }

                    break;
                case JSONConvert.RANDOMWORD:
                    //Flag byte for receiving the random word.
                    break;
                case JSONConvert.MESSAGE_RECEIVED:
                    // we now can send a new message
                    OnMessageReceivedOk?.Invoke();
                    break;

                default:
                    Debug.WriteLine("[SERVER] Received weird identifier: " + id);
                    break;
            }
        }

        private void LobbyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            System.Timers.Timer timer = sender as System.Timers.Timer;
            int lobbyID = lobbyTimers[timer];
            Debug.WriteLine("[SERVERCLIENT] timer elapsed for lobby " + lobbyID);
            serverCom.SendToLobby(lobbyID, JSONConvert.ConstructGameTimerElapsedMessage(lobbyID));
            timer.Stop();
            

        }

        private void handleLobbyMessage(byte[] payload, LobbyIdentifier l)
        {
            switch (l)
            {
                case LobbyIdentifier.REQUEST:
                    Debug.WriteLine("[SERVERCLIENT] got lobby request message, sending lobbies...");
                    sendMessage(JSONConvert.ConstructLobbyListMessage(ServerCommunication.INSTANCE.lobbies.ToArray()));
                    break;
                case LobbyIdentifier.HOST:
                    // add new lobby and add this serverclient to it
                    int createdLobbyID = ServerCommunication.INSTANCE.HostForLobby(this.User);
                    Debug.WriteLine("[SERVERCLIENT] created lobby");
                    sendMessage(JSONConvert.ConstructLobbyHostCreatedMessage(createdLobbyID));
                    ServerCommunication.INSTANCE.sendToAll(JSONConvert.ConstructLobbyListMessage(ServerCommunication.INSTANCE.lobbies.ToArray()));
                    break;
                case LobbyIdentifier.JOIN:
                    int id = JSONConvert.GetLobbyID(payload);
                    bool isHost;
                    ServerCommunication.INSTANCE.JoinLobby(this.User,id, out isHost);
                    sendMessage(JSONConvert.ConstructLobbyJoinSuccessMessage(isHost));
                    ServerCommunication.INSTANCE.sendToAll(JSONConvert.ConstructLobbyListMessage(ServerCommunication.INSTANCE.lobbies.ToArray()));
                    OnMessageReceivedOk = () =>
                    {
                        _randomWord = JSONConvert.SendRandomWord("WordsForGame.json");
                        serverCom.sendToAll(JSONConvert.GetMessageToSend(JSONConvert.RANDOMWORD, new
                        {
                            id = serverCom.GetLobbyForUser(User).ID,
                            word = _randomWord
                        }));
                        OnMessageReceivedOk = null;
                    };
                    break;
                case LobbyIdentifier.LEAVE:
                    id = JSONConvert.GetLobbyID(payload);
                    ServerCommunication.INSTANCE.LeaveLobby(User, id);
                    sendMessage(JSONConvert.ConstructLobbyLeaveMessage(id));
                    ServerCommunication.INSTANCE.sendToAll(JSONConvert.ConstructLobbyListMessage(ServerCommunication.INSTANCE.lobbies.ToArray()));
                    break;
            }
        }

        private async void SendLobbyData()
        {
            string result = await WaitForData();
            if(result == "bruh momento")
            {
              
            }
        }

        private async Task<string> WaitForData()
        {
            await Task.Delay(1000);
            return "bruh momento";
        }

        /// <summary>
        /// sends a message to the tcp client
        /// </summary>
        /// <param name="message">message to send</param>
        public void sendMessage(byte[] message)
        {
            // start writing the message from the start and until it is empty. When we are done we want to execute the OnWrite method.
            stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        /// <summary>
        /// callback method that gets called when the stream has finished writing the message
        /// </summary>
        /// <param name="ar">the async result status</param>
        private void OnWrite(IAsyncResult ar)
        {
            // end writing
            stream.EndWrite(ar);
        }
    }
}
