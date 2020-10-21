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
using System.Windows.Data;

namespace Client
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OnHostButtonClick { get; set; }
        public ICommand JoinSelectedLobby { get; set; }

        public Lobby SelectedLobby { get; set; }

        private Client client;

        private bool wantToBeHost = false;
        private int wantToBeHostId = 0;

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

        private void becomeHostForLobby(int id)
        {
            
            Debug.WriteLine($"got host succes with data {id} ");
            wantToBeHost = true;
            wantToBeHostId = id;
            client.OnLobbiesReceivedAndWaitingForHost = hostLobbiesReceived;

        }

        private void hostLobbiesReceived()
        {
            if (wantToBeHost)
                foreach (Lobby l in Lobbies)
                {
                    if (l.ID == wantToBeHostId)
                    {
                        Debug.WriteLine("found lobby that we want to be host of: " + l.ID + ", joining..");
                        SelectedLobby = l;
                        startGameInLobby();
                        wantToBeHost = false;
                        client.OnLobbiesReceivedAndWaitingForHost = null;
                        break;
                    }
                }
        }

        private void joinLobby()
        {
            // lobby die je wilt joinen verwijderen
            // nieuwe binnengekregen lobby toevoegen
            client.OnLobbyJoinSuccess = OnLobbyJoinSuccess;
            client.SendMessage(JSONConvert.ConstructLobbyJoinMessage(SelectedLobby.ID));
        }

        private void OnLobbyJoinSuccess()
        {
            startGameInLobby();
        }



        private void updateLobbies()
        {
            Debug.WriteLine("updating lobbies...");
            Lobby[] lobbiesArr = client.Lobbies;
            Application.Current.Dispatcher.Invoke(delegate
            {
                
                //for (int i = 0; i < lobbiesArr.Length; i++)
                //{
                //    Lobby lobby = lobbiesArr[i];
                //    Debug.WriteLine(lobby.PlayersIn);
                //    if (i < _lobbies.Count && _lobbies[i].ID == lobby.ID)
                //    {
                //        _lobbies[i].Set(lobby);
                //    } else
                //    {
                //        _lobbies.Add(lobbiesArr[i]);
                //    }
                //}

                _lobbies.Clear();

                foreach (Lobby l in lobbiesArr)
                {
                    _lobbies.Add(l);
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
