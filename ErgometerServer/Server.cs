using ErgometerLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ErgometerServer
{
    class Server
    {
        static void Main(string[] args)
        {
            new Server();
        }

        private List<ClientThread> clients;
        private DoctorThread doctor;

        public Server()
        {
            FileHandler.CheckDataFolder();

            TcpListener listener = new TcpListener(NetHelper.GetIP("127.0.0.1"), 8888);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for connection with client...");

                //AcceptTcpClient waits for a connection from the client
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Client connected");

                //Start new client
                var cl = new ClientThread(client, this);
                clients.Add(cl);

                //Run client on new thread
                Thread thread = new Thread(new ThreadStart(cl.run));
                thread.Start();
            }
        }

        public static string GetIp()
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

        public void ChangeClientToDoctor(TcpClient client, ClientThread clth)
        {
            clients.Remove(clth);
            doctor = new DoctorThread(client, this);
            Thread thread = new Thread(new ThreadStart(doctor.run));
            thread.Start();
        }




        //OLD DO NOT USE
        static void OldThread(object obj)
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
