

using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Models
{
    class Information : ObservableObject
    {
        public bool CanStartServer { get; set; }
        public bool ServerOnline { get; set; }
        public string ServerStatus { get {
               if (ServerOnline) return "Online";
               return "Offline";
            } 
        }
    }
}
