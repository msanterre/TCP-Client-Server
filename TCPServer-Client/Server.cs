using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TCPServer_Client
{
    class Server
    {
        TcpListener listener;
        Dictionary<Guid, TcpClient> clients;
        Dictionary<Guid, string> clientNames;
        Encoding utf8;
        private const int BUF_LENGTH = 1024;

        bool alive;


        public Server(IPAddress addr, int port)
        {
            listener = new TcpListener(addr, port);
            clients = new Dictionary<Guid, TcpClient>();
            clientNames = new Dictionary<Guid, string>();
            alive = true;
            utf8 = UTF8Encoding.UTF8;
        }

        public void startListening()
        {
            listener.Start();
            while (alive)
            {
                TcpClient client = listener.AcceptTcpClient();
                Guid clientGuid = Guid.NewGuid();
                Console.WriteLine(String.Format("Client {1} connected from: {0}", client.Client.LocalEndPoint.ToString(), clientGuid.ToString()));
                    
                try
                {
                    clients.Add(clientGuid, client);

                    if (client != null)
                    {
                        NetworkStream stream = client.GetStream();
                        String name = getMessage(stream);
                        clientNames.Add(clientGuid, name);
                        Console.WriteLine(String.Format("Client {0} is named {1}", clientGuid, name));

                        Thread readingTh = new Thread(() => 
                        {
                            while (client.Connected)
                            {
                                String message = getMessage(stream);
                                Console.WriteLine(String.Format("Client {0} says: {1}", clientNames[clients.Where(x => x.Value == client).First().Key], message));
                                publishMessage(message, client);
                            }                                
                        });
                        readingTh.Start();
                    }
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(String.Format("Client {0} disconnected", clientGuid.ToString()));
                }
            }
        }
        private String getMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[BUF_LENGTH];
            stream.Read(buffer, 0, BUF_LENGTH);
            String message = utf8.GetString(buffer).Trim('\0').Trim();
            return message;
        }

        private void publishMessage(String message, TcpClient client)
        {
            foreach (TcpClient cli in clients.Values)
            {
                if (cli != client)
                {
                    var clsStream = cli.GetStream();
                    byte[] msgBytes = utf8.GetBytes(message);
                    clsStream.Write(msgBytes, 0, msgBytes.Length);
                }
            }
        }
    }
}
