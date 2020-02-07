using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTT;

namespace VVTRebuild
{
    class VvtRebuild
    {
        private static int s_id = 0;

        private static List<List<string>> sws = null;

        private static List<List<string>> ws = null;

        private static List<List<string>> StartSingleWordLists
        {
            get
            {
                if (sws == null)
                {
                    List<string> splitStartWorld = new List<string>()
            {
                    "but", "and", "that", "so", "we", "I", "you","or"
            };

                    splitStartWorld = splitStartWorld.OrderByDescending(o => o.Length).ToList();

                    sws = new List<List<string>>();
                    foreach (var s in splitStartWorld)
                    {
                        sws.Add(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList());
                    }
                }
                return sws;
            }
        }

        /// <summary>
        /// 按照输入进行多词交叉组合
        /// so but and okey
        /// I we you all it
        /// can can't  have has 
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        static List<string> GetWords(params string[] words)
        {
            if (words.Length == 0)
                return new List<string>();

            if (words.Length == 1)
                return words[0].Split(new[] { ',', ' ',';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var ret = new List<List<string>>();

            var s = words.Select(
                o => o.Split(new[] { ',',' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x=>x.Replace("`", " "))
                .ToList()).ToArray();

            return GetWords(s);
        }

        static List<string> GetWords(params List<string>[] datas)
        {
            if (datas.Length == 0)
                return new List<string>();

            if (datas.Length == 1)
                return datas[0];

            if (datas.Length == 2)
            {
                List<string> l1 = datas[0];
                List<string> l2 = datas[1];

                // 做交叉后返回
                List<string> ret = new List<string>();
                foreach (var x1 in l1)
                {
                    foreach (var x2 in l2)
                    {
                        ret.Add(x1 + " " + x2);
                    }
                }

                return ret;
            }

            // 超过3个，则先合并后面的数据，在和自己的数据做合并

            return GetWords(datas[0], GetWords(datas.ToList().GetRange(1, datas.Length - 1).ToArray()));
        }

        private static List<List<string>> StartWordLists
        {
            get
            {
                if (ws == null)
                {
                    ws = new List<List<string>>();

                    // 三个词组的

                    List<string> gs = new List<string>();

                    var so_but_and_that = "so but and okay that now if";
                    var I_we_you_all = "I i we you all it he her they there";
                    var Ill_youd_its = "I'll you'd it's let's we're you're I'ant you've there's we'll";
                    var can_have_has = "can can't don't do`not have has know is will did are kind want wanna should allow"
                        + " also most think said doesn't gonna coming got get just talk  "
                        + " need`to not`gonna out`of hear`for any`of talked`to";

                    var what_when_where = "what when where which there whatever what's let let's";
                    gs.AddRange(GetWords(
                        so_but_and_that,
                        I_we_you_all,
                        can_have_has
                    ));

                    // 一般的缩写
                    gs.AddRange(GetWords(
                        so_but_and_that,
                        Ill_youd_its
                    ));

                    gs.AddRange(GetWords(
                        so_but_and_that,
                        what_when_where
                    ));
                    gs.AddRange(GetWords(
                        so_but_and_that,
                        can_have_has
                    ));

                    gs.AddRange(GetWords(
                        what_when_where,
                        I_we_you_all,
                        can_have_has
                    ));

                    gs.AddRange(GetWords(
                        what_when_where,
                        I_we_you_all
                    ));

                    gs.AddRange(GetWords(
                        what_when_where,
                        Ill_youd_its
                    ));

                    gs.AddRange(GetWords(
                        I_we_you_all,
                        can_have_has
                    ));

                    gs.AddRange(GetWords(
                        Ill_youd_its,
                        can_have_has
                    ));

                    List<string> splitStartWorld = new List<string>()
            {
                        "and I say",
                        "so now",
                        "so all the",
                        "but also",
                        "now I can",
                        "and I can",
                        "when it",
                        "and now",
                        "if the",
                        "so you'd have",
                        "you'd have",
                        "and that",
                        "so it can't",
                        "but it can",
                        "so when",
                        "and so",
                        "we find",
                        "we have",
                        "but if you",
                        "we can",
                        "but for",
                        "which we",
                        "there it is",
                        "how to",
                        "we will",
                        "I will",
                        "and I'll do",
                        "and if you",
                        "we have to",
                        "what is the",
                "okay",
                "so I",
                "how you're",
                "I have",
                "which is",
                "that's",
                "so just",
                "whatever you",
                "whatever I",
                "when you",
                "we just",
                "I'm gonna",
                "but not",
                "how many",
                "so let's",
                "so we're",
                "so if",
                "so we are",
                "it doesn't",
                "this is",
                "you are",
                "you will",
                "that for",
                "if anyone",
                "just to",
                "so just to",
                "that we",
                "so this is",
                "I know",
                "so that",
                "yeah",
                "let's",
                "hey",
                "it's",
                "I'm",
                "because",
                "and then"
            };

                    splitStartWorld = splitStartWorld.OrderByDescending(o => o.Length).ToList();

                    foreach (var s in gs)
                    {
                        ws.Add(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList());
                    }
                    
                    foreach (var s in splitStartWorld)
                    {
                        ws.Add(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList());
                    }
                }
                return ws;
            }
        }

        public static Vtt Rebuild(Vtt vtt)
        {
            List<VttWord> words = new List<VttWord>();
            foreach (var item in vtt.Lines)
            {
                words.AddRange(item.Words);
            }

            Console.WriteLine(words.Count);

            // 开始逐个的出句子来了

            List<VttLine> result = new List<VttLine>();

            double time = 0;
            // 单词时间不能超过 6s，免得影响观感

            List<VttWord> preList = new List<VttWord>();

            for (var i = 0; i < words.Count; i++)
            {
                if (hasStartWorld(words, i))
                {
                    // 如果之前的词小于4s，则暂时忽略
                    var d = preList.Sum(o => o.Duration);
                    if (d < 3)
                    {
                        preList.Add(words[i]);
                        continue;
                    }

                    if (d > 9) // 时间超过8s，还得想办法拆句子
                    {
                        bool isSplit = false;
                        for (var x = 2; x < preList.Count; x++)
                        {
                            if (hasStartWorld(preList, x))
                            {
                                result.AddRange(spliteSubLine(preList.GetRange(0, x)));
                                result.AddRange(spliteSubLine(preList.GetRange(x, preList.Count - x)));
                                isSplit = true;
                                break;
                            }
                        }

                        if (!isSplit)
                        {
                            result.AddRange(spliteSubLine(preList));
                        }
                    }
                    else
                    {
                        result.Add(GetLine(preList));
                    }

                    preList = new List<VttWord>();
                }

                preList.Add(words[i]);
            }

            if (preList.Count > 0)
            {
                result.Add(GetLine(preList));
            }

            // 再把list 组合成一个 vvtline
            vtt.Lines = result;

            return new Vtt 
            { 
                Language = vtt.Language,
                Kind = vtt.Kind,
                Lines = result,
            };

        }

        private static VttLine GetLine(List<VttWord> words)
        {
            var ret = new VttLine();

            ret.Id = s_id++;
            ret.From = words.First().From;
            ret.To = words.Last().To;
            ret.Text = string.Join(" ", words.Select(o => o.Text));
            ret.Words = words;

            return ret;
        }

        private static bool hasStartSingleWorld(List<VttWord> words, int index)
        {
            foreach (var item in StartSingleWordLists)
            {
                int success = 0;
                for (var i = 0; i < item.Count; i++)
                {
                    if (index + i >= words.Count)
                    {
                        return false;
                    }

                    if (words[index + i].Text != item[i])
                    {
                        break;
                    }
                    success++;
                }

                if (success == item.Count)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool hasStartWorld(List<VttWord> words, int index)
        {
            foreach (var item in StartWordLists)
            {
                int success = 0;
                for (var i = 0; i < item.Count; i++)
                {
                    if (index + i >= words.Count)
                    {
                        return false;
                    }

                    if (words[index + i].Text != item[i])
                    {
                        break;
                    }
                    success++;
                }

                if (success == item.Count)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 这次是强行拆除了
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        private static List<List<VttWord>> spliteBySingleWord(List<VttWord> words)
        {
            var ret = new List<List<VttWord>>();

            List<int> indexs = new List<int>();

            for (var i = 1; i < words.Count; i++)
            {
                if (hasStartSingleWorld(words, i))
                {
                    indexs.Add(i);
                }
            }

            if (indexs.Count == 0)
                return new List<List<VttWord>>() { words };

            // 位置索引后，要找合适的切割方式了。这里从后面往前推，超过4s的，就切出来
            int last = words.Count;
            for (var i = indexs.Count - 1; i >= 0; i--)
            {
                var s = indexs[i];
                var rang = words.GetRange(s, last - s);
                if (rang.Sum(o => o.Duration) > 2.8)
                {
                    ret.Insert(0, rang);
                    last = s;
                }
            }

            var fr = words.GetRange(0, last);
            if (fr.Sum(o => o.Duration) < 1)
            {
                // 前停止词太短了，得和后面的合并
                ret.First().InsertRange(0, fr);
            }
            else
            {
                ret.Insert(0, fr);
            }
            return ret;
        }

        private static List<VttLine> spliteSubLine(List<VttWord> words)
        {
            if (words.Sum(o => o.Duration) < 8)
                return new List<VttLine> { GetLine(words) };

            return spliteBySingleWord(words).Select(o => GetLine(o)).ToList();
        }
    }
}
