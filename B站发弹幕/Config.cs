using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace B站发弹幕
{
    public class Config
    {
        public string srtfile { get; set; }
        public string avid { get; set; }
        public string cookie { get; set; }
        public int fontSize { get; set; }

        // 1表示普通字幕  4表示底部字幕 5表示顶部
        public int mode { get; set; }

        // 0 表示普通弹幕 1 表示字幕弹幕
        public int pool { get; set; }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <returns></returns>
        public static Config LoadConfig()
        {
            try
            {
                var json = File.ReadAllText("send_srt_config.json", Encoding.Default);
                var ret = JsonConvert.DeserializeObject<Config>(json);
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        void test()
        {
            var obj = new Config();
            var str = JsonConvert.SerializeObject(obj, Formatting.Indented);
            Console.WriteLine(str);
        }
    }

    
}
