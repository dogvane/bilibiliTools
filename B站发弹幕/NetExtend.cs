using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B站发弹幕
{
    static class NetExtend
    {
        /// <summary>
        /// GZip解压函数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GZipDecompress(this byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    byte[] bytes = new byte[40960];
                    int n;
                    while ((n = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        stream.Write(bytes, 0, n);
                    }
                    gZipStream.Close();
                }
                return stream.ToArray();
            }
        }
        /// <summary>
        /// GZip压缩函数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GZipCompress(this byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    gZipStream.Write(data, 0, data.Length);
                    gZipStream.Close();
                }
                return stream.ToArray();
            }
        }
        /// <summary>
        /// Deflate解压函数
        /// JS:var details = eval_r('(' + utf8to16(zip_depress(base64decode(hidEnCode.value))) + ')')对应的C#压缩方法
        /// </summary>
        /// <param name="strSource"></param>
        /// <returns></returns>
        public static string DeflateDecompress(this string strSource)
        {
            byte[] buffer = Convert.FromBase64String(strSource);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ms.Write(buffer, 0, buffer.Length);
                ms.Position = 0;
                using (System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress))
                {
                    stream.Flush();
                    int nSize = 16 * 1024 + 256;    //假设字符串不会超过16K
                    byte[] decompressBuffer = new byte[nSize];
                    int nSizeIncept = stream.Read(decompressBuffer, 0, nSize);
                    stream.Close();
                    return System.Text.Encoding.UTF8.GetString(decompressBuffer, 0, nSizeIncept);   //转换为普通的字符串
                }
            }
        }
        /// <summary>
        /// Deflate压缩函数
        /// </summary>
        /// <param name="strSource"></param>
        /// <returns></returns>
        public static string DeflateCompress(this string strSource)
        {
            if (strSource == null || strSource.Length > 8 * 1024)
                throw new System.ArgumentException("字符串为空或长度太大！");
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strSource);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                }
                byte[] compressedData = ms.ToArray();
                ms.Close();
                return Convert.ToBase64String(compressedData);      //将压缩后的byte[]转换为Base64String
            }
        }
    }
}
