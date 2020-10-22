
﻿using Client.Views;
using GalaSoft.MvvmLight.Command;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Client.ViewModels
{
    class ViewModelGame : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ClientData data = ClientData.Instance;
        private GameWindow window;
        private Point currentPoint = new Point();
        public Color color;
        public double[][] buffer;
        public int pos = 0;
        public int maxLines = 1;
        public Queue<double[][]> linesQueue;
        private Timer queueTimer;

        public static ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        private dynamic _payload;

        public string _username;

        public string _message;
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

        public bool IsHost
        {
            get { return data.User.Host; }
        }

        public ViewModelGame(GameWindow window)
        {
            this.window = window;
            if (_payload == null)
            {
                _message = "";

            }
            else
            {
                //_message = data.Message;
                //_username = data.User.Username;
                //Messages.Add($"{data.User.Username}: {Message}");
            }

            buffer = new double[maxLines][];
            linesQueue = new Queue<double[][]>();
            OnKeyDown = new RelayCommand(ChatBox_KeyDown);
            ButtonStartGame = new RelayCommand(BeginGame);
            ButtonResetCanvas = new RelayCommand(CanvasResetLocal);
            data.Client.CanvasDataReceived = UpdateCanvasWithNewData;
            data.Client.CReset = CanvasResetData;
        }

        public ICommand OnKeyDown { get; set; }
        public ICommand ButtonStartGame { get; set; }
        public ICommand ButtonResetCanvas { get; set; }
       
        public void BeginGame()
        {
            
            queueTimer = new Timer(500);
            queueTimer.Start();
            queueTimer.Elapsed += sendArrayFromQueue;
            data.Client.SendMessage(JSONConvert.ConstructGameStartData(data.Lobby.ID));
        }


        private void CanvasResetLocal()
        {
            this.window.CanvasForPaint.Children.Clear();
            data.Client.SendMessage(JSONConvert.GetMessageToSend(JSONConvert.CANVAS, JSONConvert.CANVAS_RESET));
        }


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
                buffer[pos] = coordinates;
                pos++;

                window.CanvasForPaint.Children.Add(line);
                if (pos == maxLines)
                {
                    double[][] temp = new double[maxLines][];
                    for (int i = 0; i < maxLines; i++)
                    {
                        temp[i] = buffer[i];
                    }
                    linesQueue.Enqueue(temp);
                    Array.Clear(buffer, 0, buffer.Length);
                    pos = 0;
                }
                
            }
        }

        private void sendArrayFromQueue(object sender, ElapsedEventArgs e)
        {
            
            if (linesQueue.Count != 0)
            {
                Debug.WriteLine("[GAME] sending canvas data...");
                double[][] temp = linesQueue.Dequeue();
                data.Client.SendMessage(JSONConvert.GetMessageToSend(JSONConvert.CANVAS, JSONConvert.ConstructDrawingCanvasData(temp,color)));
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

        private void UpdateCanvasWithNewData(double[][] buffer, Color color)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (double[] arr in buffer)
                {
                    Line line = new Line();
                    line.Stroke = new SolidColorBrush(color);
                    line.X1 = arr[0];
                    line.Y1 = arr[1];
                    line.X2 = arr[2];
                    line.Y2 = arr[3];
                    this.window.CanvasForPaint.Children.Add(line);
                }
            });
        }

        private void CanvasResetData()
        {
            this.window.CanvasForPaint.Children.Clear();
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

        public static void HandleIncomingMsg(string username, string message)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Messages.Add($"{username}: {message}");
            });
        }
        public void LeaveGame(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("Leaving...");
            data.Client.SendMessage(JSONConvert.ConstructLobbyLeaveMessage(data.Lobby.ID));
        }


    }
}
       
