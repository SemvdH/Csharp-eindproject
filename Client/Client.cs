using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Media;
using System.Windows;

using static SharedClientServer.JSONConvert;

namespace Client
{
    public delegate void LobbyJoinCallback(bool isHost);

    public delegate void RandomWord(string word);
    public delegate void HandleIncomingMsg(string username, string msg);
    internal delegate void HandleIncomingPlayer(Lobby lobby);
    public delegate void CanvasDataReceived(double[][] coordinates, Color color);
    public delegate void CanvasReset();
    public delegate void LobbyCallback(int id);
    

    class Client : ObservableObject
    {

        private ClientData clientData = ClientData.Instance;

        private TcpClient tcpClient;
        private NetworkStream stream;
        private byte[] buffer = new byte[2048];
        private byte[] totalBuffer = new byte[2048];
        private int totalBufferReceived = 0;
        public int Port = 5555;
        public bool Connected = false;
        private string username;
        public Callback OnSuccessfullConnect;
        public Callback OnLobbiesListReceived;
        public LobbyJoinCallback OnLobbyJoinSuccess;
        public Callback OnLobbiesReceivedAndWaitingForHost;
        public Callback OnServerDisconnect;
        public Callback OnLobbyUpdate;
        public LobbyCallback OnLobbyCreated;
        public LobbyCallback OnLobbyLeave;
        public RandomWord RandomWord;
        public HandleIncomingMsg IncomingMsg;
        public HandleIncomingPlayer IncomingPlayer;
        private ClientData data = ClientData.Instance;
        public CanvasDataReceived CanvasDataReceived;
        public CanvasReset CReset;
        public HandleIncomingPlayer UpdateUserScores;
        public Lobby[] Lobbies { get; set; }

        public Client(string username)
        {
            this.username = username;
            this.tcpClient = new TcpClient();
            Debug.WriteLine("Starting connect to server");
            tcpClient.BeginConnect("localhost", Port, new AsyncCallback(OnConnect), null);
        }

