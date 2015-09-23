using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErgometerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            IPAddress localhost; //= IPAddress.Parse("127.0.0.1");

            bool ipIsOk = IPAddress.TryParse("127.0.0.1", out localhost);
            if (!ipIsOk) { Console.WriteLine("ip adres kan niet geparsed worden."); Environment.Exit(1); }

            TcpListener listener = new System.Net.Sockets.TcpListener(localhost, 80);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for connection with client");

                //AcceptTcpClient waits for a connection from the client
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Connection established");

                Thread thread = new Thread(HandleClientThread);
                thread.Start(client);
            }
        }

        static void HandleClientThread(object obj)
        {
            TcpClient client = obj as TcpClient;
            StreamReader reader = new StreamReader(client.GetStream(), Encoding.ASCII);
            StreamWriter stream = new StreamWriter(client.GetStream(), Encoding.ASCII);
            string q = reader.ReadLine();
            if(q == "1«")
            Console.WriteLine(q);

            stream.Flush();
            stream.Close();
            reader.Close();
        }
    }
}
