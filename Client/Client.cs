using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Windows.Media;
using static SharedClientServer.JSONConvert;

namespace Client
{
    public delegate void LobbyJoinCallback(bool isHost);
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
        public LobbyCallback OnLobbyCreated;
        public LobbyCallback OnLobbyLeave;
        private ClientData data = ClientData.Instance;
        public CanvasDataReceived CanvasDataReceived;
        public CanvasReset CReset;
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
            this.tcpClient.EndConnect(ar);
            this.stream = tcpClient.GetStream();
            OnSuccessfullConnect?.Invoke();
            SendMessage(JSONConvert.ConstructUsernameMessage(username));
            this.stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete),null);
        }

        private void OnReadComplete(IAsyncResult ar)
        {
            int amountReceived = stream.EndRead(ar);
            if (totalBufferReceived + amountReceived > 2048)
            {
                throw new OutOfMemoryException("buffer too small");
            }

            // copy the received bytes into the buffer
            Array.Copy(buffer, 0, totalBuffer, totalBufferReceived, amountReceived);
            // add the bytes we received to the total amount
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
                Debug.WriteLine($"reduced buffer: {expectedMessageLength}");
                expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
            }
            ar.AsyncWaitHandle.WaitOne();
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete), null);
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
                        ViewModels.ViewModelGame.HandleIncomingMsg(textUsername, textMsg);
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
                            // we hebben gedrawed, dus stuur dat we weer kunnen drawen
                            
                            break;
                    }
                break;


                default:
                    Debug.WriteLine("[CLIENT] Received weird identifier: " + id);
                    break;
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
        }
    }
}
