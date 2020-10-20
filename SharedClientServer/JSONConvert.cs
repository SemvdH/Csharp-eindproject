using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClientServer
{
    class JSONConvert
    {
        public const byte LOGIN = 0x01;
        public const byte MESSAGE = 0x02;
        public const byte LOBBY = 0x03;
        public const byte CANVAS = 0x04;
        public static (string,string) GetUsernameAndMessage(byte[] json)
        {
            string msg = Encoding.ASCII.GetString(json);
            dynamic payload = JsonConvert.DeserializeObject(msg);
            
            return (payload.username, payload.message);
        }

        public static string GetUsernameLogin(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            return payload.username;
        }

        public static byte[] GetUsernameMessage(string uName)
        {
            return GetMessageToSend(LOGIN, new
            {
                username = uName
            });
        }

        /// <summary>
        /// constructs a message that can be sent to the clients or server
        /// </summary>
        /// <param name="identifier">the identifier for what kind of message it is</param>
        /// <param name="payload">the json payload</param>
        /// <returns>a byte array containing a message that can be sent to clients or server</returns>
        public static byte[] GetMessageToSend(byte identifier, dynamic payload)
        {
            // convert the dynamic to bytes
            byte[] payloadBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(payload));
            // make the array that holds the message and copy the payload into it with the first spot containing the identifier
            byte[] res = new byte[payloadBytes.Length + 1];
            Array.Copy(payloadBytes, 0, res, 1, payloadBytes.Length);
            res[0] = identifier;
            return res;
        }
    }
}
