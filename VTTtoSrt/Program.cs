using System;
using System.IO;
using System.Text;
using VTT;

namespace VTTtoSrt
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

            var vtt = VTTHelper.GetVTTFromFile(fileName);

            int id = 1;
            StringBuilder writer = new StringBuilder();

            foreach (var item in vtt.Lines)
            {
                var srtItem = new Dogvane.Srt.Battuta()
                {
                    Id = id++,
                    From = item.From,
                    Text =item.Text,
                    To = item.To,
                };
                writer.AppendLine(srtItem.GetData());
                writer.AppendLine();
            }

            var newFileName = fileName.Replace(".vvt", "") + ".srt";
            File.WriteAllText(newFileName, writer.ToString(), Encoding.UTF8);
        }
    }
}
