using Client.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientData data = ClientData.Instance;
        public MainWindow()
        {
            this.DataContext = new ViewModel();
            InitializeComponent();

            usernameTextbox.Text = data.User.Username;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Lobby lobbySelected = LobbyList.SelectedItem as Lobby;
            if(lobbySelected != null)
            {
                testLabel.Content = lobbySelected.ID;
                usernameTextbox.IsEnabled = false;
                colorSelection.IsEnabled = false;
                joinButton.IsEnabled = false;
                hostButton.IsEnabled = false;

                GameWindow window = new GameWindow();
                window.Show();
            }
        }
    }
}
