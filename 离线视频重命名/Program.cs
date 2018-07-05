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
            var path = @"C:\Users\Dogvane\Desktop\bilibili\download";
            var outpath = @"H:\flvout";
            var entrys = Directory.GetFiles(path, "entry.json", SearchOption.AllDirectories);
            var batdata = new StringBuilder();


            foreach (var fileName in entrys)
            {
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


                var blvs = fileInfo.Directory.GetFiles("*.blv", SearchOption.AllDirectories).ToList();

                if (blvs.Count > 1)
                {
                    blvs.Sort((o1, o2) =>
                    {
                        return GetCompareName(o1).CompareTo(GetCompareName(o2));
                    });

                    StringBuilder sb = new StringBuilder();
                    
                    foreach (var blv in blvs)
                    {
                        sb.Append("file '").Append(blv.FullName).Append("'").AppendLine();
                    }

                    if (!Directory.Exists(Path.Combine(outpath, "concat")))
                        Directory.CreateDirectory(Path.Combine(outpath, "concat"));

                    var concatFileName = Path.Combine(outpath, "concat", title + partName + ".txt");
                    
                    File.WriteAllText(concatFileName, sb.ToString());

                    var bat = string.Format("ffmpeg -f concat -safe 0 -i \"{0}\" -c copy \"{1}\"", concatFileName, Path.Combine(outpath, outFileName));

                    batdata.AppendLine(bat);
                }
                else
                {
                    // File.Copy(blvs[0].FullName, Path.Combine(outpath, outFileName));

                    batdata.AppendFormat("copy \"{0}\" \"{1}\"", blvs[0].FullName, Path.Combine(outpath, outFileName)).AppendLine();
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
