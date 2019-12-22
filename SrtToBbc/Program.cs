using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dogvane.Srt;
using Newtonsoft.Json;
using 弹幕合并.Bussiness.Entity;

namespace SrtToBbc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("请输入要转换的 srt 文件.");
                return;
            }

            var fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("文件不存在");
                return;
            }

            var srtdata = new SrtManagerT<Battuta>().LoadBattuteByFile(fileName);

            var bilibili = new BilibiliSrtFile();
            bilibili.font_size = 0.4;
            bilibili.font_color = "#FFFFFF";
            bilibili.background_alpha = 0.5;
            bilibili.background_color = "#9C27B0";
            bilibili.Stroke = "none";
            bilibili.body = new List<BilibiliSrtFile.Battuta>();

            foreach (var item in srtdata)
            {
                bilibili.body.Add(new BilibiliSrtFile.Battuta
                {
                    from = item.FromSec,
                    to = item.ToSec,
                    content = item.Text,
                    location = 2,
                });
            }
            var fi = new FileInfo(fileName);
            var ex = fi.Extension;
            var writeFileName = fileName.Replace(ex, ".bcc");

            var json = JsonConvert.SerializeObject(bilibili, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);

            File.WriteAllBytes(writeFileName, bytes);
        }
    }
}
