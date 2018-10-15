using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dogvane.Srt;

namespace 弹幕合并.Bussiness.Entity
{
    public class TransBattuta: Battuta
    {
        public TransBattuta()
        {

        }

        public TransBattuta(int id, string from, string to, string text, string trans = null) : base(id, from, to, text)
        {
            Trans = trans;
        }

        /// <summary>
        /// 翻译内容
        /// </summary>
        public string Trans { get; set; }

        /// <summary>
        /// 生成导出翻译的数据
        /// </summary>
        /// <returns></returns>
        public string GetTransData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Id.ToString());
            sb.AppendFormat("{0} --> {1}", From, To).AppendLine();
            sb.AppendLine(Trans);

            return sb.ToString();
        }

        /// <summary>
        /// 生成导出翻译的数据
        /// </summary>
        /// <returns></returns>
        public string GetTwoLangData()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Id.ToString());
            sb.AppendFormat("{0} --> {1}", From, To).AppendLine();
            sb.AppendLine(Text);
            sb.AppendLine(Trans);

            return sb.ToString();
        }
    }
}
