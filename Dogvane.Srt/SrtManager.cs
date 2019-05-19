using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dogvane.Srt
{
    public class SrtManager
    {
        /// <summary>
        /// china name zimu
        /// </summary>
        List<Battuta> battute;

        public SrtManager()
        {
            battute = new List<Battuta>();
        }

        public List<Battuta> LoadBattuteByFile(string fileName)
        {
            return LoadBattute(File.ReadAllLines(fileName));
        }

        /// <summary>
        /// Parse file line to buttua list
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public List<Battuta> LoadBattute(string[] lines)
        {
            // data lines, battuta has 3 line data
            List<List<string>> battutaSource = new List<List<string>>();

            // temp line
            List<string> tmpline = new List<string>();

            int id;
            string from, to, content;

            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    tmpline.Add(line);
                }
                else
                {
                    battutaSource.Add(tmpline);
                    tmpline = new List<string>();
                }
            }

            foreach (var data in battutaSource)
            {
                if(data.Count < 3)
                {
                    continue;
                }

                id = int.Parse(data[0]);
                from = GetFromData(data[1]);
                to = GetToData(data[1]);
                if (data.Count == 3)
                {
                    content = data[2];
                }
                else
                {
                    // 带换行字幕的读取
                    content = string.Empty;
                    for (var i = 2; i < data.Count; i++)
                    {
                        content += data[i] + "\n";
                    }

                    content = content.TrimEnd('\n');
                }

                battute.Add(new Battuta(id, from, to, content));
            }

            return battute;
        }

        public void SaveBattute(string fileName)
        {
            StringBuilder writer = new StringBuilder();
            foreach(var item in battute)
            {
                writer.AppendLine(item.GetData());
            }

            File.WriteAllText(fileName, writer.ToString(), Encoding.UTF8);
        }

        public static void SaveBattute(string fileName, List<Battuta> battuts)
        {
            StringBuilder writer = new StringBuilder();
            foreach (var item in battuts)
            {
                writer.AppendLine(item.GetData());
            }

            File.WriteAllText(fileName, writer.ToString(), Encoding.UTF8);
        }

        public Battuta GetBattuta(int id)
        {
            foreach (var battuta in battute)
            {
                if (battuta.Id == id)
                {
                    return battuta;
                }
            }
            return null;
        }

        public void SetBattuta(int id, Battuta b)
        {
            for (int i = 0; i < battute.Count; i++)
            {
                if (battute[i].Id == id)
                {
                    battute[i] = b;
                }
            }
        }

        /// <summary>
        /// Returns the starting time
        /// </summary>
        /// <param name="fromto">format data like 00:01:01,132 -> 00:01:04,345</param>
        /// <returns>format data like 00:01:01,132</returns>
        private string GetFromData(string fromto)
        {
            return fromto.Split('-', '>')[0].Trim();
        }

        /// <summary>
        /// Returns the ending time
        /// </summary>
        /// <param name="fromto">format data like 00:01:01,132 -> 00:01:04,345</param>
        /// <returns>format data like 00:01:04,345</returns>
        private string GetToData(string fromto)
        {
            return fromto.Split('-', '>')[3].Trim();
        }
    }
}
