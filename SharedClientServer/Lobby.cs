using SharedClientServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Client
{
    internal class Lobby : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private int _id;
        private int _playersIn;
        private int _maxPlayers;
        //private List<string> _usernames;
        private List<User> _users;

        //public void AddUsername(string username, out bool success)
        //{
        //    success = false;
        //    if (_usernames.Count < _maxPlayers)
        //    {
        //        _usernames.Add(username);
        //        success = true;
        //    }
        //}

        public Lobby(int id, int playersIn, int maxPlayers)
        {
            _id = id;
            _playersIn = playersIn;
            _maxPlayers = maxPlayers;
            //_usernames = new List<string>();
            _users = new List<User>();
        }

        public void AddUser(string username, out bool succes)
        {
            succes = false;
            if (_users.Count < _maxPlayers)
            {
                _users.Add(new User(username, 0, false));
                succes = true;
            }
        }

        public void AddUser(User user, out bool succes)
        {
            succes = false;
            if (_users.Count < _maxPlayers)
            {
                _users.Add(user);
                succes = true;
            }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public int PlayersIn
        {
            get { return _users.Count; }
            set { _playersIn = value; }
        }

        public void Set(Lobby lobby)
        {
            this._id = lobby._id;
            this._users = lobby._users;
            this._maxPlayers = lobby._maxPlayers;
        }

        public int MaxPlayers
        {
            get { return _maxPlayers; }
            set { _maxPlayers = value; }
        }

        public List<User> Users
        {
            get { return _users; }
            set { _users = value; }
        }


    }
}
