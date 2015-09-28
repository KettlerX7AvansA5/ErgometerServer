using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ErgometerLibrary;

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
            FileHandler.CheckDataFolder();
            IPAddress ipAddress; //= IPAddress.Parse("127.0.0.1");

            bool ipIsOk = IPAddress.TryParse("127.0.0.1", out ipAddress); //GetIp()
            if (!ipIsOk) { Console.WriteLine("ip adres kan niet geparsed worden."); Environment.Exit(1); }

            TcpListener listener = new System.Net.Sockets.TcpListener(ipAddress, 8888);
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

        public static String GetIp()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        static void HandleClientThread(object obj)
        {
            TcpClient client = obj as TcpClient;
            StreamReader reader = new StreamReader(client.GetStream(), Encoding.Unicode);
            StreamWriter stream = new StreamWriter(client.GetStream(), Encoding.Unicode);
            string name = null;
            int session = 0;
            bool doctor = false;
            string b = reader.ReadLine();
            ArrayList data = new ArrayList();
            while (!b.StartsWith("4»") && !b.EndsWith("logout"))
            {
                if (b.StartsWith("5»"))
                {
                    session = FileHandler.GenerateSession();
                    NetCommand sescommand = new NetCommand(NetCommand.CommandType.SESSION, session);
                    Console.WriteLine(b);
                    Console.WriteLine(sescommand.ToString());
                    stream.WriteLine(sescommand.ToString());
                    stream.Flush();
                }
                if(b.StartsWith("1»"))
                {
                    NetCommand clientCommand = NetCommand.Parse(b);
                    doctor = clientCommand.IsDoctor;
                    name = clientCommand.DisplayName;
                    if (doctor)
                    {
                        //check password
                    }
                    else
                    {
                        Console.WriteLine("login of " + name + " succesful");
                        stream.WriteLine("login succes"); //moet nog geïmplementeerd worden
                    }
                    stream.Flush();
                }
                if (b.StartsWith("2»"))
                {
                    Console.WriteLine(b);
                    data.Add(NetCommand.Parse(b));
                    stream.WriteLine("2»ses" + session + "»succes");
                }
                // chat has still to be implemented
                b = reader.ReadLine();
            }
            Console.WriteLine("Closing session: ses" + session);
            stream.WriteLine("4»" + session + "»logoutsucces");
            stream.Flush();
            stream.Close();
            reader.Close();
        }
    }
}
