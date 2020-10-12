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

namespace Server.ViewModels
{
    class MainViewModel : ObservableObject
    {
        private ServerCommunication serverCommunication;
        public ICommand ServerStartCommand { get; set; }
        public Information InformationModel { get; set; }
        private MainWindow mainWindow;

        public MainViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            Debug.WriteLine("init mainviewmodel");
            InformationModel = new Information();
            InformationModel.CanStartServer = true;
            InformationModel.ServerOnline = false;
            
            this.ServerStartCommand = new RelayCommand(() =>
            {
                Debug.WriteLine("connect button clicked");
                if (serverCommunication == null)
                {
                    Debug.WriteLine("making new server communication");
                    serverCommunication = ServerCommunication.INSTANCE;
                }
                if (!serverCommunication.Started)
                {
                    
                    Debug.WriteLine("can start server " + InformationModel.CanStartServer);
                    serverCommunication.Start();
                    InformationModel.ServerOnline = true;
                    InformationModel.CanStartServer = false;
                }
            });
        } 
    }
}

