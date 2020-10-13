using GalaSoft.MvvmLight.Command;
using Server.Models;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Server.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public ServerCommunication serverCommunication { get; set; }
        public ICommand ServerStartCommand { get; set; }
        public Information InformationModel { get; set; }
        private MainWindow mainWindow;

        public MainViewModel(MainWindow mainWindow)
        {
            serverCommunication = ServerCommunication.INSTANCE;
            this.mainWindow = mainWindow;
            Debug.WriteLine("init mainviewmodel");
            InformationModel = new Information();
            InformationModel.CanStartServer = true;
            InformationModel.ServerOnline = false;
            InformationModel.ClientsConnected = 0;
            serverCommunication.newClientAction = () =>
            {
                InformationModel.ClientsConnected++;
            };
            //BitmapImage onlineImg = new BitmapImage(new Uri(@"/img/online.png",UriKind.Relative));
            //BitmapImage offlineImg = new BitmapImage(new Uri(@"/img/offline.png", UriKind.Relative));
            
            this.ServerStartCommand = new RelayCommand(() =>
            {
                Debug.WriteLine("connect button clicked");
                
                if (!serverCommunication.Started)
                {
                    serverCommunication.Start();
                    InformationModel.ServerOnline = true;
                    InformationModel.CanStartServer = false;
                }
            });
        } 
    }
}

