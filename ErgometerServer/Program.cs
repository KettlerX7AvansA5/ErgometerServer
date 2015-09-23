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
            string name = null;
            int session = 0;
            bool doctor;
            string a = reader.ReadLine();
            Console.WriteLine(a);
            if (a.StartsWith("1»") && a.EndsWith("connect"))
            {
                Console.WriteLine("Connection established");
                stream.WriteLine(a.Substring(0, a.Length - 7) + "cs");
            }
            string b = reader.ReadLine();
            if (b.StartsWith("2»"))
            {
                Console.WriteLine(b);
                string[] package = b.Split('»');
                name = package[2];
                session = int.Parse(package[1]);
                doctor = package[3].Equals("doctor");
                if (doctor)
                {
                    //check password
                }
                else
                {
                    Console.WriteLine("login of " + name + " succesful");
                    stream.WriteLine("2»" + package[1] + "»client»ls");
                }
            }
            string c = reader.ReadLine();
            ArrayList data = new ArrayList();
            while (c.StartsWith("3»"))
            {
                Console.WriteLine(c);
                data.Add(c);
                stream.WriteLine("3»" + session + "»succes");
                c = reader.ReadLine();
            }
            string d = reader.ReadLine();
            ArrayList oldData = new ArrayList();
            while (c.StartsWith("4»"))
            {
                Console.WriteLine(d);
                oldData.Add(d);
                stream.WriteLine("4»" + session + "»succes");
                d = reader.ReadLine();
            }
            stream.Flush();
            stream.Close();
            reader.Close();
        }
    }
}
