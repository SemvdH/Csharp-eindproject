using Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharedClientServer
{
    class JSONConvert
    {
        public const byte LOGIN = 0x01;
        public const byte MESSAGE = 0x02;
        public const byte LOBBY = 0x03;
        public const byte CANVAS = 0x04;

        public enum LobbyIdentifier
        {
            HOST,
            JOIN,
            JOIN_SUCCESS,
            LEAVE,
            LIST,
            REQUEST
        }
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

        #region lobby messages

        public static byte[] ConstructLobbyHostMessage()
        {
            return GetMessageToSend(LOBBY, new
            {
                identifier = LobbyIdentifier.HOST
            });
        }

        public static byte[] ConstructLobbyHostCreatedMessage(int lobbyID)
        {
            return GetMessageToSend(LOBBY, new
            {
                identifier = LobbyIdentifier.HOST,
                id = lobbyID
            }) ;
        }

        public static byte[] ConstructLobbyRequestMessage()
        {
            return GetMessageToSend(LOBBY, new
            {
                identifier = LobbyIdentifier.REQUEST
            });
        }

        public static byte[] ConstructLobbyListMessage(Lobby[] lobbiesList)
        {
            return GetMessageToSend(LOBBY, new
            { 
                identifier = LobbyIdentifier.LIST,
                lobbies = lobbiesList
            });
        }

        public static byte[] ConstructLobbyJoinMessage(int lobbyID)
        {
            return GetMessageToSend(LOBBY, new
            {
                identifier = LobbyIdentifier.JOIN,
                id = lobbyID
            });
        }

        public static byte[] ConstructLobbyLeaveMessage(int lobbyID)
        {
            return GetMessageToSend(LOBBY, new
            {
                identifier = LobbyIdentifier.LEAVE,
                id = lobbyID
            });
        }
        public static LobbyIdentifier GetLobbyIdentifier(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            return payload.identifier;
        }

        public static Lobby[] GetLobbiesFromMessage(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            JArray lobbiesArray = payload.lobbies;
            Debug.WriteLine("[JSONCONVERT] got lobbies from message" + lobbiesArray.ToString());
            Lobby[] lobbiesTemp = lobbiesArray.ToObject<Lobby[]>();
            Debug.WriteLine("lobbies in array: ");
            foreach (Lobby l in lobbiesTemp)
            {
                Debug.WriteLine("players: " + l.PlayersIn);
            }
            return lobbiesTemp;
        }

        public static int GetLobbyID(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            return payload.id;
        }

        public static Lobby GetLobby(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            JObject dynamicAsObject = payload.lobby;
            return dynamicAsObject.ToObject<Lobby>();
        }

        public static byte[] ConstructLobbyJoinSuccessMessage(bool isHost)
        {
            return GetMessageToSend(LOBBY, new { identifier = LobbyIdentifier.JOIN_SUCCESS,
            host = isHost});
        }

        public static bool GetLobbyJoinIsHost(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            return payload.host;
        }

        #endregion
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
