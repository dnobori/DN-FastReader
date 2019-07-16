using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DN_FastReader.Models;
using Microsoft.AspNetCore.Authorization;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

namespace DN_FastReader.Controllers
{
    public class HomeController : Controller
    {
        readonly FastReader Reader;

        public HomeController(FastReader reader)
        {
            this.Reader = reader;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public JsonResult GetData()
        {
            InboxMessageBox box = this.Reader.GetCurrentBox();
            return Json(box);
        }

        [Authorize]
        public JsonResult GetVersion()
        {
            int version = this.Reader.GetCurrentVersion();
            return Json(version);
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
    }
}
