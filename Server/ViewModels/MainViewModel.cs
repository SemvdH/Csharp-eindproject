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

namespace Server.ViewModels
{
    class MainViewModel : ObservableObject
    {
        private ServerCommunication serverCommunication;
        public ICommand ServerStartCommand { get; set; }
        public Information InformationModel { get; set; }

        public MainViewModel()
        {
            Debug.WriteLine("init mainviewmodel");
            InformationModel = new Information();
            InformationModel.CanStartServer = true;
            this.ServerStartCommand = new RelayCommand(() =>
            {
                Debug.WriteLine("connect button clicked");
                if (serverCommunication == null)
                {
                    Debug.WriteLine("making new server communication");
                    serverCommunication = new ServerCommunication(new TcpListener(IPAddress.Any,5555));
                }
                if (!serverCommunication.Started)
                {
                    
                    Debug.WriteLine("can start server " + InformationModel.CanStartServer);
                    serverCommunication.Start();
                    
                    InformationModel.CanStartServer = false;
                }
            });
        } 
    }
}

