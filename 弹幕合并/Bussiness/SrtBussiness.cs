using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using 弹幕合并.Bussiness.Database;
using 弹幕合并.Bussiness.Entity;
using ServiceStack.OrmLite;
using BaiduFanyi;
using 弹幕合并.Common;
using Dogvane.Srt;
using VTT;
using System.Threading;
using 弹幕合并.Bussiness.Subtitles;
using TimeExtend = 弹幕合并.Common.TimeExtend;

namespace 弹幕合并.Bussiness
{
    public class SrtBussiness
    {
        static SrtBussiness()
        {
            using (var db = DbSet.GetDb())
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

        private ITransApi api;

        public SrtBussiness()
        {
            api = TransFac.GetCurrentTransApi();
        }

        public SrtFile[] GetSrtFiles(int userId)
        {
            using (var db = DbSet.GetDb())
            {
                return db.Select<SrtFile>(o => o.UserId == userId).ToArray();
            }
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

            TransLine(line);
            DelaySaveFile(ret.srtFile, ret.jsonObj);

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

            // 去掉前后空格，方便后续处理
            source = source.Trim();
            replace = replace.Trim();

            var sarr = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var rarr = replace.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (sarr.Length == 0 || rarr.Length == 0)
                return ("替换和被替换字符串不能为空", null);

            if(rarr.Length > sarr.Length)
                return ("替换的内容长度切割数量要小于被替换内容切割数量", null);

            foreach (var line in ret.jsonObj.Battutas)
            {
                if (line.Text.IndexOf(source) > -1) // 这里先做初步的简单快速过滤
                {
                    // line.Text = line.Text.Replace(source, replace);
                    // 切换翻译内容后，则可能存在一个替换时，词组替换为单个词的问题
                    // 因此这里不能做简单的翻译替换处理了，必须是整个词组的替换
                    var index = line.Words.IntersectIndex(sarr, (t, o) => t.Text == o);
                    if(index == -1)                    
                        continue;
                    // 这里，会有一个等数量替换和非对等替换的情况存在
                    var s = line.Text;

                    if(sarr.Length == rarr.Length)
                    {
                        // 等数量的单词替换，则单词内容进行替换，单词的时间维度不做修改
                        for(var i=0;i < rarr.Length; i++)
                        {
                            line.Words[index + i].Text = rarr[i];
                        }
                    }
                    else
                    {
                        // 非对等时间变化，则存在单词时间的变化，这里只能简单的重建时间切割了。
                        var newWords = SplitWords(replace, line.Words[index].From, line.Words[index + sarr.Length - 1].To);
                        line.Words.RemoveRange(index, sarr.Length);
                        line.Words.InsertRange(index, newWords);
                    }

                    line.Text = string.Join(" ", line.Words.Select(o => o.Text));
                    Console.WriteLine($"replace {s} --> {line.Text}");

                    TransLine(line);
                    retDatas.Add(line);
                }
            }

            if (retDatas.Count > 0)
            {
                DelaySaveFile(ret.srtFile, ret.jsonObj);
            }

            return (null, retDatas.ToArray());
        }

        DateTime nextRunTimes = DateTime.Now;

        void TransLine(TransBattuta line)
        {
            bool notTrnas = string.IsNullOrEmpty(line.Trans2) || line.Trans == line.Trans2;
            try
            {
                if (nextRunTimes > DateTime.Now)
                    return;

                var trans = api.GetTrans(line.Text, "en", "zh");
                if (!string.IsNullOrEmpty(trans))
                {
                    line.Trans = trans;
                    Console.WriteLine(trans);
                    if (notTrnas)
                        line.Trans2 = line.Trans;
                }
            }
            catch (Exception ex)
            {
                nextRunTimes = DateTime.Now.AddMinutes(1); // 发生错误，就等1分钟后，才能进行翻译处理
                Logger.Error($"翻译接口有问题: {ex.Message}");
            }
        }

        /// <summary>
        /// 字幕翻译
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="srtId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
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

            TransLine(line);

            DelaySaveFile(ret.srtFile, ret.jsonObj);
            return (null, new[] {line});
        }

        /// <summary>
        /// 字幕翻译（多行）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="srtId"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
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

                TransLine(line);                

                rettb.Add(line);
            }

