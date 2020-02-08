using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 弹幕合并.Common
{
    public static class Utils
    {
        /// <summary>
        /// 获取2个数据的交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static T[] Intersect2<T, T2>(this List<T> source, T2[] t2, Func<T, T2, bool> comparer)
        {
            if (source.Count == 0 || t2.Length == 0)
                return new T[0];

            for (var i = 0; i < source.Count - t2.Length; i++)
            {
                var item1 = source[i];
                var item2 = t2[0];
                if(comparer(item1, item2))
                {
                    bool isOk = true;
                    for(var j = 1; j< t2.Length; j++)
                    {
                        if(!comparer(source[i + j], t2[j]))
                        {
                            isOk = false;
                            break;
                        }
                    }

                    if (isOk)
                    {
                        return source.Skip(i).Take(t2.Length).ToArray();
                    }
                }
            }

            return new T[0];
        }

        public static int IntersectIndex<T, T2>(this List<T> source, T2[] t2, Func<T, T2, bool> comparer)
        {
            if (source.Count == 0 || t2.Length == 0)
                return -1;

            for (var i = 0; i < source.Count - t2.Length; i++)
            {
                var item1 = source[i];
                var item2 = t2[0];
                if (comparer(item1, item2))
                {
                    bool isOk = true;
                    for (var j = 1; j < t2.Length; j++)
                    {
                        if (!comparer(source[i + j], t2[j]))
                        {
                            isOk = false;
                            break;
                        }
                    }

                    if (isOk)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
