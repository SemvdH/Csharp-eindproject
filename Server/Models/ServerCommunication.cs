
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Server.Models
{
    class ServerCommunication : ObservableObject
    {
        private TcpListener listener;
        private List<ServerClient> serverClients;
        public bool Started = false;

        public ServerCommunication(TcpListener listener)
        {
            this.listener = listener;
            serverClients = new List<ServerClient>();
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
    }
}
