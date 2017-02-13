using BethanysPieShop.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BethanysPieShop
{
    public class Startup
    {
        private IConfigurationRoot _configurationRoot;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _configurationRoot = new ConfigurationBuilder()
                                        .SetBasePath(hostingEnvironment.ContentRootPath)
                                        .AddJsonFile("appsettings.json")
                                        .Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(
                    // Connection strings can be found in appsettings.json.
                    options => options.UseSqlServer(_configurationRoot.GetConnectionString("DefaultConnection"))
                );

            // What AddTransient means is that whenever we invoke an ICategoryRepository we'll get
            // back a new MockCategoryRepository.
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IPieRepository, PieRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // The AddScoped method will create an object associated with the request.
            // It is, however, different for each request. Person A will get an instance
            // of a shopping cart but Person B will get another instance of a different shopping cart. 
            services.AddScoped<ShoppingCart>(sp => ShoppingCart.GetCart(sp));
            services.AddMvc();


            services.AddMemoryCache();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            // The placement of the UseSession middleware here before UseMvc is important, otherwise
            // it wouldn't work. 
            app.UseSession();

            //app.UseMvcWithDefaultRoute();
            app.UseMvc(routes =>
            {
                // The placement of routes is important. The most specific must go towards the top
                // while the most general fall to the very bottom.
                routes.MapRoute(
                    name: "categoryFilter",
                    template: "Pie/{action}/{category?}",
                    defaults: new { Controller = "Pie", action = "List" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            DbInitializer.Seed(app);
        }
    }
}
