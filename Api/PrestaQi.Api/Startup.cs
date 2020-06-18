using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.EFRepository;
using JabilCore.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrestaQi.DataAccess;
using PrestaQi.Model;
using PrestaQi.Service.WriteServices;

namespace PrestaQi.Api
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
            services.AddDbContext<GeneralContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Postgres")));
            services.AddScoped<DbContext>(p => p.GetService<GeneralContext>());

            services.AddScoped(typeof(IWriteRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IWriteService<>), typeof(WriteService<>));

            services.AddScoped(typeof(IRetrieveRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRetrieveService<>), typeof(RetrieveService<>));

            Type type = typeof(IWriteService<>);

            Assembly assembly = Assembly.LoadFile(@"C:\Users\2271776\Documents\Visual Studio 2019\PrestaQi\PrestaQi.Service\bin\Debug\netcoreapp3.1\PrestaQi.Service.dll");
            var arrayType = assembly.GetTypes().Where(p => p.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type) && p.IsClass && !p.IsAbstract)
                .ToArray();

            foreach (Type type1 in arrayType)
            {
                Type inter = type1.GetInterface(type.Name);
                services.AddScoped(inter, type1);
            }

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
