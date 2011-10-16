using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TCPServer_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Server chatServer = new Server(IPAddress.Any, 10100);
            chatServer.startListening();
        }
    }
}
