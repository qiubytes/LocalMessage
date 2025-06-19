using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.MsgDto.Scan
{
    /// <summary>
    /// 扫描响应
    /// </summary>
    public class ScanResponse : MsgBaseType
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
