using Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    [TestClass]
    public class JSONConvertLobbyMessagesTests
    {
        public byte[] GetPayload(byte[] message)
        {
            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);
            return payload;
        }

        public dynamic GetDynamic(byte[] res)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(GetPayload(res)));
        }

        [TestMethod]
        public void TestLobbyHostMessage()
        {
            byte[] res = JSONConvert.ConstructLobbyHostMessage();

            dynamic payload = GetDynamic(res);
            JSONConvert.LobbyIdentifier identifier = payload.identifier;

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.HOST, identifier);
        }

        [TestMethod]
        public void TestLobbyHostCreatedMessage()
        {
            byte[] res = JSONConvert.ConstructLobbyHostCreatedMessage(3);

            dynamic payload = GetDynamic(res);
            JSONConvert.LobbyIdentifier identifier = payload.identifier;
            int id = payload.id;

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.HOST, identifier);
            Assert.AreEqual(3, id);
        }
        [TestMethod]
        public void TestLobbyRequestMessage()
        {
            byte[] res = JSONConvert.ConstructLobbyRequestMessage();
            dynamic payload = GetDynamic(res);

            JSONConvert.LobbyIdentifier identifier = payload.identifier;

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.REQUEST, identifier);

        }

        [TestMethod]
        public void TestLobbyListMessage()
        {

            Lobby[] lobbies = new Lobby[] { new Lobby(3, 0, 9), new Lobby(1, 0, 3), new Lobby(1, 0, 1) };
            byte[] res = JSONConvert.ConstructLobbyListMessage(lobbies);

            dynamic payload = GetDynamic(res);
            JSONConvert.LobbyIdentifier identifier = payload.identifier;
            JArray lobbiesJArray = payload.lobbies;
            Lobby[] lobbiesFromDynamic = lobbiesJArray.ToObject<Lobby[]>();

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.LIST, identifier);
            for (int i = 0; i < lobbies.Length; i++)
            {
                Lobby l1 = lobbies[i];
                Lobby l2 = lobbiesFromDynamic[i];
                Assert.AreEqual(l1.ID, l2.ID);
                Assert.AreEqual(l1.PlayersIn, l2.PlayersIn);
                Assert.AreEqual(l1.MaxPlayers, l2.MaxPlayers);
            }

        }

        [TestMethod]
        public void TestLobbyJoinMessage() 
        {
            byte[] res = JSONConvert.ConstructLobbyJoinMessage(8);

            dynamic payload = GetDynamic(res);
            JSONConvert.LobbyIdentifier identifier = payload.identifier;
            int id = payload.id;

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.JOIN, identifier);
            Assert.AreEqual(8, id);
        }

        [TestMethod] 
        public void TestLobbyLeaveMessage()
        {
            byte[] res = JSONConvert.ConstructLobbyLeaveMessage(4);

            dynamic payload = GetDynamic(res);
            JSONConvert.LobbyIdentifier identifier = payload.identifier;
            int id = payload.id;

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.LEAVE, identifier);
            Assert.AreEqual(4, id);
        }

        [TestMethod]
        public void TestGetLobbyIdentifier()
        {
            dynamic res = new
            {
                identifier = JSONConvert.LobbyIdentifier.REQUEST
            };
            byte[] arr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));

            JSONConvert.LobbyIdentifier lobbyIdentifier = JSONConvert.GetLobbyIdentifier(arr);

            Assert.AreEqual(JSONConvert.LobbyIdentifier.REQUEST, lobbyIdentifier);
        }

        [TestMethod]
        public void TestLobbyJoinSuccessMessage()
        {
            byte[] res = JSONConvert.ConstructLobbyJoinSuccessMessage(true);

            dynamic payload = GetDynamic(res);
            JSONConvert.LobbyIdentifier identifier = payload.identifier;
            bool host = payload.host;

            Assert.AreEqual(0x03, res[4]);
            Assert.AreEqual(JSONConvert.LobbyIdentifier.JOIN_SUCCESS, identifier);
            Assert.IsTrue(host);
        }

        [TestMethod]
        public void TestGetLobbiesFromMessage()
        {
            Lobby[] lobbiesArray = new Lobby[] { new Lobby(7, 0, 8), new Lobby(3, 0, 5), new Lobby(5, 0, 10) };
            dynamic res = new
            {
                lobbies = lobbiesArray
            };
            byte[] arr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));
            Lobby[] testLobbies = JSONConvert.GetLobbiesFromMessage(arr);

            for (int i = 0; i < lobbiesArray.Length; i++)
            {
                Lobby l1 = lobbiesArray[i];
                Lobby l2 = testLobbies[i];
                Assert.AreEqual(l1.ID, l2.ID);
                Assert.AreEqual(l1.PlayersIn, l2.PlayersIn);
                Assert.AreEqual(l1.MaxPlayers, l2.MaxPlayers);
            }
        }

        [TestMethod]
        public void TestGetLobbyID()
        {
            dynamic res = new
            {
                id = 5
            };

            byte[] arr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));
            int testID = JSONConvert.GetLobbyID(arr);

            Assert.AreEqual(5, testID);
        }

        [TestMethod]
        public void TestGetLobby()
        {
            Lobby l = new Lobby(6, 0, 9);
            dynamic res = new
            {
                lobby = l
            };
            byte[] arr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));

            Lobby testLobby = JSONConvert.GetLobby(arr);

            Assert.AreEqual(l.ID, testLobby.ID);
            Assert.AreEqual(l.MaxPlayers, testLobby.MaxPlayers);
            Assert.AreEqual(l.PlayersIn, testLobby.PlayersIn);
        }

       


    }
}
