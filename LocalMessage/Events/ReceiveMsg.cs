using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessage.Events
{
    public class ReceiveMsg
    {
        public string OriginIp { get; set; }
        public string Content { get; set; }
    }
}
