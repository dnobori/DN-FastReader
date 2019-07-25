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

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;
using System.Security.Claims;
using Microsoft.Extensions.FileProviders;

namespace DN_FastReader
{
    public class Startup
    {
        readonly HttpServerStartupHelper Helper;
        readonly AspNetHelper AspNetHelper;

        public Startup(IConfiguration configuration)
        {
            Helper = new HttpServerStartupHelper(configuration);

            AspNetHelper = new AspNetHelper(configuration);

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AspNetHelper.ConfigureServices(Helper, services);

            Helper.ConfigureServices(services);

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            // Enable cookie auth
            EasyCookieAuth.LoginFormMessage.TrySet("ログインが必要です。");
            EasyCookieAuth.AuthenticationPasswordValidator = Helper.SimpleBasicAuthenticationPasswordValidator;
            EasyCookieAuth.ConfigureServices(services);

            services.AddMvc()
                .AddViewOptions(opt =>
                {
                    opt.HtmlHelperOptions.ClientValidationEnabled = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(new FastReader());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime, FastReader fastReader)
        {
            // wwwroot directory of this project
            Helper.AddStaticFileProvider(Env.AppRootDir._CombinePath("wwwroot"));

            AspNetHelper.Configure(Helper, app, env);

            Helper.Configure(app, env);

            // Enable cookie auth
            EasyCookieAuth.Configure(app, env);

            if (Helper.IsDevelopmentMode)
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
                AspNetHelper._DisposeSafe();
                Helper._DisposeSafe();
            });
        }
    }
}
