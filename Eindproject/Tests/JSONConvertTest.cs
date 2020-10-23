using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharedClientServer;
using System.Text;
using Xunit.Sdk;

namespace Tests
{
    
    [TestClass]
    public class JSONConvertTest
    {
        
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
            byte identifier = 0x01;
            dynamic payload = new
            {
                value = "test"
            };

            byte[] result = JSONConvert.GetMessageToSend(identifier, payload);
            Assert.AreEqual(0x01, result[4]);
        }
    }
}
