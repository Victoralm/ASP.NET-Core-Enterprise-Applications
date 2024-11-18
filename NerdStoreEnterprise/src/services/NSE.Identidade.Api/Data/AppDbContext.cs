using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NSE.Identidade.API.Data
{
    public class AppDbContext : IdentityDbContext
    {
        protected readonly IConfiguration Configuration;

        public DbSet<IdentityUser> applicationUsers { get; set; }

        public AppDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //connect to postgres with connection string from app settings
            var ops = options.UseNpgsql(Configuration.GetConnectionString("PostgreSql"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure default schema
            modelBuilder.HasDefaultSchema("dev");
            base.OnModelCreating(modelBuilder);
        }
    }
}
