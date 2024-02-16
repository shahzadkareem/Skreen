using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_Remote
{
     class ConnectionLog
    {

        public string UserId { get; set; }

        public string RemoteIP { get; set; }
        public string LocalIP { get; set; }
        public string LocalMac { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
