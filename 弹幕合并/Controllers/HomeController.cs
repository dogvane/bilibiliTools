using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dogvane.Srt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NETCore.Encrypt;
using 弹幕合并.Bussiness.Entity;
using 弹幕合并.Models;

namespace 弹幕合并.Controllers
{
    // [Route("{controller=Home}/{action=Index}/{id?}")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // 密码加密
            EncryptProvider.Sha256("123");
            return View("Index");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult YoutubeToBilibili()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public IActionResult UpdateSrtFile(IFormCollection files)
        {
            if (files.Files.Count == 0)
                return Json("no file.");

            var file = files.Files[0];
            using (var stream = file.OpenReadStream())
            {
                var bytes = new byte[file.Length];
                stream.Read(bytes, 0, (int)file.Length);
                var saveFile = Path.Combine("upfiles/", file.FileName);
                var dir = new FileInfo(saveFile).Directory;
                if (!dir.Exists)
                    dir.Create();

                System.IO.File.WriteAllBytes(saveFile, bytes);
            }
                return Json("Hey");
            // return View("UpSrt");
        }

        [HttpPost]
        public IActionResult YoutubeSrtFileToBilibliFile(IFormCollection files)
        {
            if (files.Files.Count == 0)
                return Json("no file.");

            var file = files.Files[0];
            using (var stream = file.OpenReadStream())
            {
                var bytes = new byte[file.Length];
                stream.Read(bytes, 0, (int) file.Length);

                var lines = Encoding.UTF8.GetString(bytes).Split(@"\n");
                var battutes = new SrtManagerT<TransBattuta>().LoadBattute(lines);

                var bilibili = new BilibiliSrtFile();
                bilibili.font_size = 0.4;
                bilibili.font_color = "#FFFFFF";
                bilibili.background_alpha = 0.5;
                bilibili.background_color = "#9C27B0";
                bilibili.Stroke = "none";
                bilibili.body = new List<BilibiliSrtFile.Battuta>();

                foreach (var item in battutes)
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
                bytes = Encoding.UTF8.GetBytes(json);

                return File(bytes, "text/json", "(b站格式)" + file.FileName.Replace(".srt", ".bcc"));
            }
        }
    }
}
