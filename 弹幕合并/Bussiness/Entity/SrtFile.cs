using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace 弹幕合并.Bussiness.Entity
{
    /// <summary>
    /// 字幕文件的管理
    /// </summary>
    public class SrtFile
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime UploadTime { get; set; }

        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// 原始字幕在本地文件存放的目录
        /// </summary>
        public string SourceLocalFileName { get; set; }

        /// <summary>
        /// 字幕本身的文件名
        /// </summary>
        public string SrtFileName { get; set; }

        /// <summary>
        /// srt文件处理后的json文件路径
        /// </summary>
        public string JsonSrtFileName { get; set; }
    }
}
