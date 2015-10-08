using System;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using ErgometerLibrary;

namespace ErgometerServer
{
    class DoctorThread
    {
        TcpClient client;
        Server server;

        string name;

        bool running;
        bool loggedin;


        public DoctorThread(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;
            this.name = "Unknown";
            this.running = false;
            this.loggedin = false;
        }

        public void run()
        {
            running = true;
            loggedin = true;
            while (loggedin && running)
            {
                NetCommand input = NetHelper.ReadNetCommand(client);

                switch (input.Type)
                {
                    case NetCommand.CommandType.LOGOUT:
                        loggedin = false;
                        Console.WriteLine("Doctor logged out");
                        break;
                    case NetCommand.CommandType.CHAT:
                        server.SendToClient(input);
                        Console.WriteLine(input);
                        break;
                    default:
                        throw new FormatException("Unknown Command");
                }
            }
            client.Close();
        }

        public void sendToDoctor(NetCommand command)
        {
            NetHelper.SendNetCommand(client, command);
        }
    }
}