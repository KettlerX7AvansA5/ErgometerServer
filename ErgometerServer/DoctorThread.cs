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
                    // - Oude data opsturen (metingen) (not tested yet)
                    // - Oude sessies bekijken  (lijst met sessies) (not tested yet)
                    // - Users opvragen (not tested yet)
                    // - Gegevens van huidige sessie krijgen (gebruikersnaam enz) (not tested yet)
                    // - Huidige sessies (not tested yet)
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
                        server.AddUser(input.DisplayName, input.Password);
                        break;
                    case NetCommand.CommandType.REQUEST:
                        switch (input.Request)
                        {
                            case NetCommand.RequestType.USERS:
                                sendToDoctor(new NetCommand(NetCommand.LengthType.USERS, server.users.Count, input.Session));
                                foreach (KeyValuePair<string, string> user in server.users)
                                {
                                    sendToDoctor(new NetCommand(user.Key, user.Value, input.Session));
                                }
                                break;
                            case NetCommand.RequestType.OLDDATA:
                                List<Meting> metingen = FileHandler.ReadMetingen(input.Session);
                                sendToDoctor(new NetCommand(NetCommand.LengthType.DATA, metingen.Count, input.Session));
                                foreach (Meting meting in metingen)
                                {
                                    sendToDoctor(new NetCommand(meting, input.Session));
                                }
                                break;
                            case NetCommand.RequestType.ALLSESSIONS:
                                int[] sessions = FileHandler.GetAllSessions();
                                sendToDoctor(new NetCommand(NetCommand.LengthType.SESSIONS, sessions.Length, input.Session));
                                for (int i = 0; i < sessions.Length; i++)
                                {
                                    sendToDoctor(new NetCommand(NetCommand.CommandType.SESSION, sessions[i]));
                                }
                                break;
                            case NetCommand.RequestType.CURRENTSESSIONS:
                                List<int> currentsessions = server.GetRunningSessions();
                                sendToDoctor(new NetCommand(NetCommand.LengthType.CURRENTSESSIONS, currentsessions.Count, input.Session));
                                foreach (int session in currentsessions)
                                {
                                    sendToDoctor(new NetCommand(NetCommand.CommandType.SESSION, session));
                                }
                                break;
                            case NetCommand.RequestType.SESSIONDATA:
                                List<Tuple<int, string>> currentsessionsdata = server.GetRunningSessionsData();
                                sendToDoctor(new NetCommand(NetCommand.LengthType.SESSIONDATA, currentsessionsdata.Count, input.Session));
                                foreach (Tuple<int, string> ses in currentsessionsdata)
                                {
                                    sendToDoctor(new NetCommand(ses.Item2, false, ses.Item1));
                                }
                                break;
                            default:
                                throw new FormatException("Unknown Command");
                        }
                        
                        break;
                    case NetCommand.CommandType.ERROR:
                        Console.WriteLine("An error occured, assuming docter disconnected");
                        running = false;
                        Console.WriteLine("Doctor logged out due to an error");
                        client.Close();
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