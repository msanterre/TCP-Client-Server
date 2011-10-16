using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TCPClient
{
    class Program
    {
        private static bool alive = true;
        const int BUF_LENGTH = 1024;
        private static bool nameSuccess;
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("localhost", 10100);
            var stream = client.GetStream();
            Encoding enc = Encoding.UTF8;

            Thread listTh = new Thread(() =>
            {
                while (alive)
                {
                    byte[] buffer = new byte[BUF_LENGTH];
                    stream.Read(buffer, 0, BUF_LENGTH);
                    String message = enc.GetString(buffer).Trim('\0').Trim();
                    if (!String.IsNullOrEmpty(message))
                    {
                        Console.WriteLine(String.Format("Server says: {0}", message));
                    }
                }
            });
            listTh.Start();

            while (!nameSuccess)
            {
                Console.WriteLine("Choose your name:");
                var name = Console.ReadLine();
                if (String.IsNullOrEmpty(name))
                    continue;

                sendMessage(stream, name);
                nameSuccess = true;                
            }
            while (alive)
            {
                Console.WriteLine("Enter your text: ");
                var message = Console.ReadLine();
                if (message.Length > BUF_LENGTH)
                { Console.WriteLine(String.Format("Message too large to send, must be within {0} characters", BUF_LENGTH)); continue; }

                if (message == "quit")
                {
                    alive = false;
                    break;
                }
                sendMessage(stream, message);
            }
            client.Close();
        }

        static private void sendMessage(NetworkStream stream, string message)
        {
            byte[] textBytes = UTF8Encoding.UTF8.GetBytes(message);
            stream.Write(textBytes, 0, textBytes.Length);
        }
    }
}
