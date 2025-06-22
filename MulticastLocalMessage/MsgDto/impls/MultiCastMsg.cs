using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.MsgDto.impls
{
    /// <summary>
    ///  1、群发组播消息
    /// </summary>
    public class MultiCastMsg
    {
        public string originIp { get; set; }
        public string content { get; set; }
    }
}
