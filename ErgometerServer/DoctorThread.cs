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
        ArrayList threads;

        bool running;
        bool loggedin;


        public DoctorThread(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;
            this.name = "Unknown";
            this.threads = new ArrayList();
            this.running = false;
            this.loggedin = false;
        }

        public void run()
        {
            running = true;
            loggedin = true;
            while (loggedin && running)
            {
                if (threads.Count < server.clients.Count)
                {
                    DoctorSessionThread dt = new DoctorSessionThread(1, client, this);
                    threads.Add(dt);
                    Thread thread = new Thread(new ThreadStart(dt.run));
                    thread.Start();
                }
            }
        }

        public void logout()
        {
            loggedin = false;
        }
    }
}