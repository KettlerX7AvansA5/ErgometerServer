using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ErgometerServer
{
    class DoctorSessionThread
    {
        public int session { get; }
        private TcpClient client;
        private DoctorThread doctor;
        private Boolean running;

        public DoctorSessionThread(int session, TcpClient client, DoctorThread doctor)
        {
            this.session = session;
            this.client = client;
            this.doctor = doctor;
        }

        public void run()
        {
            running = true;
            while (running)
            {
                
            }
        }
    }
}
