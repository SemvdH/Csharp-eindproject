using Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace SharedClientServer
{
    public class JSONConvert
    {
        public const byte LOGIN = 0x01;
        public const byte MESSAGE = 0x02;
        public const byte LOBBY = 0x03;
        public const byte CANVAS = 0x04;
        public const byte GAME = 0x05;
        public const byte MESSAGE_RECEIVED = 0x06;
        public const byte RANDOMWORD = 0x07;

        public const int CANVAS_WRITING = 0;
        public const int CANVAS_RESET = 1;

        public enum LobbyIdentifier
        {
            HOST,
            JOIN,
            JOIN_SUCCESS,
            LEAVE,
            LIST,
            REQUEST
        }

        public enum GameCommand
        {
            START_GAME,
            TIMER_ELAPSED,
            NEXT_ROUND
        }

        public static (string, string) GetUsernameAndMessage(byte[] json)
        {
            string msg = Encoding.UTF8.GetString(json);
            dynamic payload = JsonConvert.DeserializeObject(msg);

            return (payload.username, payload.message);
        }

        internal static byte[] ConstructCanvasResetMessage()
        {
            return GetMessageToSend(CANVAS, new
            {
                canvasType = CANVAS_RESET
            });
        }

        public static string GetUsernameLogin(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
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
            });
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
            dynamic payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
            return payload.identifier;
        }

        public static Lobby[] GetLobbiesFromMessage(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
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
            dynamic payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
            return payload.id;
        }

        public static Lobby GetLobby(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
            JObject dynamicAsObject = payload.lobby;
            return dynamicAsObject.ToObject<Lobby>();
        }

        public static byte[] ConstructLobbyJoinSuccessMessage(bool isHost)
        {
            return GetMessageToSend(LOBBY, new
            {
                identifier = LobbyIdentifier.JOIN_SUCCESS,
                host = isHost
            });
        }

        public static bool GetLobbyJoinIsHost(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
            return payload.host;
        }

        #endregion lobby messages

        public static byte[] ConstructCanvasDataSend(int typeToSend, double[][] buffer, Color colorToSend)
        {
            return GetMessageToSend(CANVAS, new
            {
                canvasType = typeToSend,
                coords = buffer,
                color = colorToSend
            }); ;
        }

        public static byte[] ConstructDrawingCanvasData(double[][] buffer, Color colorToSend)
        {
            return GetMessageToSend(CANVAS, new
            {
                canvasType = CANVAS_WRITING,
                coords = buffer,
                color = colorToSend
            });
        }

        public static byte[] ConstructCanvasReset()
        {
            dynamic payload = new
            {
                canvasType = CANVAS_RESET
            };
            return GetMessageToSend(CANVAS, payload);
        }

        public static int GetCanvasMessageType(byte[] json)
        {
            dynamic d = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
            return d.canvasType;
        }

        public static double[][] getCoordinates(byte[] payload)
        {
            Debug.WriteLine("got coords " + Encoding.UTF8.GetString(payload));
            dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload));
            JArray coordinatesArray = json.coords;

            double[][] coordinates = coordinatesArray.ToObject<double[][]>();

            return coordinates;
        }

        public static Color getCanvasDrawingColor(byte[] payload)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload));
            Color color = json.color;
            return color;
        }

        public static byte[] ConstructGameStartData(int lobbyID)
        {
            return GetMessageToSend(GAME, new
            {
                command = GameCommand.START_GAME,
                lobbyToStart = lobbyID
            }); ;
        }

        public static byte[] ConstructGameTimerElapsedMessage(int lobbyID)
        {
            return GetMessageToSend(GAME, new
            {
                command = GameCommand.TIMER_ELAPSED,
                id = lobbyID
            });
        }

        public static GameCommand GetGameCommand(byte[] payload)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload));
            return json.command;
        }

        public static int GetStartGameLobbyID(byte[] payload)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload));
            return json.lobbyToStart;
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
            string json = JsonConvert.SerializeObject(payload);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(json);
            // make the array that holds the message and copy the payload into it with the first spot containing the identifier
            byte[] res = new byte[payloadBytes.Length + 5];
            // put the payload in the res array
            Array.Copy(payloadBytes, 0, res, 5, payloadBytes.Length);
            // put the identifier at the start of the payload part
            res[4] = identifier;
            // put the length of the payload at the start of the res array
            Array.Copy(BitConverter.GetBytes(payloadBytes.Length + 5), 0, res, 0, 4);
            return res;
        }

        /*
    * This method sends a random word from the json file, this happens when the client joins a lobby.
    */

        public static string SendRandomWord(string filename)
        {
            dynamic words;
            Random random = new Random();
            string workingDir = Path.GetFullPath(@"..\Server");
            string projDir = Directory.GetParent(workingDir).Parent.Parent.FullName;
            string filePath = projDir += $@"\resources\{filename}";

            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                words = JsonConvert.DeserializeObject(json);
            }

            int index = random.Next(0, 24);

            Debug.WriteLine($"[SERVERCLIENT] Sending random words {words}");

            return words.words[0];
        }

        /*
         * Client gets the payload and retrieves the word from the payload
         */

        public static string GetRandomWord(byte[] json)
        {
            dynamic payload = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json));
            return payload.word;
        }
    }
}