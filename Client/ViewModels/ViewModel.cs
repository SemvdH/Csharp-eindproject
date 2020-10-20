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
using System.Linq;

namespace Client
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OnHostButtonClick { get; set; }
        public ICommand JoinSelectedLobby { get; set; }

        public Lobby SelectedLobby { get; set; }

        private Client client;

        public ViewModel()
        {
            _model = new Model();
            _lobbies = new ObservableCollection<Lobby>();
            client = ClientData.Instance.Client;
            client.OnLobbiesListReceived = updateLobbies;
            

            OnHostButtonClick = new RelayCommand(hostGame);

            JoinSelectedLobby = new RelayCommand(joinLobby, true);
        }

        private void hostGame()
        {
            Debug.WriteLine("attempting to host game for " + ClientData.Instance.User.Username);
            client.SendMessage(JSONConvert.ConstructLobbyHostMessage());
            client.OnLobbyCreated = becomeHostForLobby;
        }

        private void becomeHostForLobby(int id, int players, int maxplayers)
        {
            
            Debug.WriteLine($"got host succes with data {id} {players} {maxplayers} ");
            Lobby newLobby = new Lobby(id, players, maxplayers);
            SelectedLobby = newLobby;
            Application.Current.Dispatcher.Invoke(delegate
            {
                _lobbies.Add(newLobby);
                startGameInLobby();
            });
        }

        private void joinLobby()
        {
            client.OnLobbyJoinSuccess = OnLobbyJoinSuccess;
            client.SendMessage(JSONConvert.ConstructLobbyJoinMessage(SelectedLobby.ID));
        }

        private void OnLobbyJoinSuccess()
        {
            startGameInLobby();
        }



        private void updateLobbies()
        {
            Lobby[] lobbiesArr = client.Lobbies;
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (Lobby lobby in lobbiesArr)
                {
                    _lobbies.Add(lobby);
                }
            });
        }



        private void startGameInLobby()
        {
            if (SelectedLobby != null)
            {
                ClientData.Instance.Lobby = SelectedLobby;
                startGameWindow();
            }
        }

        private void startGameWindow()
        {
            _model.CanStartGame = false;
            Application.Current.Dispatcher.Invoke(delegate
            {
                GameWindow window = new GameWindow();
                window.Show();
            });
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
