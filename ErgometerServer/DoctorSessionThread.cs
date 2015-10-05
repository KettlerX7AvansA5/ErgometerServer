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
        private int session;
        private TcpClient client;
        private DoctorThread doctor;

        public DoctorSessionThread(int session, TcpClient client, DoctorThread doctor)
        {
            this.session = session;
            this.client = client;
            this.doctor = doctor;
        }

        public void run()
        {
            
        }
    }
}
