using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.MsgDto
{
    /// <summary>
    /// 消息基础类型
    /// </summary>
    public class MsgBaseType
    {
        /// <summary>
        /// 消息类型 1、扫描请求，2、扫描响应、
        /// </summary>
        public string MsgType { get; set; }
    }
}
