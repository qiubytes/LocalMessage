using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessage.MsgDto.impls
{
    /// <summary>
    /// 文件发送请求
    /// </summary>
    public class FileSendApply
    {
        public string MsgID { get; set; }
        /// <summary>
        /// 源IP
        /// </summary>
        public string originIp { get; set; }
        /// <summary>
        /// 目标IP
        /// </summary>
        public string destIp {  get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
    }
}
