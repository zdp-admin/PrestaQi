using System.Collections.Generic;

namespace PrestaQi.Api.Configuration
{
    public sealed class JabilCoreConfiguration
    {
        public List<JabilCoreConfigurationSection> Services { get; set; }
    }

    public sealed class JabilCoreConfigurationSection
    {
        public string Context { get; set; }
        public string ConnectionStringName { get; set; }
        public string ServiceAssemblyPath { get; set; }
        public string RepositoryAssemblyPath { get; set; }
    }
}
