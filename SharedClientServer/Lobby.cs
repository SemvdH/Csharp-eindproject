using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Client
{
    class Lobby : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private int _id;
        private int _playersIn;
        private int _maxPlayers;

        public Lobby(int id, int playersIn, int maxPlayers)
        {
            _id = id;
            _playersIn = playersIn;
            _maxPlayers = maxPlayers;
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public int PlayersIn
        {
            get { return _playersIn; }
            set { _playersIn = value; }
        }

        public int MaxPlayers
        {
            get { return _maxPlayers; }
            set { _maxPlayers = value; }
        }


    }
}
