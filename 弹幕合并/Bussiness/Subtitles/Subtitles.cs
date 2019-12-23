using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 弹幕合并.Common;

namespace 弹幕合并.Bussiness.Subtitles
{
    /// <summary>
    /// 公共的字幕文件的接口
    /// </summary>
    public class Subtitles
    {
        /// <summary>
        /// 类型吧
        /// </summary>
        public string Kind { get; set; }

        public string Language { get; set; }

        public List<Line> Lines { get; set; } = new List<Line>();
    }

    /// <summary>
    /// 弹幕里的一行
    /// </summary>
    public class Line
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
        public List<Word> Words { get; set; } = new List<Word>();

        public override string ToString()
        {
            return string.Format("{0}   {1} --> {2} {3}", Id, From, To, Text);
        }

        public string GetSrtData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Id.ToString());
            sb.AppendFormat("{0} --> {1}", From.Replace(".", ","), To.Replace(".", ",")).AppendLine();
            sb.AppendLine(Text);

            return sb.ToString();
        }
    }

    /// <summary>
    /// 弹幕了的一个单词
    /// </summary>
    public class Word
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

        public override bool Equals(object obj)
        {
            if(obj is Word)
            {
                return ((Word)obj).Text == this.Text;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Text))
                return base.GetHashCode();

            return Text.GetHashCode();
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
