using Microsoft.EntityFrameworkCore;

namespace XeniaTokenBackend.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<xtm_AppSettings> xtm_AppSettings { get; set; }
        public DbSet<xtm_Advertisement> xtm_Advertisement { get; set; }
        public DbSet<xtm_Counter> xtm_Counter { get; set; }
        public DbSet<xtm_Company> xtm_Company { get; set; }
        public DbSet<xtm_CompanySettings> xtm_CompanySettings { get; set; }
        public DbSet<xtm_Customer> xtm_Customer { get; set; }
        public DbSet<xtm_Department> xtm_Department { get; set; }
        public DbSet<xtm_TokenMaster> xtm_TokenMaster { get; set; }
        public DbSet<xtm_Service> xtm_Service { get; set; }
        public DbSet<xtm_TokenRegister> xtm_TokenRegister { get; set; }
        public DbSet<xtm_TokenHistory> xtm_TokenHistory { get; set; }
        public DbSet<xtm_Users> xtm_Users { get; set; }
        public DbSet<xtm_UserMap> xtm_UserMap { get; set; }
    }
}
