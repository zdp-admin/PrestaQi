using System.Collections.Generic;

namespace PrestaQi.Api.Configuration
{
    public sealed class InsiscoCoreConfiguration
    {
        public List<InsiscoCoreConfigurationSection> Services { get; set; }
    }

    public sealed class InsiscoCoreConfigurationSection
    {
        public string Context { get; set; }
        public string ConnectionStringName { get; set; }
        public string ServiceAssemblyPath { get; set; }
        public string RepositoryAssemblyPath { get; set; }
    }
}
