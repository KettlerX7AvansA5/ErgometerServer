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

        public List<ClientThread> clients { get; }
        private DoctorThread doctor;

        public Server()
        {
            FileHandler.CheckDataFolder();

            Console.WriteLine("Checking datafolder " + FileHandler.DataFolder);

            clients = new List<ClientThread>();

            TcpListener listener = new TcpListener(NetHelper.GetIP("127.0.0.1"), 8888);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for connection with client...");

                //AcceptTcpClient waits for a connection from the client
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Client connected");

                //Start new client
                ClientThread cl = new ClientThread(client, this);
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

        public void sendToDoctor()
        {
            
        }

        public void sendToClient(int session)
        {
            
        }
    }
}
