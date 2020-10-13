using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using SharedClientServer;

namespace Client
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        

        public ViewModel()
        {
            _model = new Model();
            ButtonCommand = new RelayCommand(() =>
            {
                Client client = new Client();
            });

            _lobbies = new List<Lobby>();

            _lobbies.Add(new Lobby(50, 3, 8));
            _lobbies.Add(new Lobby(69, 1, 9));
            _lobbies.Add(new Lobby(420, 7, 7));
        }

        private void ClickCheck()
        {
            if(!(_model.Status))
                _model.Status = true;

            _model.Numbers = _model.Numbers + 5;
        }

        public ICommand ButtonCommand { get; set; }


        private Model _model;
        public Model Model
        {
            get
            {
                if (_model == null)
                    _model = new Model();

                return _model;
            }

            set
            {
                _model = value;
            }
        }

        private List<Lobby> _lobbies;
        public List<Lobby> Lobbies
        {
            get { return _lobbies; }
            set { _lobbies = value; }
        }




    }
}
