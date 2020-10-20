
using Client;
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
        public List<Lobby> lobbies;
        private Dictionary<Lobby, List<ServerClient>> serverClientsInlobbies;
        public Action newClientAction;
        

        /// <summary>
        /// use a padlock object to make sure the singleton is thread-safe
        /// </summary>
        private static readonly object padlock = new object();

        private static ServerCommunication instance = null;
        public int port = 5555;

        private ServerCommunication()
        {
            listener = new TcpListener(IPAddress.Any, port);
            serverClients = new List<ServerClient>();
            lobbies = new List<Lobby>();
            lobbies.Add(new Lobby(1,1,1));
            lobbies.Add(new Lobby(2, 2, 2));
            lobbies.Add(new Lobby(3, 3, 3));
            serverClientsInlobbies = new Dictionary<Lobby, List<ServerClient>>();
        }

        /// <summary>
        /// returns the singleton serverCommunication instance
        /// </summary>
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
                return instance;
            }
        }

        /// <summary>
        /// start the server and start listening on port 5555 and begin acceptinc tcp clients
        /// </summary>
        public void Start()
        {
            listener.Start();
            Debug.WriteLine($"================================================\nStarted Accepting clients at {DateTime.Now}\n================================================");
            Started = true;
            // when we have accepted a tcp client, call the onclientconnected callback method

            listener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        /// <summary>
        /// callback method that gets called when a client is accepted
        /// </summary>
        /// <param name="ar">the result of the asynchronous connect call</param>
        private void OnClientConnected(IAsyncResult ar) 
        {
            // stop the acceptation
            var tcpClient = listener.EndAcceptTcpClient(ar);
            Debug.WriteLine($"Got connection from {tcpClient.Client.RemoteEndPoint}");
            newClientAction.Invoke();
            // create a new serverclient object and add it to the list
            serverClients.Add(new ServerClient(tcpClient));
            //start listening for new tcp clients
            listener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        /// <summary>
        /// send a message to all tcp clients in the list
        /// </summary>
        /// <param name="message">the message to send</param>
        public void sendToAll(byte[] message)
        {
            foreach (ServerClient sc in serverClients)
            {
                sc.sendMessage(message);
            }
        }

        public void SendToAllExcept(string username, byte[] message)
        {
            foreach (ServerClient sc in serverClients)
            {
                if (sc.User.Username != username) sc.sendMessage(message);
            }
        }

        public void SendToLobby(Lobby lobby, byte[] message)
        {
            foreach (Lobby l in lobbies)
            {
                if (l == lobby)
                {
                    foreach (ServerClient sc in serverClientsInlobbies[l])
                    {
                        sc.sendMessage(message);
                    }
                    break;
                }
            }
        }

        public void AddToLobby(Lobby lobby, User user)
        {
            foreach (Lobby l in lobbies)
            {
                if (l == lobby)
                {
                    bool succ;
                    l.AddUser(user, out succ);
                    if (!succ)
                    {
                        // TODO send lobby full message
                    } else
                    {
                        foreach(ServerClient sc in serverClients)
                        {
                            if (sc.User.Username == user.Username)
                            {
                                serverClientsInlobbies[l].Add(sc);
                                break;
                            }
                        }
                        
                    }
                    break;
                }
            }
        }
    }
}
