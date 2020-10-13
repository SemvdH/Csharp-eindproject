using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClientServer
{
    class JSONConvert
    {
        public static (string,string) GetUsernameAndMessage(byte[] json)
        {
            string msg = Encoding.ASCII.GetString(json);
            dynamic payload = JsonConvert.DeserializeObject(msg);
            return (payload.username, payload.message);
        }
    }
}
