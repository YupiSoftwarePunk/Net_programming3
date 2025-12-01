using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class ClientInfo
    {
        public string Name { get; set; }
        public IPEndPoint EndPoint { get; set; }

        public ClientInfo(string name, IPEndPoint endPoint)
        {
            Name = name;
            EndPoint = endPoint;
        }
    }
}
