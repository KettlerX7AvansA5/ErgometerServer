﻿using System;
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

        bool running;


        public DoctorThread(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;

            this.running = false;
        }

        public void run()
        {
            running = true;
            while (running)
            {
                NetCommand input = NetHelper.ReadNetCommand(client);

                switch (input.Type)
                {
                    case NetCommand.CommandType.LOGOUT:
                        running = false;
                        client.Close();
                        Console.WriteLine("Doctor logged out");
                        break;
                    case NetCommand.CommandType.CHAT:
                        server.SendToClient(input);
                        break;
                    case NetCommand.CommandType.VALUESET:
                        server.SendToClient(input);
                        break;
                    default:
                        throw new FormatException("Unknown Command");
                }
            }
        }

        public void sendToDoctor(NetCommand command)
        {
            NetHelper.SendNetCommand(client, command);
        }
    }
}