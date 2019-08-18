using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DN_FastReader.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

using IPA.Cores.Web;
using IPA.Cores.Helper.Web;
using static IPA.Cores.Globals.Web;

using IPA.Cores.Codes;
using IPA.Cores.Helper.Codes;
using static IPA.Cores.Globals.Codes;

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
        public IActionResult GetData()
        {
            InboxMessageBox box = this.Reader.GetCurrentBox();
            return box._AspNetJsonResult();
        }

        [Authorize]
        public IActionResult GetVersion()
        {
            int version = this.Reader.GetCurrentVersion();
            return version._AspNetJsonResult();
        }

        [Authorize]
        public IActionResult GetErrorCount()
        {
            return this.Reader.GetErrorCount()._AspNetJsonResult();
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

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            IExceptionHandlerPathFeature errPath =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            Exception error = errPath?.Error;

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = error?.Message ?? "Unknown Error" });
        }
    }
}
