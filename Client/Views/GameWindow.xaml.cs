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

        public GameWindow()
        {
            DataContext = new ViewModelGame();
            InitializeComponent();
            
        }
        Point currentPoint = new Point();

        private void CanvasForPaint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(CanvasForPaint);
            }

        }

        private void CanvasForPaint_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

             
                line.Stroke = new SolidColorBrush(color);
                //line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(CanvasForPaint).X;
                line.Y2 = e.GetPosition(CanvasForPaint).Y;

                currentPoint = e.GetPosition(CanvasForPaint);

                CanvasForPaint.Children.Add(line);
            }

        }

        private Color color;

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Color colorSelected = new Color();
            colorSelected.A = 255;
            colorSelected.R = ClrPcker_Background.SelectedColor.Value.R;
            colorSelected.G = ClrPcker_Background.SelectedColor.Value.G;
            colorSelected.B = ClrPcker_Background.SelectedColor.Value.B;
            color = colorSelected;
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
            Color colorSelected = new Color();
            colorSelected.A = 255;
            colorSelected.R = ClrPcker_Background.SelectedColor.Value.R;
            colorSelected.G = ClrPcker_Background.SelectedColor.Value.G;
            colorSelected.B = ClrPcker_Background.SelectedColor.Value.B;
            color = colorSelected;
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
            string user = "Monkey"; 
            SentMessage.AppendText($"{user}: {message}\n");
        }
    }
}
