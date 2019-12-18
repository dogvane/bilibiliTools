using System;
using System.Collections.Generic;
using System.Text;

namespace VTT
{
    /// <summary>
    /// 谷歌的Vtt字幕
    /// </summary>
    public class Vtt
    {
        /// <summary>
        /// 类型吧
        /// </summary>
        public string Kind { get; set; }

        public string Language { get; set; }

        public List<VttLine> Lines { get; set; } = new List<VttLine>();
    }

    /// <summary>
    /// 一行字幕
    /// </summary>
    public class VttLine
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// start time, format HH:mm:ss,fff
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// star time, 00:01:01 -> 61 Sec
        /// </summary>
        public double FromSec
        {
            get
            {
                return From.ConvertToTimeSec();
            }
        }

        /// <summary>
        /// end time,  format HH:mm:ss,fff
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// end time, 00:01:01 -> 61 Sec
        /// </summary>
        public double ToSec
        {
            get
            {
                return To.ConvertToTimeSec();
            }
        }

        /// <summary>
        /// show time, ToSec - FormSec 
        /// </summary>
        public double Duration
        {
            get { return ToSec - FromSec; }
        }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 拆分到每一个单词
        /// </summary>
        public List<VttWord> Words { get; set; } = new List<VttWord>();


        public override string ToString()
        {
            return string.Format("{0}   {1} --> {2} {3}", Id, From, To, Text);
        }

        public string GetData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Id.ToString());
            sb.AppendFormat("{0} --> {1}", From, To).AppendLine();
            sb.AppendLine(Text);

            return sb.ToString();
        }
    }

    /// <summary>
    /// 一个单词
    /// </summary>
    public class VttWord
    {

        /// <summary>
        /// start time, format HH:mm:ss,fff
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// star time, 00:01:01 -> 61 Sec
        /// </summary>
        public double FromSec
        {
            get
            {
                return From.ConvertToTimeSec();
            }
        }

        /// <summary>
        /// end time,  format HH:mm:ss,fff
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// end time, 00:01:01 -> 61 Sec
        /// </summary>
        public double ToSec
        {
            get
            {
                return To.ConvertToTimeSec();
            }
        }

        /// <summary>
        /// show time, ToSec - FormSec 
        /// </summary>
        public double Duration
        {
            get { return ToSec - FromSec; }
        }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
    }
}
