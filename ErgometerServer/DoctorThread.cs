using System;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using ErgometerLibrary;
using System.Collections.Generic;

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
                    //Nog toe te voegen:
                    // - Oude data opsturen (metingen)
                    // - Oude sessies bekijken  (lijst met sessies)
                    // - Users opvragen
                    // - Gegevens van huidige sessie krijgen (gebruikersnaam enz)
                    // - Huidige sessies
                    // - 
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
                    case NetCommand.CommandType.USER:
                        server.AddUser(input.users);
                        break;
                    case NetCommand.CommandType.REQUEST:
                        switch (input.Request)
                        {
                            case NetCommand.RequestType.USERS:
                                sendToDoctor(new NetCommand(FileHandler.LoadUsers(), input.Session));
                                break;
                            case NetCommand.RequestType.OLDDATA:
                                List<Meting> metingen = FileHandler.ReadMetingen(input.Session);
                                foreach (Meting meting in metingen)
                                {
                                    sendToDoctor(new NetCommand(meting, input.Session));
                                }
                                break;
                            case NetCommand.RequestType.ALLSESSIONS:
                                //Net implemented yet
                                break;
                            case NetCommand.RequestType.CURRENTSESSIONS:
                                //not implemented yet
                                break;
                            case NetCommand.RequestType.SESSIONDATA:
                                //not implemented yet
                                break;
                            default:
                                throw new FormatException("Unknown Command");
                        }
                        
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