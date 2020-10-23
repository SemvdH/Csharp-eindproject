using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    [TestClass]
    public class JSONConvertRandomWord
    {
        public byte[] GetPayload(byte[] message)
        {
            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);
            return payload;
        }

        public byte[] RandomWord()
        {
            byte identifier = 0x07;
            dynamic payload = new
            {
                word = "teacher"
            };

            byte[] res = JSONConvert.GetMessageToSend(identifier, payload);

            return res;
        }

        public dynamic GetDynamic(byte[] payload)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(GetPayload(payload)));
        }

        [TestMethod]
        public void TestSendRandomWord()
        {
            string randomWord = JSONConvert.SendRandomWord("WordsForGame.json");

            string result = "teacher";
            
            Assert.AreEqual(result, randomWord);
        }

        [TestMethod]
        public void TestGetRandomWord()
        {
            byte[] data = GetPayload(RandomWord());
            string result = JSONConvert.GetRandomWord(data);

            string word = "teacher";

            Assert.AreEqual(word, result);
        }
    }
}
