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
            byte[] canvasReset = JSONConvert.ConstructCanvasReset();
            dynamic json = GetDynamic(GetPayload(canvasReset));
            int ID = json.canvasType;

            Assert.AreEqual(JSONConvert.CANVAS_RESET, ID, $"Canvas type should be reset(1)! Not, {ID}");
        }

        [TestMethod]
        public void TestGetCanvasMessageType()
        {
            int type = JSONConvert.CANVAS_WRITING;
            byte IDsend = JSONConvert.CANVAS;
           
            dynamic payloadSend = new
            {
                canvasType = type
            };
            byte[] message = JSONConvert.GetMessageToSend(IDsend, payloadSend);
            byte[] payload = GetPayload(message);
            int resultID = JSONConvert.GetCanvasMessageType(payload);

            Assert.AreEqual(type, resultID, $"Canvas type should be {IDsend}! Not, {resultID}");
        }

        [TestMethod]
        public void TestgetCoordinates()
        {
            int type = JSONConvert.CANVAS_WRITING;
            byte IDsend = JSONConvert.CANVAS;
            double[][] coordinateInfo = new double[2][];
            double[] coordinatesOne = { 10.0, 10.0, 3.0, 3.0 };
            double[] coordinatesTwo = { 10.0, 10.0, 3.0, 3.0 };
            coordinateInfo[0] = coordinatesOne;
            coordinateInfo[1] = coordinatesTwo;
            dynamic payloadSend = new
            {
                canvasType = type,
                coords = coordinateInfo
            };
            byte[] message = JSONConvert.GetMessageToSend(IDsend, payloadSend);
            byte[] payload = GetPayload(message);

            double[][] coordinates = JSONConvert.getCoordinates(payload);

            for (int i = 0; i < coordinateInfo.Length; i++)
            {
                CollectionAssert.AreEqual(coordinateInfo[i], coordinates[i], "Coordinates are not correct on the ConstructDrawingCanvasData");
            }
        }

        [TestMethod]
        public void TestGetCanvasDrawingColor()
        {
            int type = JSONConvert.CANVAS_WRITING;
            byte IDsend = JSONConvert.CANVAS;
            Color colorSend = Color.FromRgb(0, 0, 0);
            dynamic payloadSend = new
            {
                canvasType = type,
                color = colorSend
            };
            byte[] message = JSONConvert.GetMessageToSend(IDsend, payloadSend);
            byte[] payload = GetPayload(message);

            Color colorResult = JSONConvert.getCanvasDrawingColor(payload);

            Assert.AreEqual(colorSend, colorResult, "Colors are not equal!");
        }

    }
}
