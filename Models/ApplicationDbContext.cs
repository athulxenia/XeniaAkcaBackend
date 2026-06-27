using Microsoft.EntityFrameworkCore;
using XeniaKhraBackend.Models;
using XeniaTokenBackend.Models;


namespace XeniaAkcaBackend.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<MemberPayment> MemberPayments { get; set; }

        public DbSet<MemberContribution> MemberContributions { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberWallet> MemberWallet { get; set; }
        public DbSet<KaruthalMember> KaruthalMembers { get; set; }
        public DbSet<Contribution> Contributions { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<MemberGroup> MemberGroups { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Nominee> Nominees { get; set; }
        public DbSet<Information> Informations { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
    }
}
