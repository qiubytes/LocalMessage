using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessage.Events
{
    public class FileSendReceiveProgress
    {
        public long totalBytes {  get; set; }    
        public long currentBytes { get; set; }
        /// <summary>
        /// 状态：未传输、传输中、传输完成、传输失败
        /// </summary>
        public string state { get; set; }
        /// <summary>
        /// 消息提示：来自xxxxip的xxx文件
        /// </summary>
        public string msg {  get; set; }    
    }
}
