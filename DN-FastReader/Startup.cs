using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;

using System.Security.Claims;
using Microsoft.Extensions.FileProviders;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

using IPA.Cores.Codes;
using IPA.Cores.Helper.Codes;
using static IPA.Cores.Globals.Codes;

namespace DN_FastReader
{
    public class Startup
    {
        readonly HttpServerStartupHelper StartupHelper;
        readonly AspNetLib AspNetLib;

        public Startup(IConfiguration configuration)
        {
            StartupHelper = new HttpServerStartupHelper(configuration);

            AspNetLib = new AspNetLib(configuration);

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AspNetLib.ConfigureServices(StartupHelper, services);

            StartupHelper.ConfigureServices(services);

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            // Enable cookie auth
            EasyCookieAuth.LoginFormMessage.TrySet("ログインが必要です。");
            EasyCookieAuth.AuthenticationPasswordValidator = StartupHelper.SimpleBasicAuthenticationPasswordValidator;
            EasyCookieAuth.ConfigureServices(services);
            
            services.AddMvc()
                .AddViewOptions(opt =>
                {
                    opt.HtmlHelperOptions.ClientValidationEnabled = false;
                })
                .AddRazorOptions(opt =>
                {
                    AspNetLib.ConfigureRazorOptions(opt);
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(new FastReader());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime, FastReader fastReader)
        {
            // wwwroot directory of this project
            StartupHelper.AddStaticFileProvider(Env.AppRootDir._CombinePath("wwwroot"));

            AspNetLib.Configure(StartupHelper, app, env);

            StartupHelper.Configure(app, env);

            // Enable cookie auth
            EasyCookieAuth.Configure(app, env);

            if (StartupHelper.IsDevelopmentMode)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpExceptionLogger();

            //app.UseStaticFiles();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                fastReader._DisposeSafe();
                AspNetLib._DisposeSafe();
                StartupHelper._DisposeSafe();
            });
        }
    }
}
