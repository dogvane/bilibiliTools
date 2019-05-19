using System.Text;

namespace Dogvane.Srt
{
    /// <summary>
    /// 弹幕
    /// </summary>
    public class Battuta
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

        public Battuta()
        {

        }

        public Battuta(int id, string from, string to, string text)
        {
            this.Id = id;
            this.From = from;
            this.To = to;
            this.Text = text;
        }

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
}