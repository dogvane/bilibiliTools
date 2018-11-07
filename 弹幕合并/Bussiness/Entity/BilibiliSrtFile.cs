using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 弹幕合并.Bussiness.Entity
{
    /// <summary>
    /// b站自己的弹幕格式数据
    /// </summary>
    public class BilibiliSrtFile
    {
        public double font_size { get; set; }

        public string font_color { get; set; }

        public double background_alpha { get; set; }

        public string background_color { get; set; }

        public string Stroke { get; set; }

        public List<Battuta> body { get; set; }

        public class Battuta
        {
            public double from { get; set; }

            public double to { get; set; }

            public int location { get; set; }

            public string content { get; set; }
        }
    }

    
}
