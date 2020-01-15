using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace VTT
{
    /// <summary>
    /// 字幕的工具类
    /// </summary>
    public class VTTHelper
    {
        private static readonly string[] _delimiters = new string[] { "-->", "- >", "->" };

        /// <summary>
        /// 从文件里读取一个VTT的文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>

        public static Vtt GetVTTFromFile(string fileName)
        {
            return GetVTTFromFile2<Vtt>(fileName);
        }

        /// <summary>
        /// 从文件里读取一个VTT的文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T GetVTTFromFile2<T>(string fileName) where T : Vtt, new()
        {
            if (!File.Exists(fileName))
                return null;

            var vtt = new T();

            var reader = File.ReadLines(fileName).GetEnumerator();
            if (!(reader.MoveNext() && reader.Current == "WEBVTT"))
            {
                return null;
            }

            // read first file property
            while (reader.MoveNext() && !string.IsNullOrEmpty(reader.Current))
            {
                var line = reader.Current;

                if (reader.Current.Contains("Kind:"))
                {
                    vtt.Kind = line.Replace("Kind:", "").Trim();
                }

                if (reader.Current.Contains("Language:"))
                {
                    vtt.Language = line.Replace("Language:", "").Trim();
                }
            }

            VttLine preline = null;
            bool checkFileStats = false;
            bool googleVTT = true;

            while (reader.MoveNext())
            {
                var readLines = new List<string>();
                while (!string.IsNullOrEmpty(reader.Current))
                {
                    readLines.Add(reader.Current);

                    if (!reader.MoveNext())
                    {
                        break;
                    }
                }

                if (readLines.Count <= 1)
                    continue;

                if (!checkFileStats && readLines.Count > 1)
                {
                    var firstLine = readLines[0];
                    if (string.IsNullOrEmpty(firstLine.Regx("align:")) && string.IsNullOrEmpty(firstLine.Regx("position:")))
                    {
                        // 如果没有正则到 谷歌vtt的位置信息，则字幕可能是用户上传的，处理模式应该类似于srt
                        googleVTT = false;
                    }
                }
                if (googleVTT)
                {
                    if (readLines.Count >= 3)
                    {
                        var line = new VttLine();

                        // 第一行是时间格式
                        var l1 = readLines[0].Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
                        if (l1.Length >= 2)
                        {
                            line.From = l1[0].RegTime();
                            if (preline != null)
                            {
                                // 因为 google 会存在一个独立的字幕行段，只有 0.01s
                                // 会导致解析的字幕会断针，所以这里把整个字幕向前修正
                                var d = line.FromSec - preline.ToSec;
                                if (d < 1)
                                {
                                    line.From = preline.To;
                                }
                            }

                            line.To = l1[1].RegTime();
                        }

                        // 第二行是上一段的正文
                        // 这里先忽略

                        // 本次的内容
                        var reg = new Regex("<[0-9]+:[0-9]+:[0-9]+[,\\.][0-9]+>"); // 单词是通过时间做间隔的
                        var ms = reg.Matches(readLines[2]).Select(o => o.Value).ToArray();
                        if (ms.Length > 0)
                        {
                            // 能截取到时间，则进行后续处
                            var words = readLines[2].Split(ms, StringSplitOptions.RemoveEmptyEntries);
                            if (ms.Length + 1 != words.Length)
                            {
                                continue; // 理论上时间的长度要小于单词的长度
                            }

                            line.Words.Add(new VttWord
                            {
                                Text = words[0],
                                From = line.From,
                                To = ms[0].Trim('<', '>'),
                            });

                            for (var i = 1; i < words.Length - 1; i++)
                            {
                                line.Words.Add(new VttWord
                                {
                                    Text = words[i].TrimString("<c>", "</c>").Trim(),
                                    From = ms[i - 1].Trim('<', '>'),
                                    To = ms[i].Trim('<', '>'),
                                });
                            }

                            line.Words.Add(new VttWord
                            {
                                Text = words[words.Length - 1].TrimString("<c>", "</c>"),
                                From = ms[words.Length - 2].Trim('<', '>'),
                                To = line.To,
                            });

                            // word 在上面取消了空格，这里输出text的时候，得补充回来
                            line.Text = string.Join(" ", line.Words.Select(o => o.Text));

                            vtt.Lines.Add(line);

                            preline = line;
                        }
                        else
                        {
                            // 没有截取时间，那么，还有一种可能，就是 [MUSIC] 过度，这里要用一个时间做一下判断
                            if (line.Duration > 1)
                            {
                                line.Text = readLines[2];
                                if (readLines.Count > 3)
                                {
                                    line.Text += readLines[3];
                                }
                                line.Words.Add(new VttWord
                                {
                                    From = line.From,
                                    To = line.To,
                                    Text = line.Text
                                });
                                vtt.Lines.Add(line);
                                preline = line;
                            }
                        }
                    }

                    if (readLines.Count == 2)
                    {
                        // 只有2行也是可能的，大多是google翻译后的文件内容
                        var line = new VttLine();

                        // 第一行是时间格式
                        var l1 = readLines[0].Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
                        if (l1.Length >= 2)
                        {
                            line.From = l1[0].RegTime();
                            if (preline != null)
                            {
                                // 因为 google 会存在一个独立的字幕行段，只有 0.01s
                                // 会导致解析的字幕会断针，所以这里把整个字幕向前修正
                                var d = line.FromSec - preline.ToSec;
                                if (d < 1)
                                {
                                    line.From = preline.To;
                                }
                            }

                            line.To = l1[1].RegTime();
                        }

                        // 第二行就是正文了，后面的单词word，参考srt的切割方案进行处理
                        line.Text = readLines[1];
                    }
                }
                else
                {
                    // srt模式
                    var line = new VttLine();

                    // 第一行是时间格式
                    var l1 = readLines[0].Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
                    if (l1.Length >= 2)
                    {
                        line.From = l1[0].RegTime();
                        if (preline != null)
                        {
                            // 因为 google 会存在一个独立的字幕行段，只有 0.01s
                            // 会导致解析的字幕会断针，所以这里把整个字幕向前修正
                            var d = line.FromSec - preline.ToSec;
                            if (d < 1)
                            {
                                line.From = preline.To;
                            }
                        }

                        line.To = l1[1].RegTime();
                    }

                    for (var i = 1; i < readLines.Count; i++)
                        line.Text += readLines[i] + " ";

                    vtt.Lines.Add(line);
                    preline = line;
                }
            }

            var id = 1;
            vtt.Lines.ForEach(o => o.Id = id++);
            return vtt;
        }

    }
    public static class RegEx
    {
        public static string Regx(this string source, string reg)
        {
            var regx = new Regex(reg);
            return regx.Match(source).Value;
        }

        const string TimeReg = "[0-9]+:[0-9]+:[0-9]+[,\\.][0-9]+";

        public static string RegTime(this string source)
        {
            return Regx(source, TimeReg);
        }

        public static string TrimString(this string source, params string[] trims)
        {
            if (string.IsNullOrEmpty(source))
                return source;

            foreach(var item in trims)
            {
                if (source.IndexOf(item) == 0)
                    source = source.Substring(item.Length, source.Length - item.Length);
                if (source.LastIndexOf(item) > -1)
                    source = source.Substring(0, source.Length - item.Length);
            }

            return source;
        }
    }
}
