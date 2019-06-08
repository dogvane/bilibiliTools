using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 弹幕合并.Common
{
    /// <summary>
    /// 翻译的接口
    /// </summary>
    public interface ITransApi
    {
        string GetTrans(string query, string from, string to);
    }

    public static class TransFac
    {
        /// <summary>
        /// 获得当前正在用的翻译用api
        /// 优先使用百度,其次是腾讯
        /// </summary>
        /// <returns></returns>
        public static ITransApi GetCurrentTransApi()
        {
            if (!string.IsNullOrEmpty(BaiduFanyi.TransApi.s_appid))
                return new BaiduFanyi.TransApi();

            return new QCloud.QCloudTransApi();
        }
    }
}
