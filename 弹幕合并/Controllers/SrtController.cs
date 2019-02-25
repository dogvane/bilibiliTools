using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        /// 翻译字幕
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/srtTrans2")]
        [Authorize]
        public ServerReturn SrtTrans(int srtId, string ids)
        {
            var id = ids.Split(',').Select(int.Parse).ToArray();
            var ret = bu.SrtTrans2(UserId, srtId, id);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 用户翻译自己的字幕
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/updateTrans")]
        [Authorize]
        public ServerReturn SrtTrans(postSource post)
        {
            var ret = bu.UpdateTrans(UserId, post.srtId, post.id, post.text);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        public class postSource
        {
            public int srtId { get; set; }

            public int id { get; set; }

            public string text { get; set; }

            public string replace { get; set; }
        }

        /// <summary>
        /// 更新字幕的原始值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/updateSource")]
        [Authorize]
        public ServerReturn SrtUpdateSource(postSource post)
        {
            var ret = bu.UpdateSource(UserId, post.srtId, post.id, post.text);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 原始字幕搜索并替换操作
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/replaceSource")]
        [Authorize]
        public ServerReturn SrtReplaceSource(postSource post)
        {
            var ret = bu.ReplaceSource(UserId, post.srtId, post.text, post.replace);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 将首单词提升到上一句的末尾
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/SrtUp")]
        [Authorize]
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
        /// 将首单词提升到上一句的末尾
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/SrtLineUp")]
        [Authorize]
        public ServerReturn SrtLineUp(int srtId, int id)
        {
            var ret = bu.SrtLineUp(UserId, srtId, id);
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
        [Authorize]
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
        [Route("api/srt/SrtLineDown")]
        [Authorize]
        public ServerReturn SrtLineDown(int srtId, int id)
        {
            var ret = bu.SrtLineDown(UserId, srtId, id);
            if (!string.IsNullOrEmpty(ret.error))
            {
                return new ServerReturn { error = -1, error_msg = ret.error };
            }
            return new ServerReturn { data = ret.battuta };
        }

        /// <summary>
        /// 删除一个字幕
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/srt/deleteSrt")]
        [Authorize]
        public ServerReturn DeleteSrt(int srtId)
        {
            var ret = bu.DeleteSrt(UserId, srtId);
            return new ServerReturn {error_msg = ret, error = string.IsNullOrEmpty(ret) ? 0 : -1};
        }
    }
}