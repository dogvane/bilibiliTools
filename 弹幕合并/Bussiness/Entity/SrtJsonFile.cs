using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 弹幕合并.Bussiness.Entity
{
    public class SrtJsonFile
    {
        public string FileName { get; set; }

        public List<TransBattuta> Battutas { get; set; }
    }
}
