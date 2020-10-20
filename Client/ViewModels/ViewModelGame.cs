using GalaSoft.MvvmLight.Command;
using SharedClientServer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Client.ViewModels
{
    class ViewModelGame : INotifyPropertyChanged
    {
        ClientData data = ClientData.Instance;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        private dynamic _payload;

        private string _username;

        private string _message;
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

        public ViewModelGame()
        {
            if (_payload == null)
            {
                _message = "";
            }
            else
            {
                _message = _payload.message;
                _username = _payload.username;
            }
            OnKeyDown = new RelayCommand(ChatBox_KeyDown);
        }

        private void ChatBox_KeyDown()
        {
            //if enter then clear textbox and send message.
            if (Message != string.Empty) AddMessage(Message);
            Message = string.Empty;
        }

        internal void AddMessage(string message)
        {
            Messages.Add($"{data.User.Username}: {message}");

            _payload = new
            {
                username = data.User.Username,
                message = message
            };

            //Broadcast the message after adding it to the list!
            data.Client.SendMessage(JSONConvert.GetMessageToSend(JSONConvert.MESSAGE, _payload));
        }
    }
}
