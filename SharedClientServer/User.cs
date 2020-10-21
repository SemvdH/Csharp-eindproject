﻿using Newtonsoft.Json;
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
        private string _message;

        [JsonConstructor]
        public User(string username, int score, bool host)
        {
            _username = username;
            _score = score;
            _host = host;
        }

        public User(string username)
        {
            _username = username;
            _score = 0;
            _host = false;
        }

        public static bool operator ==(User u1, User u2)
        {
            return u1.Username == u2.Username;
        }

        public static bool operator !=(User u1, User u2)
        {
            return u1.Username != u2.Username;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                User other = obj as User;
                return other.Username == this.Username;
            }
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
    }
}
