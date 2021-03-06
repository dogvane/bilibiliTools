using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 弹幕合并.Bussiness.Subtitles;

namespace 弹幕合并.Bussiness.Entity
{
    public class TransBattuta: Line
    {
        public TransBattuta()
        {

        }

        //public TransBattuta(int id, string from, string to, string text, string trans = null) : base(id, from, to, text)
        //{
        //    Trans = trans;
        //}

        /// <summary>
        /// 翻译内容
        /// </summary>
        public string Trans { get; set; }

        /// <summary>
        /// 人工翻译的内容
        /// </summary>
        public string Trans2 { get; set; }

        /// <summary>
        /// 生成导出翻译的数据
        /// </summary>
        /// <returns></returns>
        public string GetSrtTransData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Id.ToString());
            sb.AppendFormat("{0} --> {1}", From.Replace(".", ","), To.Replace(".", ",")).AppendLine();
            if (string.IsNullOrEmpty(Trans2))
                sb.AppendLine(Trans);
            else
                sb.Append(Trans2);

            return sb.ToString();
        }

        /// <summary>
        /// 生成导出翻译的数据
        /// </summary>
        /// <returns></returns>
        public string GetSrtTwoLangData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Id.ToString());
            sb.AppendFormat("{0} --> {1}", From.Replace(".", ","), To.Replace(".", ",")).AppendLine();
            sb.AppendLine(Text);
            sb.AppendLine(Trans);

            return sb.ToString();
        }
    }
}
