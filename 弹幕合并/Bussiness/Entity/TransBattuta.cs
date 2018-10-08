using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dogvane.Srt;

namespace 弹幕合并.Bussiness.Entity
{
    public class TransBattuta: Battuta
    {
        public TransBattuta()
        {

        }

        public TransBattuta(int id, string from, string to, string text) : base(id, from, to, text)        
        {
        }

        /// <summary>
        /// 翻译内容
        /// </summary>
        public string Trans { get; set; }
    }
}
