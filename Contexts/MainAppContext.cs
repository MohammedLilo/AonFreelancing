using AonFreelancing.Enums;
using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AonFreelancing.Contexts
{
    public class MainAppContext:IdentityDbContext<User, ApplicationRole, long>
    {

        public DbSet<Project> Projects { get; set; }
        public DbSet<User> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public MainAppContext(DbContextOptions<MainAppContext> contextOptions) : base(contextOptions) {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<User>().ToTable("AspNetUsers").HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<User>().Property(u => u.PhoneNumber).IsRequired();


            builder.Entity<Freelancer>().ToTable("Freelancers");
            
            builder.Entity<Client>().ToTable("Clients");
            
            builder.Entity<SystemUser>().ToTable("SystemUsers");
           
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","length([Code]) = 6"));
            
            builder.Entity<TempUser>().ToTable("TempUsers").HasIndex(u=>u.PhoneNumber).IsUnique();

            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_PRICE_TYPE", "[PriceType] IN ('Fixed', 'PerHour')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_QUALIFICATION_NAME", "[QualificationName] IN ('Backend Developer', 'Frontend Developer', 'Mobile Developer', 'UI/UX')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_STATUS", "[Status] IN ('Available', 'Closed')"))
                                                                .Property(p=>p.Status).HasDefaultValue("Available");



//set up relationships
builder.Entity<TempUser>().HasOne<OTP>()
                                    .WithOne()
                                    .HasForeignKey<OTP>()
                                    .HasPrincipalKey<TempUser>(nameof(TempUser.PhoneNumber));


            base.OnModelCreating(builder);
        }

    }
}
