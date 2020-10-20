using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using SharedClientServer;
using System.Diagnostics;
using System.Windows;
using System.Collections.ObjectModel;
using Client.Views;

namespace Client
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OnHostButtonClick { get; set; }
        public ICommand JoinSelectedLobby { get; set; }

        public Lobby SelectedLobby { get; set; }

        public ViewModel()
        {
            _model = new Model();
            _lobbies = new ObservableCollection<Lobby>();

            _lobbies.Add(new Lobby(50, 3, 8));
            _lobbies.Add(new Lobby(69, 1, 9));
            _lobbies.Add(new Lobby(420, 7, 7));

            OnHostButtonClick = new RelayCommand(() => 
            {
                Debug.WriteLine("Host button clicked");
            });

            JoinSelectedLobby = new RelayCommand(() =>
            {
                if (SelectedLobby != null)
                {
                    GameWindow window = new GameWindow();
                    window.Show();
                }
                
            }, true);
        }

        private void ClickCheck()
        {
            if(!(_model.Status))
                _model.Status = true;

            _model.Numbers = _model.Numbers + 5;
        }



        private Model _model;
        public Model Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
            }
        }

        private ObservableCollection<Lobby> _lobbies;
        public ObservableCollection<Lobby> Lobbies
        {
            get { return _lobbies; }
            set { _lobbies = value; }
        }
    }
}
