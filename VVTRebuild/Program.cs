using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VTT;

namespace VVTRebuild
{
    internal class Program
    {
        /// <summary>
        /// youtube字幕重建功能
        /// 会按照行文的段落，重新对字幕做重组
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var path = @"W:\zb\t16";
            var file = @"So you want to be a Code Reviewer #3 - Code Review-5D6IAKebZSQ.en.vtt";

            // DoFile(Path.Combine(path, file));
            //foreach (var fileName in Directory.GetFiles(path, "*.vvt"))
            //{
            //}

            var fileName = Path.Combine(path, file);
            if (args.Length > 0)
            {
                fileName = args[0];
            }

            Vtt vtt = VTTHelper.GetVTTFromFile(fileName);
            // 将 vtt 合并到一个厂的列表里，再逐个拆开

            vtt = VvtRebuild.Rebuild(vtt);

            int id = 1;
            StringBuilder writer = new StringBuilder();

            foreach (var item in vtt.Lines)
            {
                var srtItem = new Dogvane.Srt.Battuta()
                {
                    Id = id++,
                    From = item.From,
                    Text = item.Text,
                    To = item.To,
                };
                writer.AppendLine(srtItem.GetData());
                writer.AppendLine();
            }

            Console.WriteLine($"id: {id}");
                    
            var newFileName = fileName.Replace(".vvt", "") + ".srt";
            File.WriteAllText(newFileName, writer.ToString(), Encoding.UTF8);
        }

    }
}