using Client.Views;
using SharedClientServer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
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




    }
}
