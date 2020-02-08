using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 弹幕合并.Common
{
    public static class TimeExtend
    {
        /// <summary>
        /// time 00:01:41,500 --> 101.5(sec)
        /// </summary>
        /// <param name="time">formater hh:mm:ss.fff </param>
        /// <returns>if error return 0</returns>
        public static double ConvertToTimeSec(this string time)
        {
            TimeSpan ret;
            
            if (TimeSpan.TryParseExact(time, @"hh\:mm\:ss\.fff", null, out ret))
            {
                return ret.TotalSeconds;
            }

            if (TimeSpan.TryParseExact(time, @"hh\:mm\:ss\,fff", null, out ret))
            {
                return ret.TotalSeconds;
            }

            return 0;
        }
        
        public static double GetDuration(string from, string to)
        {
            return to.ConvertToTimeSec() - from.ConvertToTimeSec();
        }
    }
}
