using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SharedClientServer
{
    class ClientServerUtil
    {

        public static Encoding encoding = Encoding.UTF8;

        public static string ReadTextMessage(NetworkStream networkStream)
        {
            var stream = new StreamReader(networkStream, encoding);
            {
                return stream.ReadLine();
            }
        }

        public static void WriteTextMessage(NetworkStream networkStream, string message)
        {
            using (var stream = new StreamWriter(networkStream, encoding, -1, true))
            {
                stream.WriteLine(message);
                stream.Flush();
            }
        }


        public static string ReadMessage(NetworkStream networkStream)
        {
            byte[] lengthBytes = new byte[4];

            networkStream.Read(lengthBytes, 0, 4);
            Console.WriteLine("read message..");

            int length = BitConverter.ToInt32(lengthBytes);

            byte[] buffer = new byte[length];
            int totalRead = 0;


            do
            {
                int read = networkStream.Read(buffer, totalRead, buffer.Length - totalRead);
                totalRead += read;
            } while (totalRead < length);

            return Encoding.UTF8.GetString(buffer, 0, totalRead);

        }

        public static void SendMessage(NetworkStream networkStream, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);

            byte[] res = new byte[data.Length + 4];

            Array.Copy(BitConverter.GetBytes(data.Length), 0, res, 0, 4);
            Array.Copy(data, 0, res, 4, data.Length);

            networkStream.Write(res);
        }



    }
}
