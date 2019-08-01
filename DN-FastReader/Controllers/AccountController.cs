using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DN_FastReader.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

using IPA.Cores.AspNet;
using IPA.Cores.Helper.AspNet;
using static IPA.Cores.Globals.AspNet;

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

            foreach (Account a in list)
            {
                a.DeleteUrl = this.Url.Action(nameof(Delete), new { id = a.Guid });
            }

            return View(list);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            Account a = new Account();
            return View(a);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(Account a)
        {
            if (a.ProviderName._IsEmpty() == false)
            {
                string helpStr = Reader.Inbox.GetProviderAddingAppHelpString(a.ProviderName);

                string redirectUrl = this.GenerateAbsoluteUrl(nameof(AuthCallback));

                Uri uri = new Uri(redirectUrl);

                helpStr = helpStr._ReplaceStr("___DOMAIN_FQDN___", uri.Host);

                helpStr = helpStr._ReplaceStr("___REDIRECT_URL___", redirectUrl);

                string helpStrHtml = Str.LinkUrlOnText(Str.EncodeHtml(helpStr, true), Consts.HtmlTarget.Blank);

                ViewBag.HelpStr = helpStrHtml;
            }

            return View("Create", a);
        }

        [HttpPost]
        [Authorize]
        public IActionResult OnAdd(Account a)
        {
            List<string> error = new List<string>();
            if (a.ProviderName._IsEmpty()) error.Add("サービスが選択されていません。");

            if (a.ProviderName._IsSamei(Consts.InboxProviderNames.Slack_User) == false)
            {
                if (a.AppClientId._IsEmpty()) error.Add("Client ID が指定されていません。");
            }

            if (a.AppClientSecret._IsEmpty()) error.Add("Client Secret が指定されていません。");

            if (error.Count >= 1)
            {
                ViewBag.Error = error._Combine("\n");
                return Create(a);
            }

            string guid = Reader.AddAccount(a);

            return RedirectToAction("AuthStart", new { id = guid });
        }

        [HttpGet]
        [Authorize]
        public IActionResult AuthStart(string id)
        {
            InboxAdapter adapter = Reader.GetAdapter(id);

            string callbackUrl = this.GenerateAbsoluteUrl(nameof(AuthCallback));

            string authUrl = adapter.AuthStartGetUrl(callbackUrl, id);

            if (authUrl._IsEmpty())
            {
                // No need to auth redirect
                adapter.Start(new InboxAdapterUserCredential());

                Reader.SaveSettingsFile();

                return Redirect("/");
            }

            return Redirect(authUrl);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AuthCallback(string code, string state)
        {
            if (code._IsEmpty() || state._IsEmpty())
            {
                throw new ApplicationException($"この URL は認証サービスからのコールバック専用です。直接アクセスすることはできません。");
            }

            string id = state;
            InboxAdapter adapter = Reader.GetAdapter(id);
            InboxAdapterUserCredential credential = await adapter.AuthGetCredentialAsync(code, this.GenerateAbsoluteUrl(nameof(AuthCallback)));

            adapter.Start(credential);

            Reader.SaveSettingsFile();

            return Redirect("/");
        }

        [Authorize]
        public IActionResult Delete(string id)
        {
            Reader.DeleteAdapter(id);

            Reader.SaveSettingsFile();

            return RedirectToAction("Index");
        }
    }
}

