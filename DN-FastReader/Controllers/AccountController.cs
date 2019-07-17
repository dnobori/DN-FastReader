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
    public class AccountController : Controller
    {
        readonly FastReader Reader;
        
        public AccountController(FastReader reader)
        {
            this.Reader = reader;
        }

        [Authorize]
        public IActionResult Index()
        {
            Account[] list = Reader.GetAccountList();

            return View(list);
        }

        [Route("account/create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("account/create")]
        [HttpPost]
        public IActionResult CreatePost()
        {
            return View();
        }
    }
}