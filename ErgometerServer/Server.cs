﻿using ErgometerLibrary;
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
        private static Dictionary<string, string> users;

        public Server()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            FileHandler.CheckStorage();

            users = FileHandler.LoadUsers();
            clients = new List<ClientThread>();

            TcpListener listener = new TcpListener(NetHelper.GetIP("127.0.0.1"), 8888);
            listener.Start();

            Console.WriteLine("Server started successfully...");

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

        public void SendToDoctor(NetCommand command)
        {
            if (doctor != null)
            {
                doctor.sendToDoctor(command);
            }
            else
            {
                Console.WriteLine("No doctor connected to the server yet");
            }
        }

        public void BroadcastToClients(NetCommand command)
        {
            foreach (ClientThread clientThread in clients)
            {
                    clientThread.SendToClient(command);
            }
        }

        public void SendToClient(NetCommand command)
        {
            foreach (ClientThread clientThread in clients)
            {
                if (clientThread.session == command.Session)
                {
                    clientThread.SendToClient(command);
                }
            }
        }

        private void AddUser(string name, string password)
        {
            users.Add(name, password);
            FileHandler.SaveUsers(users);
        }

        private void AddUser(Dictionary<string, string> users)
        {
            foreach(KeyValuePair<string, string> user in users)
            {
                users.Add(user.Key, user.Value);
            }
            
            FileHandler.SaveUsers(users);
        }

        public bool CheckPassword(string name, string password)
        {
            string pass;
            bool isOk = users.TryGetValue(name, out pass);

            Console.WriteLine($"Checking {name}, {password}: found {isOk} with response {pass}");

            if (!isOk) return false;

            return pass == password;
        }

        private string GeneratePassword(int len = 8)
        {
            string pass = "";

            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

            Random rand = new Random();

            for (int i=0; i<=len; i++)
            {
                pass += chars[rand.Next(0, chars.Length - 1)];
            }

            return pass;
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Closing server");
        }
    }

}
