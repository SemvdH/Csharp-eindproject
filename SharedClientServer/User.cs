using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClientServer
{
    class User
    {
        private string _username;
        private int _score;
        private bool _host;
        private bool _turnToDraw;
        private string _message;

        [JsonConstructor]
        public User(string username, int score, bool host, bool turnToDraw)
        {
            _username = username;
            _score = score;
            _host = host;
            _turnToDraw = turnToDraw;
        }

        public User(string username)
        {
            _username = username;
            _score = 0;
            _host = false;
            _turnToDraw = false;
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public bool Host
        {
            get { return _host; }
            set { _host = value; }
        }

        public bool TurnToDraw
        {
            get { return _turnToDraw; }
            set { _turnToDraw = value; }
        }
    }
}
