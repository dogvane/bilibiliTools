using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dogvane.Srt;
using Newtonsoft.Json;
using 弹幕合并.Bussiness.Database;
using 弹幕合并.Bussiness.Entity;
using ServiceStack.OrmLite;
using BaiduFanyi;

namespace 弹幕合并.Bussiness
{
    public class SrtBussiness
    {
        static SrtBussiness()
        {
            using(var db = DbSet.GetDb())
            {
                db.CreateTableIfNotExists<SrtFile>();
            }

            var dir = new DirectoryInfo("upfiles/");
            if (!dir.Exists)
            {
                dir.Create();
            }

            dir = new DirectoryInfo("jsonfiles/");
            if (!dir.Exists)
            {
                dir.Create();
            }

            Directory.CreateDirectory("jsonfiles");
            Directory.CreateDirectory("upfiles");
        }

        private TransApi api;

        public SrtBussiness()
        {
            api = new TransApi();
        }

        public SrtFile[] GetSrtFiles(int userId)
        {
            using (var db = DbSet.GetDb())
            {
                return db.Select<SrtFile>(o => o.UserId == userId).ToArray();
            }
        }

        private List<TransBattuta> 合并文件字幕(string fileName)
        {
            var srt = new SrtManagerT<TransBattuta>();
            var battute = srt.LoadBattuteByFile(fileName);
            var rets = 合并字幕(battute, 5.1);
            //rets = 合并字幕(rets, 6.0);
            return rets;
        }

        private static List<TransBattuta> 合并字幕(List<TransBattuta> battute, double timeSpan = 5.0, bool fixText = false)
        {
            var rets = new List<TransBattuta>();

            for (var i = 0; i < battute.Count - 1; i += 1)
            {
                var item1 = battute[i];
                var item2 = battute[i + 1];

                // 如果2个字幕总显示时间的间隔在5s以内，则进行合并
                var span = item2.ToSec - item1.FromSec;
                if (span > timeSpan)
                {
                    item1.Text = item1.Text;
                    if (fixText)
                        item1.Text = item1.Text.Replace(" ", "");
                    else
                        item1.Text = item1.Text.Trim();

                    rets.Add(item1);
                    continue;
                }

                var text = (item1.Text + " " + item2.Text);
                if (fixText)
                {
                    text = text.Replace(" ", "");
                }

                rets.Add(new TransBattuta(0, item1.From, item2.To, text));
                i += 1;
            }

            var index = 1;
            foreach (var item in rets)
            {
                item.Id = index++;
                //Console.WriteLine("{0:F2}  {1}  {2}", item.Duration, item.Text.Length, item.Text);
            }

            return rets;
        }

        private static Dictionary<int, SrtJsonFile> jsonFileCache = new Dictionary<int, SrtJsonFile>();

        /// <summary>
        /// 更新一个用户自己的翻译
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="srtId">字幕文件id</param>
        /// <param name="id">字幕编号</param>
        /// <param name="trans">翻译内容</param>
        /// <returns></returns>
        public (string error, TransBattuta[] battuta) UpdateTrans(int userId, int srtId, int id, string trans)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            var line = ret.jsonObj.Battutas.FirstOrDefault(o => o.Id == id);
            if (line == null)
            {
                return ($"{id} 行错误", null);
            }

            line.Trans2 = trans;
            
            File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
            return (null, new[] { line });
        }

        /// <summary>
        /// 更新字幕的原始值
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="srtId">字幕文件id</param>
        /// <param name="id">字幕编号</param>
        /// <param name="text">翻译内容</param>
        /// <returns></returns>
        public (string error, TransBattuta[] battuta) UpdateSource(int userId, int srtId, int id, string text)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            var line = ret.jsonObj.Battutas.FirstOrDefault(o => o.Id == id);
            if (line == null)
            {
                return ($"{id} 行错误", null);
            }

            line.Text = text;

            bool notTrnas = string.IsNullOrEmpty(line.Trans2) || line.Trans == line.Trans2;
            line.Trans = api.GetTransResult3(line.Text, "en", "zh");
            if (notTrnas)
                line.Trans2 = line.Trans;

