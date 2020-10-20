using Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static byte[] ConstructUsernameMessage(string uName)
        {
            return GetMessageToSend(LOGIN, new
            {
                username = uName
            });
        }

        public static byte[] ConstructLobbyDataMessage(Lobby lobby)
        {
            return null;
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
            byte[] res = new byte[payloadBytes.Length + 5];
            // put the payload in the res array
            Array.Copy(payloadBytes, 0, res, 5, payloadBytes.Length);
            // put the identifier at the start of the payload part
            res[4] = identifier;
            // put the length of the payload at the start of the res array
            Array.Copy(BitConverter.GetBytes(payloadBytes.Length+5),0,res,0,4);
            return res;
        }
    }
}
