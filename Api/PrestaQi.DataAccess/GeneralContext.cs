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
        public DbSet<Accredited> Accrediteds { get; set; }
        public DbSet<Investor> Investors { get; set; }
        public DbSet<Capital> Capitals { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<UserModule> UserModules { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<CapitalDetail> CapitalDetails { get; set; }
        public DbSet<Advance> Advances { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Institution> Institution { get; set; }
        public DbSet<SpeiResponse> SpeiResponses { get; set; }
        public DbSet<PaidAdvance> PaidAdvances { get; set; }
        public DbSet<Repayment> Repayments { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           // builder.Entity<Capital>().HasOne(x => x.Investor).WithMany(x => x.Capitals).HasForeignKey(x => x.investor_id);
            base.OnModelCreating(builder);
        }
    }

}
