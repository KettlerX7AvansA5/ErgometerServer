using System.Net.Sockets;

namespace ErgometerServer
{
    class DoctorThread
    {
        TcpClient client;
        Server server;

        string name;
        int session;

        bool running;
        bool loggedin;


        public DoctorThread(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;
            this.name = "Unknown";
            this.session = 0;
            this.running = false;
            this.loggedin = false;
        }

        public void run()
        {

        }
    }
}