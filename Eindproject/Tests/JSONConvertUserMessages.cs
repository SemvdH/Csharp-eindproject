using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    [TestClass]
    public class JSONConvertUserMessages
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

        private byte[] ComboArray()
        {
            byte identifier = 0x02;
            dynamic payload = new
            {
                username = "testName",
                message = "message"
            };

            byte[] result = JSONConvert.GetMessageToSend(identifier, payload);

            return result;
        }

        private byte[] LoginArray()
        {
            byte identifier = 0x01;
            dynamic payload = new
            {
                username = "testname"
            };

            byte[] result = JSONConvert.GetMessageToSend(identifier, payload);

            return result;
        }

        [TestMethod]
        public void TestGetUsernameAndMessage()
        {
            byte[] data = GetPayload(ComboArray());
            (string,string) result = JSONConvert.GetUsernameAndMessage(data);

            (string, string) testCombo = ("testName", "message");

            Assert.AreEqual(testCombo, result);
        }

        [TestMethod]
        public void TestGetUsernameLogin()
        {
            byte[] data = GetPayload(LoginArray());
            string result = JSONConvert.GetUsernameLogin(data);
            string username = "testname";

            Assert.AreEqual(username, result);
        }

        [TestMethod]
        public void TestConstructUsernameMessage()
        {
            byte[] res = JSONConvert.ConstructUsernameMessage("testname");
            dynamic payload = GetDynamic(res);

            string username = "testname";

            Assert.AreEqual(0x01, res[4]);
            Assert.AreEqual(username, (string)payload.username);
        }



    }
}
