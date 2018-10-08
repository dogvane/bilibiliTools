using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            return View("UpSrt");
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
    }
}
