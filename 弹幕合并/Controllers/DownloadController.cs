using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using 弹幕合并.Bussiness.Entity;

namespace 弹幕合并.Controllers
{
    [Route("srt/[action]/{id}")]
    public class DownloadController : BaseController
    {
        [HttpGet]
        [HttpPost]
        public IActionResult Download(int id)
        {
            var srtdata = SrtController.bu.GetSrt(UserId, id);
            if (string.IsNullOrEmpty(srtdata.error))
            {
                StringBuilder writer = new StringBuilder();
                foreach (var item in srtdata.jsonObj.Battutas)
                {
                    writer.AppendLine(item.GetData());
                }

                var bytes = Encoding.UTF8.GetBytes(writer.ToString());
                return File(bytes, "text/srt", srtdata.jsonObj.FileName);
            }
            return File(Encoding.UTF8.GetBytes("no find srt " + id), "txt/srt", Path.GetFileName("error.log"));
        }

        [HttpGet]
        [HttpPost]
        public IActionResult DownloadTrans(int id)
        {
            var srtdata = SrtController.bu.GetSrt(UserId, id);
            if (string.IsNullOrEmpty(srtdata.error))
            {
                StringBuilder writer = new StringBuilder();
                foreach (var item in srtdata.jsonObj.Battutas)
                {
                    writer.AppendLine(item.GetTransData());
                }

                var bytes = Encoding.UTF8.GetBytes(writer.ToString());
                return File(bytes, "text/srt", "(中文)" + srtdata.jsonObj.FileName);
            }
            return File(Encoding.UTF8.GetBytes("no find srt " + id), "txt/srt", Path.GetFileName("error.log"));
        }

        /// <summary>
        /// 下载中英文的字幕
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IActionResult DownloadTwoLang(int id)
        {
            var srtdata = SrtController.bu.GetSrt(UserId, id);
            if (string.IsNullOrEmpty(srtdata.error))
            {
                StringBuilder writer = new StringBuilder();
                foreach (var item in srtdata.jsonObj.Battutas)
                {
                    writer.AppendLine(item.GetTwoLangData());
                }

                var bytes = Encoding.UTF8.GetBytes(writer.ToString());
                return File(bytes, "text/srt", "(中英文)" + srtdata.jsonObj.FileName);
            }
            return File(Encoding.UTF8.GetBytes("no find srt " + id), "txt/srt", Path.GetFileName("error.log"));
        }

        /// <summary>
        /// 下载B站的弹幕格式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IActionResult DownloadBilibili(int id)
        {
            var srtdata = SrtController.bu.GetSrt(UserId, id);
            if (string.IsNullOrEmpty(srtdata.error))
            {
                var bilibili = new BilibiliSrtFile();
                bilibili.font_size = 0.4;
                bilibili.font_color = "#FFFFFF";
                bilibili.background_alpha = 0.5;
                bilibili.background_color = "#9C27B0";
                bilibili.Stroke = "none";
                bilibili.body = new List<BilibiliSrtFile.Battuta>();

                foreach (var item in srtdata.jsonObj.Battutas)
                {
                    bilibili.body.Add(new BilibiliSrtFile.Battuta
                    {
                        from = item.FromSec,
                        to = item.ToSec,
                        content = item.Text,
                        location = 2,
                    });                    
                }

                var json = JsonConvert.SerializeObject(bilibili);
                var bytes = Encoding.UTF8.GetBytes(json);
                return File(bytes, "text/json", "(b站格式)" + srtdata.jsonObj.FileName.Replace(".srt",".bcc"));
            }
            return File(Encoding.UTF8.GetBytes("no find srt " + id), "txt/srt", Path.GetFileName("error.log"));
        }
    }
}