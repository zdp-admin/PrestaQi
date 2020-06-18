using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrestaQi.Api.Configuration;
using PrestaQi.DataAccess;
using System;
using System.Security.AccessControl;

namespace PrestaQi.Api
{
    public class Startup
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _Environment;

        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            Configuration = configuration;
            _Environment = env;
        }

       
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Type type = typeof(GeneralContext);

            /*services.AddDbContext<GeneralContext>(options => {
                options.UseNpgsql(Configuration.GetConnectionString("Postgres")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

            services.AddScoped<DbContext>(p => p.GetService<GeneralContext>());
      */

            ServicePool.RegistryService(_Environment.ContentRootPath, Configuration, services);
           

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