            File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
            return (null, new[] { line });
        }

        /// <summary>
        /// 更新字幕的原始值
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="srtId">字幕文件id</param>
        /// <param name="source">搜索的字符串</param>
        /// <param name="replace">目标字符串</param>
        /// <returns></returns>
        public (string error, TransBattuta[] battuta) ReplaceSource(int userId, int srtId, string source, string replace)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            List<TransBattuta> retDatas = new List<TransBattuta>();

            foreach (var line in ret.jsonObj.Battutas)
            {
                if (line.Text.IndexOf(source) > -1)
                {
                    line.Text = line.Text.Replace(source, replace);

                    bool notTrnas = string.IsNullOrEmpty(line.Trans2) || line.Trans == line.Trans2;
                    line.Trans = api.GetTransResult3(line.Text, "en", "zh");
                    if (notTrnas)
                        line.Trans2 = line.Trans;
                    retDatas.Add(line);
                }
            }

            File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
            return (null, retDatas.ToArray());
        }

        public (string error, TransBattuta[] battuta) SrtTrans(int userId, int srtId, int id)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            var line = ret.jsonObj.Battutas.FirstOrDefault(o => o.Id == id);
            if (line == null)
            {
                return ($"{id} 行错误", null);
            }

            bool notTrnas = string.IsNullOrEmpty(line.Trans2) || line.Trans == line.Trans2;
            line.Trans = api.GetTransResult3(line.Text, "en", "zh");
            if (notTrnas)
                line.Trans2 = line.Trans;

            File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
            return (null, new[] {line});
        }

        public (string error, TransBattuta[] battuta) SrtTrans2(int userId, int srtId, int[] ids)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);
            List<TransBattuta> rettb = new List<TransBattuta>();

            foreach (var id in ids)
            {
                var line = ret.jsonObj.Battutas.FirstOrDefault(o => o.Id == id);
                if (line == null || rettb.Any(o=>o.Id == id))
                {
                    continue;
                }

                bool notTrnas = string.IsNullOrEmpty(line.Trans2) || line.Trans == line.Trans2;
                line.Trans = api.GetTransResult3(line.Text, "en", "zh");
                if (notTrnas)
                    line.Trans2 = line.Trans;

                rettb.Add(line);
            }

            File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
            return (null, rettb.ToArray());
        }

        /// <summary>
        /// 将当行的首单词，放到上一行的结尾
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="srtId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public (string error, TransBattuta[] battuta) SrtUp(int userId, int srtId, int id)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            for (var i = 0; i < ret.jsonObj.Battutas.Count; i++)
            {
                var line = ret.jsonObj.Battutas[i];
                if (line.Id == id)
                {
                    if (i == 0)
                        return ("首行不能提升", null);

                    var preLine = ret.jsonObj.Battutas[i - 1];
                    var firstWord = line.Text.Split(' ').FirstOrDefault();
                    if (firstWord == line.Text)
                    {
                        // 提升会导致自己的数据被删除
                        line.Text = string.Empty;
                        preLine.Text += " " + firstWord;
                        preLine.Trans = api.GetTransResult3(preLine.Text, "en", "zh");
                        preLine.To = line.To;
                        ret.jsonObj.Battutas.RemoveAt(i);
                        return (null, new[] { line, preLine });
                    }
                    else
                    {
                        var lineLength = line.Text.Length;
                        line.Text = line.Text.Remove(0, firstWord.Length + 1);
                        preLine.Text += " " + firstWord;

                        //preLine.Trans = api.GetTransResult3(preLine.Text, "en", "zh");
                        //line.Trans = api.GetTransResult3(line.Text, "en", "zh");

                        var sec = line.Duration * (1 - line.Text.Length * 1.0 / lineLength); // 目前先按照字符长度等比例减少时间的值
                        Console.WriteLine("up sec {0}", sec);
                        preLine.To = TimeSpan.FromSeconds(preLine.ToSec + sec).ToString(@"hh\:mm\:ss\,fff");
                        line.From = preLine.To;

                        File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
                        return (null, new[] {line, preLine});
                    }
                }
            }

            return ($"{id} 行错误", null);
        }

        public (string error, TransBattuta[] battuta) SrtLineUp(int userId, int srtId, int id)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            for (var i = 0; i < ret.jsonObj.Battutas.Count; i++)
            {
                var line = ret.jsonObj.Battutas[i];
                if (line.Id == id && line.Text != string.Empty)
                {
                    if (i == 0)
                        return ("首行不能提升", null);

                    var preLine = ret.jsonObj.Battutas[i - 1];

                    preLine.Text += " " + line.Text;
                    preLine.Trans = api.GetTransResult3(preLine.Text, "en", "zh");
                    preLine.To = line.To;

                    line.Text = string.Empty; // 空串数据也要返回的
                    ret.jsonObj.Battutas.RemoveAt(i);
                    return (null, new[] {line, preLine});
                }
            }

            return ($"{id} 行错误", null);
        }

        public (string error, TransBattuta[] battuta) SrtDown(int userId, int srtId, int id)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            for (var i = 0; i < ret.jsonObj.Battutas.Count; i++)
            {
                var line = ret.jsonObj.Battutas[i];
                if (line.Id == id)
                {
                    if (i == ret.jsonObj.Battutas.Count - 1)
                        return ("末行不能下降", null);

                    var nextLine = ret.jsonObj.Battutas[i + 1];
                    var lastWord = line.Text.Split(' ').LastOrDefault();
                    if (lastWord == line.Text)
                    {
                        // 下降会导致当前行被删除
                        line.Text = string.Empty;
                        nextLine.Text = lastWord + " " + nextLine.Text;
                        nextLine.Trans = api.GetTransResult3(nextLine.Text, "en", "zh");
                        nextLine.From = line.From;
                        ret.jsonObj.Battutas.RemoveAt(i);
                        return (null, new[] { line, nextLine });
                    }
                    else
                    {
                        var lineLength = line.Text.Length;
                        line.Text = line.Text.Remove(line.Text.Length - lastWord.Length - 1, lastWord.Length + 1);
                        nextLine.Text = lastWord + " " + nextLine.Text;

                        //nextLine.Trans = api.GetTransResult3(nextLine.Text, "en", "zh");
                        //line.Trans = api.GetTransResult3(line.Text, "en", "zh");

                        var sec = line.Duration * (1 - line.Text.Length * 1.0 / lineLength); // 目前先按照字符长度等比例减少时间的值
                        Console.WriteLine("down sec {0}", sec);
                        line.To = TimeSpan.FromSeconds(line.ToSec - sec).ToString(@"hh\:mm\:ss\,fff");
                        nextLine.From = line.To;

                        File.WriteAllText(ret.srtFile.JsonSrtFileName, JsonConvert.SerializeObject(ret.jsonObj));
                        return (null, new[] {line, nextLine});
                    }
                }
            }

            return ($"{id} 行错误", null);
        }

        public (string error, TransBattuta[] battuta) SrtLineDown(int userId, int srtId, int id)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);

            for (var i = 0; i < ret.jsonObj.Battutas.Count; i++)
            {
                var line = ret.jsonObj.Battutas[i];
                if (line.Id == id && line.Text != string.Empty)
                {
                    if (i == ret.jsonObj.Battutas.Count - 1)
                        return ("末行不能下降", null);

                    var nextLine = ret.jsonObj.Battutas[i + 1];
                    nextLine.Text = line.Text + " " + nextLine.Text;
                    nextLine.Trans = api.GetTransResult3(nextLine.Text, "en", "zh");
                    nextLine.From = line.From;
                    ret.jsonObj.Battutas.RemoveAt(i);
                    line.Text = string.Empty; // 被删除的行，也需要返回，并设置为空串，这样前端就可以删除这行了
                    return (null, new[] { line, nextLine });
                }
            }

            return ($"{id} 行错误", null);
        }

        /// <summary>
        /// 上传一个srt文件
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="context"></param>
        /// <param name="ismerge">是否合并弹幕</param>
        /// <returns></returns>
        public SrtFile UploadSrtFile(int userId, string fileName, byte[] context, bool ismerge = true)
        {
            var saveFile = Path.Combine("upfiles/", userId + "_" + fileName);

            File.WriteAllBytes(saveFile, context);
            List<TransBattuta> battuata;
            if (ismerge)
                battuata = 合并文件字幕(saveFile);
            else
                battuata = new SrtManagerT<TransBattuta>().LoadBattuteByFile(saveFile);

            var jsonObj = new SrtJsonFile();
            jsonObj.Battutas = battuata;
            jsonObj.FileName = fileName;

            var jsonSaveFile = Path.Combine("jsonfiles/", userId + "_" + fileName + ".json");

            File.WriteAllText(jsonSaveFile, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));

            var srtItem = new SrtFile()
            {
                UserId = userId,
                LastUpdate = DateTime.Now,
                UploadTime = DateTime.Now,
                SourceLocalFileName = saveFile,
                SrtFileName = fileName,
                JsonSrtFileName = jsonSaveFile
            };

            using (var db = DbSet.GetDb())
            {
                db.Insert(srtItem);
            }

            return srtItem;
        }

        public (string error, SrtFile srtFile, SrtJsonFile jsonObj) GetSrt(int userid, int srtId)
        {
            using (var db = DbSet.GetDb())
            {
                var srtfile = db.Single<SrtFile>(o => o.Id == srtId);
                if (srtfile == null)
                    return ("没找到字幕", null, null);

                if (srtfile.UserId != userid)
                    return ("用户id错误", null, null);

                if (!jsonFileCache.TryGetValue(srtId, out SrtJsonFile obj))
                {
                    var jsonFile = srtfile.JsonSrtFileName;
                    var jsonStr = File.ReadAllText(jsonFile);
                    obj = JsonConvert.DeserializeObject<SrtJsonFile>(jsonStr);
                    if (obj == null)
                        return ("json文件损坏，需要重新上传", null, null);
                }

                jsonFileCache[srtId] = obj;
                return (null, srtfile, obj);
            }
        }

        /// <summary>
        /// 删除一个字幕
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="srtId"></param>
        /// <returns></returns>
        public string DeleteSrt(int userid, int srtId)
        {
            using (var db = DbSet.GetDb())
            {
                var srtfile = db.Single<SrtFile>(o => o.Id == srtId);
                if (srtfile == null)
                    return "没找到字幕";

                if (srtfile.UserId != userid)
                    return "用户id错误";

                if(File.Exists(srtfile.JsonSrtFileName))
                    File.Delete(srtfile.JsonSrtFileName);

                if (File.Exists(srtfile.SourceLocalFileName))
                    File.Delete(srtfile.SourceLocalFileName);

                db.Delete(srtfile);

                return "";
            }
        }
    }
}