        private void OnConnect(IAsyncResult ar)
        {
            Debug.Write("finished connecting to server");
            try
            {
                this.tcpClient.EndConnect(ar);
                this.stream = tcpClient.GetStream();
                OnSuccessfullConnect?.Invoke();
                OnLobbyUpdate = updateGameLobby;
                SendMessage(JSONConvert.ConstructUsernameMessage(username));
                this.stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete),null);

            } catch (Exception e)
            {
                Debug.WriteLine("Can't connect, retrying...");
                tcpClient.BeginConnect("localhost", Port, new AsyncCallback(OnConnect), null);
            }
        }

        private void OnReadComplete(IAsyncResult ar)
        {

            if (ar == null || (!ar.IsCompleted) || (!this.stream.CanRead) || !this.tcpClient.Client.Connected)
                return;

            try
            {
                int amountReceived = stream.EndRead(ar);

                if (totalBufferReceived + amountReceived > 2048)
                {
                    throw new OutOfMemoryException("buffer too small");
                }

                Array.Copy(buffer, 0, totalBuffer, totalBufferReceived, amountReceived);
                totalBufferReceived += amountReceived;

                int expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);

                while (totalBufferReceived >= expectedMessageLength)
                {
                    // we have received the complete packet
                    byte[] message = new byte[expectedMessageLength];
                    // put the message received into the message array
                    Array.Copy(totalBuffer, 0, message, 0, expectedMessageLength);
                    handleData(message);

                    totalBufferReceived -= expectedMessageLength;
                    expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
                }

                ar.AsyncWaitHandle.WaitOne();
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete), null);
            } catch (IOException e)
            {
                Debug.WriteLine("[CLIENT] server not responding! got error: " + e.Message);
                OnServerDisconnect?.Invoke();   
            }
        }

        private void handleData(byte[] message)
        {
            byte id = message[4];

            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);

            Debug.WriteLine("[CLIENT] GOT STRING" + Encoding.ASCII.GetString(payload));
            switch (id)
            {
                case JSONConvert.LOGIN:
                    // json log in username data
                    break;
                case JSONConvert.MESSAGE:
                    // json message data
                    (string, string) combo = JSONConvert.GetUsernameAndMessage(payload);
                    string textUsername = combo.Item1;
                    string textMsg = combo.Item2;

                    if(textUsername != data.User.Username)
                    {
                        IncomingMsg?.Invoke(textUsername, textMsg);
                    }

                    //TODO display username and message in chat window
                    Debug.WriteLine("[CLIENT] INCOMING MESSAGE!");
                    Debug.WriteLine("[CLIENT] User name: {0}\t User message: {1}", textUsername, textMsg);
                    break;

                case JSONConvert.LOBBY:
                    // lobby data
                    LobbyIdentifier lobbyIdentifier = JSONConvert.GetLobbyIdentifier(payload);
                    switch (lobbyIdentifier)
                    {
                        case LobbyIdentifier.LIST:
                            Debug.WriteLine("got lobbies list");
                            Lobbies = JSONConvert.GetLobbiesFromMessage(payload);
                            OnLobbiesListReceived?.Invoke();
                            OnLobbiesReceivedAndWaitingForHost?.Invoke();
                            OnLobbyUpdate?.Invoke();
                            break;
                        case LobbyIdentifier.HOST:
                            // we receive this when the server has made us a host of a new lobby
                            // TODO get lobby id 
                            Debug.WriteLine("[CLIENT] got lobby object");
                            int lobbyCreatedID = JSONConvert.GetLobbyID(payload);
                            OnLobbyCreated?.Invoke(lobbyCreatedID);
                            break;
                        case LobbyIdentifier.JOIN_SUCCESS:
                            OnLobbyJoinSuccess?.Invoke(JSONConvert.GetLobbyJoinIsHost(payload));
                            break;
                        case LobbyIdentifier.LEAVE:
                            int lobbyLeaveID = JSONConvert.GetLobbyID(payload);
                            OnLobbyLeave?.Invoke(lobbyLeaveID);
                            break;
                    }
                    //TODO fill lobby with the data received
                    break;

                case JSONConvert.CANVAS:
                    // canvas data
                    //clientData.CanvasData = JSONConvert.getCoordinates(payload);         
                    int type = JSONConvert.GetCanvasMessageType(payload);
                    switch (type)
                    {
                        case JSONConvert.CANVAS_RESET:
                            CReset?.Invoke();
                            break;

                        case JSONConvert.CANVAS_WRITING:
                            CanvasDataReceived?.Invoke(JSONConvert.getCoordinates(payload), JSONConvert.getCanvasDrawingColor(payload));
                            
                            break;
                    }
                break;


                case JSONConvert.RANDOMWORD:
                    //Flag byte for receiving the random word.
                    int lobbyId = JSONConvert.GetLobbyID(payload);
                    string randomWord = JSONConvert.GetRandomWord(payload);

                    if (data.Lobby?.ID == lobbyId)
                        RandomWord?.Invoke(randomWord);
                    
                    break;
                default:
                    Debug.WriteLine("[CLIENT] Received weird identifier: " + id);
                    break;
            }
            SendMessage(JSONConvert.GetMessageToSend(JSONConvert.MESSAGE_RECEIVED,null));

        }

        private void updateGameLobby()
        {
            Debug.WriteLine("[CLIENT] updating game lobby");
            foreach (var item in Lobbies)
            {
                Debug.WriteLine("[CLIENT] lobby data: {0}", item.Users.Count);
                if (item.ID == data.Lobby?.ID)
                {
                    //IncomingPlayer?.Invoke(item);
                    UpdateUserScores?.Invoke(item as Lobby);
                }

            }
        }
        
        public void SendMessage(byte[] message)
        {
            Debug.WriteLine("[CLIENT] sending message " + Encoding.ASCII.GetString(message));
            stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWriteComplete), null);
        }

        private void OnWriteComplete(IAsyncResult ar)
        {
            Debug.WriteLine("[CLIENT] finished writing");
            stream.EndWrite(ar);
            stream.Flush();
        }
    }
}
