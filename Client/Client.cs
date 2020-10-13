using SharedClientServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Client : ObservableObject
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private byte[] totalBuffer = new byte[1024];
        private int totalBufferReceived = 0;
        public int Port = 5555;
        public bool Connected = false;
        //TODO send login packet to server with ClientServerUtil.createpayload(0x01,dynamic json with username)
        public string username;

        public Client()
        {
            this.tcpClient = new TcpClient();
            tcpClient.BeginConnect("localhost", Port, new AsyncCallback(OnConnect), null);
        }

        private void OnConnect(IAsyncResult ar)
        {
            this.tcpClient.EndConnect(ar);
            this.stream = tcpClient.GetStream();
            this.stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete),null);
        }

        private void OnReadComplete(IAsyncResult ar)
        {
            int amountReceived = stream.EndRead(ar);

            if (totalBufferReceived + amountReceived > 1024)
            {
                throw new OutOfMemoryException("buffer too small");
            }

            Array.Copy(buffer, 0, totalBuffer, totalBufferReceived, amountReceived);
            totalBufferReceived += amountReceived;

            int expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);

            while (totalBufferReceived >= expectedMessageLength)
            {
                // we have received the complete packet
                byte[] message = new byte[expectedMessageLength];
                // put the message received into the message array
                Array.Copy(totalBuffer, 0, message, 0, expectedMessageLength);

                handleData(message);

                totalBufferReceived -= expectedMessageLength;
                expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
            }

            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadComplete), null);

        }

        private void handleData(byte[] message)
        {
            byte id = message[0];
            byte[] payload = new byte[message.Length - 1];
            Array.Copy(message, 1, payload, 0, message.Length - 1);
            switch (id)
            {
                case 0x01:
                    // json log in username data
                    break;
                case 0x02:
                    // json message data
                    (string, string) combo = JSONConvert.GetUsernameAndMessage(payload);
                    string textUsername = combo.Item1;
                    string textMsg = combo.Item2;
                    //TODO display username and message in chat window
                    break;

                case 0x03:
                    // lobby data
                    //TODO fill lobby with the data received
                    break;
                case 0x04:
                    // canvas data
                    break;
                default:
                    Debug.WriteLine("[CLIENT] Received weird identifier: " + id);
                    break;
            }

        }

        public void SendMessage(byte[] message)
        {
            stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWriteComplete), null);
        }

        private void OnWriteComplete(IAsyncResult ar)
        {
            stream.EndWrite(ar);
        }
    }
}
