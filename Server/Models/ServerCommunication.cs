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
        internal Action DisconnectClientAction;
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
            Lobby temp = new Lobby(1, 7, 8);
            lobbies.Add(temp);
            serverClientsInlobbies = new Dictionary<Lobby, List<ServerClient>>();
            serverClientsInlobbies.Add(temp, new List<ServerClient>());
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
        public async void sendToAll(byte[] message)
        {
            foreach (ServerClient sc in serverClients)
            {
                sc.sendMessage(message);
            }
        }

        public void ServerClientDisconnect(ServerClient serverClient)
        {
            Debug.WriteLine("[SERVERCOMM] handling disconnect");
            DisconnectClientAction?.Invoke();
            int id = -1;
            foreach (Lobby l in serverClientsInlobbies.Keys)
            {
                if (serverClientsInlobbies[l].Contains(serverClient))
                {
                    id = l.ID;
                }break;
            }

            if (id != -1)
            {
                LeaveLobby(serverClient.User, id);
                SendToAllExcept(serverClient, JSONConvert.ConstructLobbyLeaveMessage(id));
            }
        }

        public void SendToAllExcept(string username, byte[] message)
        {
            foreach (ServerClient sc in serverClients)
            {
                if (sc.User.Username != username) sc.sendMessage(message);
            }
        }

        public void SendToAllExcept(ServerClient sc, byte[] message)
        {
            foreach (ServerClient s in serverClients)
            {
                if (s != sc) s.sendMessage(message);
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
                        Debug.WriteLine("[SERVERCLIENT] Sending message to lobby");
                        sc.sendMessage(message);
                    }
                    break;
                }
            }
        }

        public void SendToLobby(int lobbyID, byte[] message)
        {
            foreach (Lobby l in lobbies)
            {
                if (l.ID == lobbyID)
                {
                    foreach (ServerClient sc in serverClientsInlobbies[l])
                    {
                        Debug.WriteLine("[SERVERCLIENT] Sending message to lobby");
                        sc.sendMessage(message);
                    }
                    break;
                }
            }
        }

        public void SendCanvasDataToLobby(Lobby lobby, string username, byte[] message)
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

        public Lobby GetLobbyForUser(User user)
        {
            foreach (Lobby l in lobbies)
            {
                if (l.Users.Contains(user))
                {
                    return l;
                }
            }
            return null;
        }

        public void AddToLobby(Lobby lobby, User user)
        {
            foreach (Lobby l in lobbies)
            {
                if (l == lobby)
                {
                    bool succ;
                    l.AddUser(user, out succ);
                    Debug.WriteLine("[SERVERCOMM] added user to lobby, now contains " + l.PlayersIn);
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


        public int HostForLobby(User user)
        {
            Lobby lobby = new Lobby( lobbies.Count + 1,0, 8);
            lobbies.Add(lobby);
            serverClientsInlobbies.Add(lobby, new List<ServerClient>());
            user.Host = true;
            AddToLobby(lobby, user);
            return lobby.ID;
        }

        public void JoinLobby(User user, int id, out bool isHost)
        {
            isHost = false;
            foreach (Lobby l in lobbies)
            {
                if (l.ID == id)
                {
                    if (l.Users.Count == 0)
                    {
                        user.Host = true;
                        isHost = true;
                    }
                    AddToLobby(l, user);
                    Debug.WriteLine($"{user.Username} joined lobby with id {id}");
                    break;
                }
            }
        }

        public void LeaveLobby(User user, int id)
        {
            Debug.WriteLine("[SERVERCOMM] removing user from lobby");
            foreach (Lobby l in lobbies)
            {
                if (l.ID == id)
                {
                    Debug.WriteLine($"[SERVERCOMM] checking for lobby with id {l.ID}");
                    
                    foreach (User u in l.Users)
                    {
                        Debug.WriteLine($"[SERVERCOMM] checking if {u.Username} is {user.Username} ");
                        // contains doesn't work, so we'll do it like this...
                        if (u.Username == user.Username)
                        {
                            Debug.WriteLine("[SERVERCOMM] removed user from lobby!");
                            l.Users.Remove(user);
                            foreach (ServerClient sc in serverClients)
                            {
                                if (sc.User.Username == user.Username)
                                {
                                    serverClientsInlobbies[l].Remove(sc);
                                    break;
                                }
                            }
                            if (l.Users.Count != 0)
                            {
                                l.Users[0].Host = true;
                            }
                            break;
                        }
                    }

                    
                   
                }
            }
        }

        public void CloseALobby(int lobbyID)
        {
            foreach (Lobby lobby in lobbies)
            {
                if (lobby.ID == lobbyID)
                {
                    lobby.LobbyJoinable = false;
                    break;
                }
            }
        }

        public string FindUserNameInLobby(int lobbyID)
        {
            Lobby lobbyFound = null;
            foreach (Lobby lobby in lobbies)
            {
                if (lobby.ID == lobbyID)
                {
                    lobbyFound = lobby;
                    break;
                }
            }

            return lobbyFound?.Users[lobbyFound.UserDrawing]?.Username;
        }
    }
}
