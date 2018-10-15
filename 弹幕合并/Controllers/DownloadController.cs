using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;

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
    }
}