using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ProductRecommendation.Service;
using ProductRecommendationRepository.Context;
using ProductRecommendationService.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationHangfire
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var supportedCultures = new List<CultureInfo> { new CultureInfo("tr-TR") };

            services.AddCors();

            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    opts.DefaultRequestCulture = new RequestCulture("tr-TR");
                    opts.SupportedCultures = supportedCultures;
                    opts.SupportedUICultures = supportedCultures;
                }
            );

            services.AddRazorPages();
            services.AddDbContext<ProductContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:DbConnectionString"]));
            services.AddHangfire(x => x.UseSqlServerStorage(Configuration["ConnectionStrings:DefaultConnection"]));
            services.AddHangfireServer();

            services.AddScoped<IProductService, ProductService>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }            

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            var supportedCultures = new[]
            {
              new CultureInfo("tr-TR")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("tr-TR"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                  {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                  }
            });
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * 4
            });
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
            FakeContext.SetServiceProvider(app.ApplicationServices);
            RecurringJobs.SchedulerJob();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
