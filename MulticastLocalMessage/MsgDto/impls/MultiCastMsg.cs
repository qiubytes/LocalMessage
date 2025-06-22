using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.MsgDto.impls
{
    /// <summary>
    /// tab1 组播群发消息
    /// </summary>
    public class MultiCastMsg
    {
        public string originIp { get; set; }
        public string content { get; set; }
    }
}
