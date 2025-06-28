using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalMessage.Config
{
    /// <summary>
    /// 读取配置文件的帮助类
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        private static Config GetConfig()
        {
            string basedir = AppContext.BaseDirectory;
            string configPath = System.IO.Path.Combine(basedir, "config.json");
            if (!System.IO.File.Exists(configPath))
            {
                Config config = new Config { nettype = NetType.Multicast }; // 默认配置
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never // 仅忽略 null，但保留默认值
                });
                File.WriteAllText(configPath, json);
                return config;
            }
            else
            {
                string json = System.IO.File.ReadAllText(configPath);
                return System.Text.Json.JsonSerializer.Deserialize<Config>(json);
            }
        }
        /// <summary>
        /// 读取网络类型配置
        /// </summary>
        /// <returns></returns>
        public static NetType GetNetType()
        {
            Config config = GetConfig();
            return config.nettype;
        }
    }
    /// <summary>
    /// 网络类型枚举：广播、组播
    /// </summary>
    public enum NetType
    {
        Boradcast,
        Multicast
    }
    public class Config
    {
        /// <summary>
        /// 网络类型
        /// </summary>
        [JsonInclude]
        public NetType nettype;
    }
}
