using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 弹幕合并.Common
{
    /// <summary>
    /// 服务器的返回
    /// </summary>
    public class ServerReturn
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int error { get; set; } = 0;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string error_msg { get; set; } = string.Empty;

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object data { get; set; }
    }
}