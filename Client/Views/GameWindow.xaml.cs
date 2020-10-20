using Client.ViewModels;
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
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        ClientData data = ClientData.Instance;
        private ViewModelGame viewModel;
        public GameWindow()
        {
            this.viewModel = new ViewModelGame();
            DataContext = this.viewModel;
            InitializeComponent();
            
        }
        

        private void CanvasForPaint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.viewModel.Canvas_MouseDown(e, this);
        }

        private void CanvasForPaint_MouseMove(object sender, MouseEventArgs e)
        {
            viewModel.Canvas_MouseMove(e, this);
        }

        private void CanvasReset_Click(object sender, RoutedEventArgs e)
        {
            CanvasForPaint.Children.Clear();

            //FOR FUTURE USE, IF NECCESSARY
            //TEST.Children.Clear();

            //foreach (UIElement child in CanvasForPaint.Children)
            //{
            //    var xaml = System.Windows.Markup.XamlWriter.Save(child);
            //    var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as UIElement;
            //    TEST.Children.Add(deepCopy);
            //}

        }

        private void ClrPcker_Background_SelectedColorChanged_1(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            viewModel.Color_Picker(e, this);
        }

        private void ChatBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if enter then clear textbox and send message.
            if (e.Key.Equals(Key.Enter))
            {
                WriteToChat(ChatBox.Text);
                ChatBox.Clear();
            }
        }

        /*
         * Writes the current client's message to the chatbox.
         */
        private void WriteToChat(string message)
        {
            string user = data.User.Username; 
            SentMessage.AppendText($"{user}: {message}\n");
        }
    }
}
