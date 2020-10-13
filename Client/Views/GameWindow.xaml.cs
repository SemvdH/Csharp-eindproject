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

                line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(CanvasForPaint).X;
                line.Y2 = e.GetPosition(CanvasForPaint).Y;

                currentPoint = e.GetPosition(CanvasForPaint);

                CanvasForPaint.Children.Add(line);
            }

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
    }
}
