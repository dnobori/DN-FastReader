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
using Microsoft.AspNetCore.Mvc.Routing;

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

        [HttpGet]
        public IActionResult Create()
        {
            Account a = new Account();
            return View(a);
        }

        [HttpPost]
        public IActionResult Create(Account a)
        {
            return View(a);
        }

        [HttpPost]
        public IActionResult OnAdd(Account a)
        {
            List<string> error = new List<string>();
            if (a.ProviderName._IsEmpty()) error.Add("サービスが選択されていません。");
            if (a.AppClientId._IsEmpty()) error.Add("Client ID が指定されていません。");
            if (a.AppClientSecret._IsEmpty()) error.Add("Client Secret が指定されていません。");

            if (error.Count >= 1)
            {
                ViewBag.Error = error._Combine("\n");
                return View("Create", a);
            }

            string guid = Reader.AddAccount(a);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AuthStart(string id)
        {
            InboxAdapter adapter = Reader.GetAdapter(id);

            string url = Str.BuildHttpUrl(Request.Scheme, Request.Host.Host, Request.Host.Port ?? 0, this.Url.Action("test1"));

            throw new ApplicationException(url);

            //adapter.AuthStartGetUrl(redi


        }
    }
}

