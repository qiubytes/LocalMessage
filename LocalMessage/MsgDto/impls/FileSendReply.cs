using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessage.MsgDto.impls
{
    /// <summary>
    /// 文件发送请求响应
    /// </summary>
    public class FileSendReply
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string RelationMsgID { get; set; }
        /// <summary>
        /// 是否同意
        /// </summary>
        public bool IsReply { get; set; }
    }
}
