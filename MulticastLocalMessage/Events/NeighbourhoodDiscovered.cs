using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.Events
{
    /// <summary>
    /// 发现邻居消息
    /// </summary>
    public class NeighbourhoodDiscovered
    {
        /// <summary>
        /// 邻居IP
        /// </summary>
        public string NeighbourhoodIp { get; set; }
    }
}
