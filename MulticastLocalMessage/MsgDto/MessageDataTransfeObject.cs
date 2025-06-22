using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.MsgDto
{
    /// <summary>
    /// 除了文件传输、其余消息都需要解析为这个类型
    /// </summary>
    public class MessageDataTransfeObject
    {
        /// <summary>
        /// 消息类型 1、群发组播消息 2、发现组播邻居 3、发现组播邻居（回应）,4、Udp单发消息
        /// </summary>
        public string MsgType { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
    }
}
