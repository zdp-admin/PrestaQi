using JabilCore.EFRepository;
using Microsoft.EntityFrameworkCore;
using PrestaQi.Model;
using System;

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
    }
}
