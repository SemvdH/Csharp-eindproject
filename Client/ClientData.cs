using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Client
{
    class ClientData
    {
        private static ClientData _instance;
        private static readonly object padlock = new object();

        public static ClientData Instance
        {
            get
            {
                lock (padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ClientData();
                    }
                    return _instance;
                }
            }
        }


        private User _user;
        private Client _client;
        private Lobby _lobby;
        private string _message;

        private ClientData()
        {

        }


        public User User
        {
            get { return _user; }
            set { _user = value; }
        }

        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public Lobby Lobby
        {
            get { return _lobby; }
            set { _lobby = value; }
        }

        public String Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }

    }
}