            DelaySaveFile(ret.srtFile, ret.jsonObj);
            return (null, rettb.ToArray());
        }

        /// <summary>
        /// 翻译所有的内容
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="srtId"></param>
        /// <returns></returns>
        public (string error, TransBattuta[] battuta) SrtTransAll(int userId, int srtId)
        {
            var ret = GetSrt(userId, srtId);
            if (ret.error != null)
                return (ret.error, null);
            List<TransBattuta> rettb = new List<TransBattuta>();

            ThreadPool.QueueUserWorkItem(o =>
            {
                foreach (var item in ret.jsonObj.Battutas)
                {
                    if (string.IsNullOrEmpty(item.Trans))
                    {
                        TransLine(item);
                    }
                }

                DelaySaveFile(ret.srtFile, ret.jsonObj);
            });
            
            return (null, rettb.ToArray());
        }

        /// <summary>
        /// 将当行的首单词，放到上一行的结尾
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="srtId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public (string error, TransBattuta[] battuta) WordUp(int userId, int srtId, int id)
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
                    
                    if (line.Words.Count == 1)
                    {
                        // 提升会导致自己的数据被删除

                        preLine.Words.AddRange(line.Words);
                        preLine.To = preLine.Words.Last().To;
                        preLine.Text = string.Join(" ", preLine.Words.Select(o => o.Text));

                        TransLine(preLine);

                        ret.jsonObj.Battutas.RemoveAt(i);

                        // 这行清空后返回
                        line.Words.Clear();
                        line.Text = string.Empty;

                        DelaySaveFile(ret.srtFile, ret.jsonObj);
                        return (null, new[] { line, preLine });
                    }
                    else
                    {
                        // 只提升一个单词
                        var firstWord = line.Words.First();
                        line.Words.RemoveAt(0);
                        preLine.Words.Add(firstWord);

                        line.Text = string.Join(" ", line.Words.Select(o => o.Text));
                        preLine.Text = string.Join(" ", preLine.Words.Select(o => o.Text));

                        line.From = line.Words.First().From;
                        preLine.To = preLine.Words.Last().To;

                        DelaySaveFile(ret.srtFile, ret.jsonObj);
                        return (null, new[] {line, preLine});
                    }
                }
            }

            return ($"{id} 行错误", null);
        }

        public (string error, TransBattuta[] battuta) LineUp(int userId, int srtId, int id)
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

                    preLine.Words.AddRange(line.Words);
                    preLine.Text = string.Join(" ", preLine.Words.Select(o => o.Text));
                    preLine.To = line.To;
                    TransLine(preLine);

                    line.Text = string.Empty; // 空串数据也要返回的
                    line.Words.Clear();

                    ret.jsonObj.Battutas.RemoveAt(i);

                    DelaySaveFile(ret.srtFile, ret.jsonObj);
                    return (null, new[] {line, preLine});
                }
            }

            return ($"{id} 行错误", null);
        }

        public (string error, TransBattuta[] battuta) WordDown(int userId, int srtId, int id)
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
                    if (line.Words.Count == 1)
                    {
                        // 下降会导致当前行被删除
                        line.Text = string.Empty;

                        var word = line.Words.Last();
                        nextLine.Words.Insert(0, word);
                        nextLine.From = line.From;
                        nextLine.Text = string.Join(" ", nextLine.Words.Select(o=>o.Text));
                        
                        TransLine(nextLine);

                        ret.jsonObj.Battutas.RemoveAt(i);
                        
                        DelaySaveFile(ret.srtFile, ret.jsonObj);
                        return (null, new[] { line, nextLine });
                    }
                    else
                    {
                        var word = line.Words.Last();
                        nextLine.Words.Insert(0, word);
                        nextLine.From = word.From;

                        line.Words.RemoveAt(line.Words.Count - 1);
                        line.To = word.From;

                        nextLine.Text = string.Join(" ", nextLine.Words.Select(o => o.Text));
                        line.Text = string.Join(" ", line.Words.Select(o => o.Text));

                        DelaySaveFile(ret.srtFile, ret.jsonObj);
                        return (null, new[] {line, nextLine});
                    }
                }
            }

            return ($"{id} 行错误", null);
        }

        public (string error, TransBattuta[] battuta) LineDown(int userId, int srtId, int id)
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
                    line.Words.AddRange(nextLine.Words);
                    nextLine.Words = line.Words;

                    nextLine.Text = string.Join(" ", nextLine.Words.Select(o => o.Text));                    
                    nextLine.From = line.From;
                    TransLine(nextLine);

                    ret.jsonObj.Battutas.RemoveAt(i);

                    line.Words = new List<Subtitles.Word>();
                    line.Text = string.Empty; // 被删除的行，也需要返回，并设置为空串，这样前端就可以删除这行了

                    DelaySaveFile(ret.srtFile, ret.jsonObj);
                    return (null, new[] { line, nextLine });
                }
            }

            return ($"{id} 行错误", null);
        }

        List<TransBattuta> UpWrodsVersion(List<TransBattuta> battutas)
        {
            foreach (var item in battutas)
            {
                if (item.Words.Count > 0)
                    continue;

                // 这里要对原先的文本做切割，用来和vtt的保持一致
                var words = item.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var textLength = (float)words.Sum(o => o.Length);
                var start = item.From;

                foreach (var w in words)
                {
                    var sec = item.Duration * (w.Length / textLength); // 时间暂时按照单词长度做切割
                    var addWord = new Subtitles.Word
                    {
                        From = start,
                        Text = w,
                    };

                    addWord.To = TimeSpan.FromSeconds(addWord.FromSec + sec).ToString(@"hh\:mm\:ss\,fff");
                    start = addWord.To;
                    item.Words.Add(addWord);
                }

                item.Words.Last().To = item.To;   // 因为百分比切割还是会有误差，这里最后把的和字幕的放到一起
            }

            return battutas;
        }

        private List<Word> SplitWords(string linsText, string from, string to)
        {
            var words = linsText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var textLength = (float)words.Sum(o => o.Length);
            var start = from;

            var duration = TimeExtend.GetDuration(from, to);
            List<Word> ret = new List<Word>();

            foreach (var w in words)
            {
                var sec = duration * (w.Length / textLength); // 时间暂时按照单词长度做切割
                var addWord = new Subtitles.Word
                {
                    From = start,
                    Text = w,
                };

                addWord.To = TimeSpan.FromSeconds(addWord.FromSec + sec).ToString(@"hh\:mm\:ss\,fff");
                start = addWord.To;
                ret.Add(addWord);
            }

            ret.Last().To = to;   // 因为百分比切割还是会有误差，这里最后把的和字幕的放到一起

            return ret;
        }

        List<TransBattuta> ConverSrtToSubtitles(List<Battuta> battutas)
        {
            var ret = new List<TransBattuta>(battutas.Count);

            foreach(var item in battutas)
            {
                var tb = new TransBattuta()
                {
                    Id = item.Id,
                    From = item.From,
                    To = item.To,
                    Text = item.Text
                };

                // 这里要对原先的文本做切割，用来和vtt的保持一致
                tb.Words = SplitWords(item.Text, item.From, item.To);

                ret.Add(tb);
            }

            return ret;
        }

        List<TransBattuta> ConverVttToSubtitles(List<VTT.VttLine> battutas)
        {
            var ret = new List<TransBattuta>(battutas.Count);

            foreach (var item in battutas)
            {
                var tb = new TransBattuta()
                {
                    Id = item.Id,
                    From = item.From,
                    To = item.To,
                    Text = item.Text
                };

                foreach(var w in item.Words)
                {
                    tb.Words.Add(new Subtitles.Word 
                    { 
                        From = w.From,
                        To = w.To,
                        Text = w.Text,                        
                    });
                }
                ret.Add(tb);
            }

            return ret;
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
            var fileBattuate = new SrtManager().LoadBattuteByFile(saveFile);

            // 因为本地文件的srt与原始的不一样了，这里要做一下转换
            var jsonObj = new SrtJsonFile();
            jsonObj.Battutas = ConverSrtToSubtitles(fileBattuate);
            jsonObj.FileName = fileName;
            jsonObj.Version = 1;

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

        /// <summary>
        /// 上传一个vtt文件
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="context"></param>
        /// <param name="ismerge">是否合并弹幕</param>
        /// <returns></returns>
        public SrtFile UploadVttFile(int userId, string fileName, byte[] context, bool ismerge = true)
        {
            var saveFile = Path.Combine("upfiles/", userId + "_" + fileName);

            File.WriteAllBytes(saveFile, context);

            // 加载vtt文件
            var vtt = VTTHelper.GetVTTFromFile(saveFile);

            var jsonObj = new SrtJsonFile();
            jsonObj.Battutas = ConverVttToSubtitles(vtt.Lines);
            jsonObj.FileName = fileName;
            jsonObj.Version = 1;

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

        /// <summary>
        /// 获得一个字幕文件数据
        /// 数据会被缓存在内存里
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="srtId"></param>
        /// <returns></returns>
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
                    if(obj.Version == 0)
                    {
                        // 旧的文件要先升级先
                        UpWrodsVersion(obj.Battutas);
                        obj.Version = 1;
                        Console.WriteLine($"up version. {srtfile.JsonSrtFileName}");
                    }
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

        Dictionary<string, SrtJsonFile> delaySaveFileMap = new Dictionary<string, SrtJsonFile>();

        DateTime nextSaveTime = DateTime.Now;

        bool isSaveing = false;

        /// <summary>
        /// 延迟保存文件内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        void DelaySaveFile(SrtFile srtFile, SrtJsonFile jsonObj)
        {
            lock (delaySaveFileMap)
            {
                delaySaveFileMap[srtFile.JsonSrtFileName] = jsonObj;
            }

            if (isSaveing)
                return;

            isSaveing = true;
            ThreadPool.QueueUserWorkItem(o =>
            {
                // 延迟1分钟后统一做一次数据保存
                Thread.Sleep(1000 * 60);    // 你没看错，我就是这么牛，这么写延迟保存

                try
                {
                    lock (delaySaveFileMap)
                    {
                        foreach (var item in delaySaveFileMap)
                        {
                            File.WriteAllText(item.Key, JsonConvert.SerializeObject(item.Value));
                            Console.WriteLine($"save file:{item.Key}");
                        }
                        delaySaveFileMap.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                isSaveing = false;
            });
        }
    }
}
