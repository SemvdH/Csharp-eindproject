using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    [TestClass]
    public class JSONConvertGameCommand
    {
        public byte[] GetPayload(byte[] message)
        {
            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);
            return payload;
        }

        public dynamic GetDynamic(byte[] payload)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(GetPayload(payload)));
        }

        public byte[] GameCommandMessage()
        {
            byte identifier = 0x05;
            dynamic payload = new
            {
                command = JSONConvert.GameCommand.NEXT_ROUND
            };

            byte[] res = JSONConvert.GetMessageToSend(identifier, payload);

            return res;
        }
        
        public byte[] GameLobbyID()
        {
            byte identifier = 0x05;
            dynamic payload = new
            {
                lobbyToStart = 1
            };

            byte[] res = JSONConvert.GetMessageToSend(identifier, payload);

            return res;
        }

        [TestMethod]
        public void TestConstructGameStartData()
        {
            byte[] lobbyData = JSONConvert.ConstructGameStartData(1);
            dynamic payload = GetDynamic(lobbyData);

            int lobbyid = 1;
            JSONConvert.GameCommand gameCommand = payload.command;

            Assert.AreEqual(0x05, lobbyData[4]);
            Assert.AreEqual(JSONConvert.GameCommand.START_GAME, gameCommand);
            Assert.AreEqual(lobbyid, (int)payload.lobbyToStart);
        }

        [TestMethod]
        public void TestConstructGameTimerElapsedMessage()
        {
            byte[] lobbyData = JSONConvert.ConstructGameTimerElapsedMessage(1);
            dynamic payload = GetDynamic(lobbyData);

            int lobbyid = 1;
            JSONConvert.GameCommand gameCommand = payload.command;

            Assert.AreEqual(0x05, lobbyData[4]);
            Assert.AreEqual(JSONConvert.GameCommand.TIMER_ELAPSED, gameCommand);
            Assert.AreEqual(lobbyid, (int)payload.id);
        }

        [TestMethod]
        public void TestGetGameCommand()
        {
            byte[] data = GetPayload(GameCommandMessage());
            JSONConvert.GameCommand gameCommand = JSONConvert.GetGameCommand(data);

            Assert.AreEqual(JSONConvert.GameCommand.NEXT_ROUND, gameCommand);

        }

        [TestMethod]
        public void TestGetStartGameLobbyID()
        {
            byte[] data = GetPayload(GameLobbyID());
            int lobbyID = JSONConvert.GetStartGameLobbyID(data);

            int expected = 1;

            Assert.AreEqual(expected, lobbyID);
        }
    }
}
