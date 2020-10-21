using SharedClientServer;

namespace Server.Models
{
    public class Information : ObservableObject
    {
        
        public bool CanStartServer { get; set; }
        public bool ServerOnline { get; set; }

        public string ServerStatus
        {
            get
            {
                if (ServerOnline) return "Online";
                return "Offline";
            }
        }

        public int ClientsConnected{ get; set; }
    }
}