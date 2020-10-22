﻿using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using static SharedClientServer.JSONConvert;

namespace Client
{
    public delegate void OnLobbyCreated(int id);

    public delegate void CanvasDataReceived(double[] coordinates);
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
        public Callback OnLobbyJoinSuccess;
        public Callback OnLobbiesReceivedAndWaitingForHost;
        public OnLobbyCreated OnLobbyCreated;

        public CanvasDataReceived CanvasDataReceived;
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

            if (totalBufferReceived > 2048)
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
                Debug.WriteLine($"reduced buffer: {expectedMessageLength}");
                expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
            }

            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete), null);
        }

        private void handleData(byte[] message)
        {
            byte id = message[4];

            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);

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
                            OnLobbyJoinSuccess?.Invoke();
                            break;
                    }
                    //TODO fill lobby with the data received
                    break;

                case JSONConvert.CANVAS:
                    // canvas data
                    //clientData.CanvasData = JSONConvert.getCoordinates(payload);                   
                    CanvasDataReceived?.Invoke(JSONConvert.getCoordinates(payload));
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
