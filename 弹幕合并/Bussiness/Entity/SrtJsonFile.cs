using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 弹幕合并.Bussiness.Entity
{
    public class SrtJsonFile
    {
        public string FileName { get; set; }

        /// <summary>
        /// 文件版本
        /// 目前线上的版本为 v0
        /// vtt版本为v1
        /// </summary>
        public int Version { get; set; }

        public List<TransBattuta> Battutas { get; set; }
    }
}
