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
        private LoginViewModel loginViewModel;
        public LoginScreen()
        {
            loginViewModel = new LoginViewModel(this);
            InitializeComponent();
        }

        private void Button_EnterUsername(object sender, RoutedEventArgs e)
        {
            string name = usernameTextbox.Text;
            if (name == string.Empty) return;
            LoginButton.IsEnabled = false;
            loginViewModel.UsernameEntered(name);

            
        }
    }
}
