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
                    case NetCommand.CommandType.LOGOUT:
                        running = false;
                        client.Close();
                        Console.WriteLine("Doctor logged out");
                        break;
                    case NetCommand.CommandType.CHAT:
                        server.SendToClient(input);
                        break;
                    case NetCommand.CommandType.BROADCAST:
                        server.BroadcastToClients(input.ChatMessage);
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
                                sendToDoctor(new NetCommand(NetCommand.LengthType.USERS, server.users.Count-1, input.Session));
                                foreach (KeyValuePair<string, string> user in server.users)
                                {
                                    Thread.Sleep(10);
                                    if(user.Key != "Doctor0tVfW")
                                        sendToDoctor(new NetCommand(user.Key, user.Value, input.Session));
                                }
                                break;
                            case NetCommand.RequestType.CHAT:
                                List<ChatMessage> chat = FileHandler.ReadChat(input.Session);
                                foreach (ChatMessage msg in chat)
                                {
                                    Thread.Sleep(10);
                                    sendToDoctor(new NetCommand(msg.Message, msg.IsDoctor, input.Session));
                                }
                                break;
                            case NetCommand.RequestType.OLDDATA:
                                List<Meting> metingen = FileHandler.ReadMetingen(input.Session);
                                foreach (Meting meting in metingen)
                                {
                                    Thread.Sleep(10);
                                    sendToDoctor(new NetCommand(meting, input.Session));      
                                }
                                break;
                            case NetCommand.RequestType.ALLSESSIONS:
                                List<Tuple<int, string, double>> sessions = FileHandler.GetAllSessions();
                                sendToDoctor(new NetCommand(NetCommand.LengthType.SESSIONS, sessions.Count, input.Session));
                                foreach(Tuple<int,string,double> session in sessions)
                                {
                                    sendToDoctor(new NetCommand(session.Item2, session.Item3, session.Item1));
                                    
                                    Thread.Sleep(10);
                                }
                                break;
                            case NetCommand.RequestType.SESSIONDATA:
                                List<Tuple<int, string>> currentsessionsdata = server.GetRunningSessionsData();
                                sendToDoctor(new NetCommand(NetCommand.LengthType.SESSIONDATA, currentsessionsdata.Count, input.Session));
                                foreach (Tuple<int, string> ses in currentsessionsdata)
                                {
                                    sendToDoctor(new NetCommand(ses.Item2, Helper.Now, ses.Item1));
                                    Thread.Sleep(10);
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