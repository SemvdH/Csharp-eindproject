using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClientServer
{
    class User
    {
        private string _username;
        private int _score;

        public User(string username, int score)
        {
            _username = username;
            _score = score;
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
    }
}
