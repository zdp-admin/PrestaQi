using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.EFRepository;
using JabilCore.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PrestaQi.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PrestaQi.Api.Configuration
{
    public class ServicePool
    {
        public static void RegistryService(string path, IConfiguration configuration, IServiceCollection services)
        {
            RegistryDefault(services);

            var configurationJabil = configuration.GetSection("JabilCore").Get<JabilCoreConfiguration>();

            foreach (var service in configurationJabil.Services)
            {
                if (!string.IsNullOrEmpty(service.ServiceAssemblyPath))
                    RegisterServices(Path.Combine(path, "../" + service.ServiceAssemblyPath), services);
                if (!string.IsNullOrEmpty(service.RepositoryAssemblyPath))
                    RegisterRepositories(Path.Combine(path, "../" + service.RepositoryAssemblyPath), services);
                if (!string.IsNullOrEmpty(service.Context)) 
                    RegistryContexts(path,  service, services, configuration.GetConnectionString(service.ConnectionStringName));
            }
        }

        static void RegistryContexts(string path, JabilCoreConfigurationSection configuration, IServiceCollection services, string connectionString)
        {
            /*Assembly assembly = Assembly.LoadFile(Path.Combine(path, "../" + configuration.RepositoryAssemblyPath));
            var dbContextType = assembly.GetType(configuration.Context);



            var optionsType = typeof(DbContextOptions<>).MakeGenericType(dbContextType);
            var options = (DbContextOptions)Activator.CreateInstance(optionsType);
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType);
            var optionsBuilder = (DbContextOptionsBuilder)Activator.CreateInstance(optionsBuilderType);
            var dbContext = (DbContext)Activator.CreateInstance(dbContextType, optionsBuilder.Options);

            optionsBuilder.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            services.AddScoped<DbContext>(p => (DbContext)p.GetService(dbContext.GetType()));
            */
            services.AddDbContext<GeneralContext>(option => option.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            services.AddScoped<DbContext>(p => p.GetService<GeneralContext>());
        }

        static void RegisterServices(string path, IServiceCollection services)
        {
            Assembly assembly = Assembly.LoadFile(path);

            Type typeWrite = typeof(IWriteService<>);
            RegistryType(GetTypeByInterface(typeWrite, assembly), typeWrite, services);

            Type typeRetrieve = typeof(IRetrieveService<>);
            RegistryType(GetTypeByInterface(typeRetrieve, assembly), typeRetrieve, services);

            Type typeProcess = typeof(IProcessService<>);
            RegistryType(GetTypeByInterface(typeProcess, assembly), typeProcess, services);
        }

        static void RegisterRepositories(string path, IServiceCollection services)
        {
            Assembly assembly = Assembly.LoadFile(path);

            Type typeWrite = typeof(IRetrieveRepository<>);
            RegistryType(GetTypeByInterface(typeWrite, assembly), typeWrite, services);

            Type typeRetrieve = typeof(IWriteRepository<>);
            RegistryType(GetTypeByInterface(typeRetrieve, assembly), typeRetrieve, services);
        }

        static Type[] GetTypeByInterface(Type typeInterface, Assembly assembly)
        {
            return assembly.GetTypes().Where(p => p.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeInterface) && p.IsClass && !p.IsAbstract)
                        .ToArray();
        }

        static void RegistryType(Type[] typeServices, Type interfaceType, IServiceCollection services)
        {
            foreach (Type type in typeServices)
            {
                Type inter = type.GetInterface(interfaceType.Name);
                services.AddScoped(inter, type);
            }
        }

        static void RegistryDefault(IServiceCollection services)
        {
            services.AddScoped(typeof(IWriteRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IWriteService<>), typeof(WriteService<>));

            services.AddScoped(typeof(IRetrieveRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRetrieveService<>), typeof(RetrieveService<>));
        }
    }
}
