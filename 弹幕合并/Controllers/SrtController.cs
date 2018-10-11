using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using 弹幕合并.Bussiness;
using 弹幕合并.Bussiness.Entity;
using 弹幕合并.Common;

namespace 弹幕合并.Controllers
{    
    [ApiController]
    public class SrtController : BaseController
    {
        public static SrtBussiness bu = new SrtBussiness();

        [HttpPost]
        [Route("api/srt/updatesrtfile")]
        public ServerReturn UpdateSrtFile(IFormCollection postData)
        {
            if (postData.Files.Count == 0)
                return new ServerReturn {error = -1, error_msg = "没有上传文件"};

            var file = postData.Files[0];
            var ismergeStr = postData["ismerge"].LastOrDefault();
            bool ismerge = false;
            if (!string.IsNullOrEmpty(ismergeStr))
                bool.TryParse(ismergeStr, out ismerge);

            using (var stream = file.OpenReadStream())
            {
                var bytes = new byte[file.Length];
                stream.Read(bytes, 0, (int)file.Length);
                bu.UploadSrtFile(UserId, file.FileName, bytes, ismerge);
            }

            return new ServerReturn { };
        }

        /// <summary>
        /// 获得用户当前的字幕列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [Route("api/srt/getsrtlist")]
        public ServerReturn GetSrtList()
        {
            var srtItems = bu.GetSrtFiles(UserId).Select(o =>
            new {
                o.Id,
                o.SrtFileName,
                uploadTime = o.UploadTime.ToString("yyyy-MM-dd"),
                lastUpdate = o.LastUpdate.ToString("yyyy-MM-dd")
            });
            return new ServerReturn {data = srtItems};
        }

        /// <summary>
        /// 获得用户当前的字幕列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/getsrt")]
        public ServerReturn GetSrt(int srtId)
        {
            var ret = bu.GetSrt(UserId, srtId);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn {error = -1, error_msg = ret.error};
            }
            return new ServerReturn { data = ret.jsonObj };
        }
        
        /// <summary>
        /// 翻译字幕
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/srtTrans")]
        public ServerReturn SrtTrans(int srtId, int id)
        {
            var ret = bu.SrtTrans(UserId, srtId, id);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn {error = -1, error_msg = ret.error};
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 将首单词提升到上一句的末尾
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/SrtUp")]
        public ServerReturn SrtUp(int srtId, int id)
        {
            var ret = bu.SrtUp(UserId, srtId, id);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 将结尾的单词放到下一句的首字母里
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/SrtDown")]
        public ServerReturn SrtDown(int srtId, int id)
        {
            var ret = bu.SrtDown(UserId, srtId, id);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 将结尾的单词放到下一句的首字母里
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/deleteSrt")]
        public ServerReturn DeleteSrt(int srtId)
        {
            var ret = bu.DeleteSrt(UserId, srtId);
            return new ServerReturn {error_msg = ret, error = string.IsNullOrEmpty(ret) ? 0 : -1};
        }


    }
}