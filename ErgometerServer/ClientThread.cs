using ErgometerLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErgometerServer
{
    class ClientThread
    {
        TcpClient client;
        Server server;

        string name;
        int session;

        bool running;
        bool loggedin;

        List<Meting> metingen;
        List<ChatMessage> chat;


        public ClientThread(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;
            this.name = "Unknown";
            this.session = 0;
            this.running = false;
            this.loggedin = false;

            metingen = new List<Meting>();
            chat = new List<ChatMessage>();
        }

        public void run()
        {
            running = true;

            session = FileHandler.GenerateSession();
            NetHelper.SendNetCommand(client, new NetCommand(session));

            while (running)
            {
                NetCommand input = NetHelper.ReadNetCommand(client);

                switch(input.Type)
                {
                    case NetCommand.CommandType.SESSION:
                        break;
                    case NetCommand.CommandType.LOGIN:
                        if(input.IsDoctor)
                        {
                            server.ChangeClientToDoctor(client, this);
                            running = false;
                        }
                        else
                        {
                            name = input.DisplayName;
                            loggedin = true;
                            FileHandler.CreateSession(session, name);
                        }
                        break;
                    case NetCommand.CommandType.DATA:
                        metingen.Add(input.Meting);
                        FileHandler.WriteMetingen(session, metingen);
                        break;
                    case NetCommand.CommandType.CHAT:
                        chat.Add(new ChatMessage(name, input.ChatMessage));
                        break;
                    case NetCommand.CommandType.LOGOUT:
                        loggedin = false;
                        running = false;
                        FileHandler.WriteMetingen(session, metingen);
                        FileHandler.WriteChat(session, chat);
                        client.Close();
                        break;
                    default:
                        throw new FormatException("Unknown command");
                }
            }
        }

    }
}
