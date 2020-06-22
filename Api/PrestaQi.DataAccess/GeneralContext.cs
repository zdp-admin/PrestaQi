using Microsoft.EntityFrameworkCore;
using PrestaQi.Model;

namespace PrestaQi.DataAccess
{
    public class GeneralContext : DbContext
    {
        public GeneralContext(DbContextOptions<GeneralContext> options) : base(options)
        {
        }

        public DbSet<Period> Periods { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<UserProperty> UserProperties { get; set; }
        public DbSet<UserCapital> UserCapitals { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<UserModule> UserModules { get; set; }
    }

}
