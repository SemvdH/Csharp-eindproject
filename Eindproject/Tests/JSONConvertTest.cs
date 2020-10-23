using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharedClientServer;
using System;
using System.Diagnostics;
using System.Text;
using Xunit.Sdk;

namespace Tests
{
    
    [TestClass]
    public class JSONConvertTest
    {

        private byte[] testArray1()
        {
            byte identifier = 0x01;
            dynamic payload = new
            {
                value = "test"
            };

            byte[] result = JSONConvert.GetMessageToSend(identifier, payload);
            return result;
        }
        
        [TestMethod]
        public void TestGetMessageToSendLength()
        {
            
            byte identifier = 0x01;
            dynamic payload = new
            {
                value = "test"
            };
            string payloadToJson = JsonConvert.SerializeObject(payload);
            byte[] payloadToBytes = Encoding.UTF8.GetBytes(payloadToJson);

            byte[] result = JSONConvert.GetMessageToSend(identifier, payload);
            Assert.AreEqual(payloadToBytes.Length + 5, result.Length);
        }

        [TestMethod]
        public void TestGetMessageToSendIdentifier()
        {
            byte[] result = testArray1();
            Assert.AreEqual(0x01, result[4]);
        }

        [TestMethod]
        public void TestGetMessageToSendMessageLength()
        {

            byte identifier = 0x01;
            dynamic payload = new
            {
                value = "test"
            };

            string json = JsonConvert.SerializeObject(payload);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(json);
            byte[] res = new byte[payloadBytes.Length + 5];
            Array.Copy(payloadBytes, 0, res, 5, payloadBytes.Length);
            res[4] = identifier;
            Array.Copy(BitConverter.GetBytes(payloadBytes.Length + 5), 0, res, 0, 4);
            int lengthTest = BitConverter.ToInt32(res, 0);
            byte[] result = testArray1();
            int lengthResult = BitConverter.ToInt32(result, 0);

            Assert.AreEqual(lengthTest, lengthResult);

        }


    }
}
