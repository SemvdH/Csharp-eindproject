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
            User user = new User(usernameTextbox.Text, 0, false);
            Client client = new Client();

            data.User = user;
            data.Client = client;

            MainWindow startWindow = new MainWindow();
            startWindow.Show();
            this.Close();
        }
    }
}
