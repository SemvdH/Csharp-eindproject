
using Client.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SharedClientServer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Client.ViewModels
{
    class ViewModelGame : INotifyPropertyChanged
    {
        private ClientData data = ClientData.Instance;

        public event PropertyChangedEventHandler PropertyChanged;

        private Point currentPoint = new Point();
        private Color color;

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Players { get; } = new ObservableCollection<string>();

        private dynamic _payload;

        private string _randomWord;

        public string RandomWord
        {
            get { return _randomWord; }
            set { _randomWord = value; }
        }

        public string _message;
        public string Message
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

        public ICommand OnKeyDown { get; set; }

        public void Canvas_MouseDown(MouseButtonEventArgs e, GameWindow window)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(window.CanvasForPaint);
            }
        }

        public void Canvas_MouseMove(MouseEventArgs e, GameWindow window)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double[] coordinates = new double[4];
                Line line = new Line();

                line.Stroke = new SolidColorBrush(color);
                //line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(window.CanvasForPaint).X;
                line.Y2 = e.GetPosition(window.CanvasForPaint).Y;
                coordinates[0] = line.X1;
                coordinates[1] = line.Y1;
                coordinates[2] = line.X2;
                coordinates[3] = line.Y2;
                currentPoint = e.GetPosition(window.CanvasForPaint);

                window.CanvasForPaint.Children.Add(line);
                data.Client.SendMessage(JSONConvert.GetMessageToSend(0x04, coordinates));
            }
        }

        public void Color_Picker(RoutedPropertyChangedEventArgs<Color?> e, GameWindow window)
        {
            Color colorSelected = new Color();
            colorSelected.A = 255;
            colorSelected.R = window.ClrPcker_Background.SelectedColor.Value.R;
            colorSelected.G = window.ClrPcker_Background.SelectedColor.Value.G;
            colorSelected.B = window.ClrPcker_Background.SelectedColor.Value.B;
            color = colorSelected;
        }


        public ViewModelGame()
        {
            OnKeyDown = new RelayCommand(ChatBox_KeyDown);
            data.Client.RandomWord = HandleRandomWord;
            data.Client.IncomingMsg = HandleIncomingMsg;
            data.Client.IncomingPlayer = HandleIncomingPlayer;
        }

        private void ChatBox_KeyDown()
        {
            //if enter then clear textbox and send message.
            if (Message != string.Empty) AddMessage(Message);
            Message = string.Empty;
        }

        internal void AddMessage(string incomingMessage)
        {
            Messages.Add($"{data.User.Username}: {incomingMessage}");
            _payload = new
            {
                username = data.User.Username,
                message = incomingMessage
            };

            //Broadcast the message after adding it to the list!
            data.Client.SendMessage(JSONConvert.GetMessageToSend(JSONConvert.MESSAGE, _payload));
        }

        /*
         * MISC make this a callback
         * Handles the incoming chat message from another client.
         */
        public void HandleIncomingMsg(string username, string message)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Messages.Add($"{username}: {message}");
            });
        }
        public void LeaveGame(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Leaving...");
            Application.Current.Dispatcher.Invoke(delegate
            {
                Players.Remove(data.User.Username);
            });
            data.Client.SendMessage(JSONConvert.ConstructLobbyLeaveMessage(data.Lobby.ID));
        }

        /*
         * Handles the random word that has been received from the server.
         */
        public void HandleRandomWord(string randomWord)
        {
            RandomWord = randomWord;
            Debug.WriteLine($"[CLIENT] The random word is: {_randomWord}");
        }

        public void HandleIncomingPlayer(Lobby lobby)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Players.Clear();
                foreach (var item in lobby.Users)
                {
                    Players.Add(item.Username);
                }
            });
        }
    }
}

