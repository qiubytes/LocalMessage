using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.MsgDto.Scan
{
    /// <summary>
    /// 扫描请求
    /// </summary>
    public class ScanRequest : MsgBaseType
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
