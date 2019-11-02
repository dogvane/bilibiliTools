using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace 离线视频重命名
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"R:\73706327";
            var outpath = @"V:\bilibil2\Elasticsearch高级课程2019入门到核心技术讲解与项目实战教程";

            if(args.Length > 0)
            {
                path = args[0];
                outpath = AppDomain.CurrentDomain.BaseDirectory;
            }

            var entrys = Directory.GetFiles(path, "entry.json", SearchOption.AllDirectories);
            var batdata = new StringBuilder();


            foreach (var fileName in entrys)
            {
                Console.WriteLine(fileName);
                var fileInfo = new FileInfo(fileName);
                var hasSubPart = fileInfo.Directory.Parent.GetDirectories().Length > 1;

                var json = File.ReadAllText(fileName);
                
                var data = JsonConvert.DeserializeObject(json) as JObject;

                var title = data["title"].ToString().ReplaceInvalidPath();
                var partName = data["page_data"]["part"].ToString().ReplaceInvalidPath();

                var outFileName = title + ".flv";
                if (hasSubPart)
                {
                    outFileName = title + " " + partName + ".flv";
                }

                int prefered_video_quality = 0;

                if (data["prefered_video_quality"] != null)
                {
                    prefered_video_quality = (int)data["prefered_video_quality"];
                }

                #region blv 模式


                var blvs = fileInfo.Directory.GetFiles("*.blv", SearchOption.AllDirectories).ToList();

                if (blvs.Count > 0)
                {
                    // blv 文件模式
                    if (blvs.Count > 1)
                    {
                        blvs.Sort((o1, o2) => { return GetCompareName(o1).CompareTo(GetCompareName(o2)); });

                        StringBuilder sb = new StringBuilder();

                        foreach (var blv in blvs)
                        {
                            sb.Append("file '").Append(blv.FullName).Append("'").AppendLine();
                        }

                        if (!Directory.Exists(Path.Combine(outpath, "concat")))
                            Directory.CreateDirectory(Path.Combine(outpath, "concat"));

                        var concatFileName = Path.Combine(outpath, "concat", title + partName + ".txt");

                        File.WriteAllText(concatFileName, sb.ToString());

                        var bat = string.Format("ffmpeg -f concat -safe 0 -i \"{0}\" -c copy \"{1}\"", concatFileName,
                            Path.Combine(outpath, outFileName));

                        batdata.AppendLine(bat);
                    }
                    else
                    {
                        // File.Copy(blvs[0].FullName, Path.Combine(outpath, outFileName));

                        batdata.AppendFormat("copy \"{0}\" \"{1}\"", blvs[0].FullName,
                            Path.Combine(outpath, outFileName)).AppendLine();
                    }
                }

                #endregion


                var m4s = fileInfo.Directory.GetFiles("*.m4s", SearchOption.AllDirectories).ToList();

                if (m4s.Count > 0)
                {
                    // m4s 模式，音频和视频分离，但是不存在时间上的分割了
                    var video = m4s.FirstOrDefault(o => o.Name == "video.m4s");
                    var audio = m4s.FirstOrDefault(o => o.Name == "audio.m4s");

                    if (video == null || audio == null)
                    {
                        continue;
                    }

                    var bat = string.Format("ffmpeg -i \"{0}\" -i \"{1}\" -c:v copy -c:a copy \"{2}\"", video.FullName, audio.FullName, Path.Combine(outpath, outFileName));

                    batdata.AppendLine(bat);

                }
            }

            Console.WriteLine(batdata);
            File.WriteAllText(Path.Combine(outpath, "run.bat"), batdata.ToString(), Encoding.Default);
        }

        private static string GetCompareName(FileInfo o1)
        {
            var name1 = o1.Name;
            if (name1.Length == 5)
            {
                name1 = "0" + name1;
            }

            return name1;
        }
    }

    static class PathExtend
    {
        public static string ReplaceInvalidPath(this string file)
        {
            foreach(var p in Path.GetInvalidFileNameChars())
            {
                file = file.Replace(p.ToString(), "");
            }

            return file;
        }
    }
}
