
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Models
{
    class ServerCommunication : ObservableObject
    {
        private TcpListener listener;
        private List<ServerClient> serverClients;
        public bool Started = false;
        private static readonly object padlock = new object();
        private static ServerCommunication instance = null;

        private ServerCommunication()
        {
            listener = new TcpListener(IPAddress.Any, 5555);
            serverClients = new List<ServerClient>();
        }

        public static ServerCommunication INSTANCE
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null) {
                        instance = new ServerCommunication();
                    }

                }
                return INSTANCE;
            }
        }

        public void Start()
        {
            listener.Start();
            Debug.WriteLine($"================================================\nStarted Accepting clients at {DateTime.Now}\n================================================");
            Started = true;
            listener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        private void OnClientConnected(IAsyncResult ar) 
        {
            TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Got connection from {tcpClient.Client.RemoteEndPoint}");
            ServerClient sc = new ServerClient(tcpClient);
            
            serverClients.Add(new ServerClient(tcpClient));
            listener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        public void sendToAll(byte[] message)
        {
            foreach (ServerClient sc in serverClients)
            {
                sc.sendMessage(message);
            }
        }
    }
}
