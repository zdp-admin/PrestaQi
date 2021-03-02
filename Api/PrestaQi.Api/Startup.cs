using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PrestaQi.Api.Configuration;
using PrestaQi.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WebSocketManager;

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
            ServicePool.RegistryService(_Environment.ContentRootPath, Configuration, services);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            services.AddCors(o => o.AddPolicy("PrestaQiPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddHttpContextAccessor();
            services.AddWebSocketManager();

            services.AddMvc(config =>
            {
                config.Filters.Add(new ExceptionHandling());
            });

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("es-MX");
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("es-MX") };
            });

            services.Configure<FormOptions>(opt =>
            {
                opt.MultipartBodyLengthLimit = Convert.ToInt64(Configuration["Configuration:FileSize"]);
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;

            if (Configuration["environment"] == "dev")
            {
                app.UseDeveloperExceptionPage();
            }

            MonitoringService.Initialize(serviceProvider, Configuration);

            app.UseRequestLocalization();
            app.UseCors("PrestaQiPolicy");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();
            app.MapWebSocketManager("/ws", serviceProvider.GetService<NotificationsMessageHandler>());

            app.UseStaticFiles();
            app.UseWebSockets();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            
        }

    }
}
