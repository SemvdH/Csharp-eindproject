using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace Tests
{
    [TestClass]
    public class JSONConvertCanvasMessages
    {
        //Helper method for the tests
        public byte[] GetPayload(byte[] message)
        {
            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);
            return payload;
        }

        public dynamic GetDynamic(byte[] payload)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload));
            return json;
        }

        [TestMethod]
        public void TestConstructCanvasDataSend()
        {
            int type = JSONConvert.CANVAS_WRITING;
            double[][] coordinateInfo = new double[2][];
            double[] coordinatesOne = { 10.0, 10.0, 3.0, 3.0 };
            double[] coordinatesTwo = { 10.0, 10.0, 3.0, 3.0 };
            coordinateInfo[0] = coordinatesOne;
            coordinateInfo[1] = coordinatesTwo;
            Color color = Color.FromRgb(0, 0, 0);

            byte[] message = JSONConvert.ConstructCanvasDataSend(type, coordinateInfo, color);
            byte[] payload = GetPayload(message);
            dynamic json = GetDynamic(payload);
            int ID = json.canvasType;
            JArray coorArray = json.coords;
            double[][] coordinates = coorArray.ToObject<double[][]>();
            Color colorResult = json.color;

            Assert.AreEqual(0x04, message[4]);
            Assert.AreEqual(type, ID, "The canvas type message is not correct on the ConstructDrawingCanvasData");
            for (int i = 0; i < coordinateInfo.Length; i++)
            {
                CollectionAssert.AreEqual(coordinateInfo[i], coordinates[i], "Coordinates are not correct on the ConstructDrawingCanvasData");
            }
            Assert.AreEqual(color, colorResult, "color is not correct on the ConstructDrawingCanvasData");
        }

        [TestMethod]
        public void TestConstructDrawingCanvasData()
        {
            double[][] coordinateInfo = new double[2][];
            double[] coordinatesOne = {10.0, 10.0, 3.0, 3.0 };
            double[] coordinatesTwo = { 10.0, 10.0, 3.0, 3.0 };
            coordinateInfo[0] = coordinatesOne;
            coordinateInfo[1] = coordinatesTwo;
            Color color = Color.FromRgb(0, 0, 0);

            byte[] message = JSONConvert.ConstructDrawingCanvasData(coordinateInfo, color);
            byte[] payload = GetPayload(message);
            dynamic json = GetDynamic(payload);
            int ID = json.canvasType;
            JArray coorArray = json.coords;
            double[][] coordinates = coorArray.ToObject<double[][]>();
            Color colorResult = json.color;

            Assert.AreEqual(0x04, message[4]);
            Assert.AreEqual(JSONConvert.CANVAS_WRITING, ID, "The canvas type message is not correct on the ConstructDrawingCanvasData");
            for (int i = 0; i < coordinateInfo.Length; i++)
            {
                CollectionAssert.AreEqual(coordinateInfo[i], coordinates[i], "Coordinates are not correct on the ConstructDrawingCanvasData");
            }
            Assert.AreEqual(color, colorResult, "color is not correct on the ConstructDrawingCanvasData");
        }

        [TestMethod]
        public void TestConstructCanvasReset()
        {
          
            dynamic payload = new
            {

            };

        }

        [TestMethod]
        public void TestConstructCanvasResetMessage()
        {
            byte identifier = 0x04;
            dynamic payload = new
            {

            };

        }

        [TestMethod]
        public void TestGetCanvasMessageType()
        {
            byte identifier = 0x04;
            dynamic payload = new
            {

            };

        }

        [TestMethod]
        public void TestgetCoordinates()
        {
            byte identifier = 0x04;
            dynamic payload = new
            {

            };

        }

        [TestMethod]
        public void TestGetCanvasDrawingColor()
        {
            byte identifier = 0x04;
            dynamic payload = new
            {

            };

        }
    }
}
