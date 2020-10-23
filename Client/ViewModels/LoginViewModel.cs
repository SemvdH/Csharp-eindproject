using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Client.ViewModels
{

    class LoginViewModel
    {
        ClientData data = ClientData.Instance;
        private Window window;

        public LoginViewModel(Window window)
        {
            this.window = window;
        }
        public void UsernameEntered(string name) {
            User user = new User(name);

            Client client = new Client(user.Username);
            client.OnSuccessfullConnect = () =>
            {
                // because we need to start the main window on a UI thread, we need to let the dispatcher handle it, which will execute the code on the ui thread
                Application.Current.Dispatcher.Invoke(delegate {
                    data.User = user;
                    data.Client = client;
                    client.SendMessage(JSONConvert.ConstructLobbyRequestMessage());
                    MainWindow startWindow = new MainWindow();
                    startWindow.Show();
                    window.Close();
                });
            };
        }
    }
}
