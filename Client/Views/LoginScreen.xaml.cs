using Client.ViewModels;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        ClientData data = ClientData.Instance;
        public LoginScreen()
        {
            InitializeComponent();
        }

        private void Button_EnterUsername(object sender, RoutedEventArgs e)
        {
            User user = new User(usernameTextbox.Text);
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
                    this.Close();
                });
            };

        }
    }
}
