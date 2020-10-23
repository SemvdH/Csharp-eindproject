using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SharedClientServer
{
    public delegate void Callback();


    class ClientServerUtil
    {
        // creates a message array to send to the server or to clients
        public byte[] createPayload(byte id, string payload)
        {
            byte[] stringAsBytes = Encoding.ASCII.GetBytes(payload);
            byte[] res = new byte[stringAsBytes.Length + 1];
            res[0] = id;
            Array.Copy(stringAsBytes, 0, res, 1, stringAsBytes.Length);
            return res;
        }
    }
}
