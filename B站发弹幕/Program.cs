using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dogvane.Srt;

namespace B站发弹幕
{
    class Program
    {

        //public static string 发弹幕用户id = "";
        //public static string 影片id = "25437639";
        //public static string cid = "43246475"; // 注意这个值的获取，有点奇怪
        //public static string Cookie = "";
        //public static int FontSize = 36;
        //public static int mode = 4; // 1表示普通字幕  4表示底部字幕 5表示顶部
        //public static int pool = 1; // 0 表示普通弹幕 1 表示字幕弹幕

        const string 弹幕Url = "https://api.bilibili.com/x/v2/dm/post";
        const int 视频每秒多少ms = 1000;

        static Config _config = new Config
        {
            cookie = "",
            avid = "25437639",
            mode = 4,
            pool = 1,
            fontSize = 36
        };

        static void Main(string[] args)
        {
            if (!File.Exists("config.json"))
            {
                Console.WriteLine("找不到配置文件 config.json");
                return;
            }

            var config = Config.LoadConfig();
            if (config == null)
            {
                return;
            }
            _config = config;

            var fileName = _config.srtfile;
            if (!File.Exists(fileName))
            {
                Console.WriteLine("没找到弹幕文件:{0}", fileName);
                return;
            }

            var lines = File.ReadAllLines(fileName, Encoding.Default);
            var srtReader = new SrtManager();
            var battutes = srtReader.LoadBattute(lines);
            for (var i = 0; i < battutes.Count; i++)
            {
                var item = battutes[i];
                Console.WriteLine(item.Id);

                var client = GetClient();
                var post = GetPostData((int)(item.FromSec * 视频每秒多少ms), item.Text);
                var postEscape = Uri.EscapeUriString(post);
                Console.WriteLine(post);

                var ret = client.UploadData(弹幕Url, Encoding.UTF8.GetBytes(postEscape));
                var json = Encoding.UTF8.GetString(ret);
                Console.WriteLine(json);

                Thread.Sleep(31000);
            }
        }

        void test2()
        {
            Console.WriteLine(GetPostData(1427, "测试弹幕"));
        }
        static string Getcsrf()
        {
            var reg = new Regex(@"bili_jct=\w*");
            return reg.Match(_config.cookie).Value.Replace("bili_jct=", "");
        }

        static Random rand = new Random();
        public static string GetPostData(int farme, string text)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("type=1");
            sb.Append("&").Append("oid=").Append(getcid());
            sb.Append("&").Append("msg=").Append(text);
            sb.Append("&").Append("aid=").Append(_config.avid);
            sb.Append("&").Append("progress=").Append(farme);
            sb.Append("&").Append("color=").Append("16777215");
            sb.Append("&").Append("fontsize=").Append(_config.fontSize);
            sb.Append("&").Append("pool=").Append(_config.pool);
            sb.Append("&").Append("mode=").Append(_config.mode);
            sb.Append("&").Append("rnd=").Append("1502109" + rand.Next(879020920, 969263783));
            sb.Append("&").Append("plat=").Append(1);
            sb.Append("&").Append("csrf=").Append(Getcsrf());

            return sb.ToString();
        }

        public static string getcid()
        {
            var client = GetPageClient();
            var bytes = client.DownloadData(string.Format("https://www.bilibili.com/video/av{0}", _config.avid));
            var html = Encoding.UTF8.GetString(bytes.GZipDecompress());

            var reg = new Regex(@"cid=\d*&");
            var ret = reg.Match(html).Value;
            // 43246475
            return ret.Replace("cid=", "").Replace("&", "");
        }

        static WebClient GetPageClient()
        {
            WebClient client = new WebClient();

            client.Headers.Add("Host", "www.bilibili.com");
            client.Headers.Add("Accept", "*/*");
            client.Headers.Add("Origin", "https://www.bilibili.com");
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.75 Safari/537.36");
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            client.Headers.Add("Referer", string.Format("https://www.bilibili.com/video/av{0}", _config.avid));
            client.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,zh-TW;q=0.8");
            client.Headers.Add("Cookie", _config.cookie);

            return client;
        }

        static WebClient GetClient()
        {
            WebClient client = new WebClient();

            client.Headers.Add("Host", "api.bilibili.com");
            client.Headers.Add("Accept", "*/*");
            client.Headers.Add("Origin", "https://www.bilibili.com");
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.75 Safari/537.36");
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            client.Headers.Add("Referer", string.Format("https://www.bilibili.com/video/av{0}", _config.avid));
            client.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,zh-TW;q=0.8");
            client.Headers.Add("Cookie", _config.cookie);

            return client;
        }
    }
}
