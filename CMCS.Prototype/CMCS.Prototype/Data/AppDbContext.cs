using CMCS.Prototype.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Prototype.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<ClaimLine> ClaimLines => Set<ClaimLine>();
        public DbSet<SupportingDocument> SupportingDocuments => Set<SupportingDocument>();
        public DbSet<Approval> Approvals => Set<Approval>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ----- Primary keys (explicit = safer) -----
            b.Entity<User>().HasKey(x => x.UserId);
            b.Entity<Claim>().HasKey(x => x.ClaimId);
            b.Entity<ClaimLine>().HasKey(x => x.ClaimLineId);
            b.Entity<SupportingDocument>().HasKey(x => x.DocumentId);
            b.Entity<Approval>().HasKey(x => x.ApprovalId);

            // ----- Decimal precision (remove the warnings) -----
            b.Entity<Claim>().Property(x => x.HoursWorked).HasPrecision(18, 2);   // >>>
            b.Entity<Claim>().Property(x => x.HourlyRate).HasPrecision(18, 2);    // >>>
            b.Entity<Claim>().Property(x => x.TotalAmount).HasPrecision(18, 2);   // >>>
            b.Entity<ClaimLine>().Property(x => x.Hours).HasPrecision(18, 2);     // >>>

            // ----- Relationships + delete behavior (avoid cascade cycles) -----
            // Claim -> Lecturer (User): Restrict
            b.Entity<Claim>()
                .HasOne(x => x.Lecturer)
                .WithMany(x => x.Claims)
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClaimLine -> Claim: Cascade is fine
            b.Entity<ClaimLine>()
                .HasOne(x => x.Claim)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupportingDocument -> Claim: Cascade is fine
            b.Entity<SupportingDocument>()
                .HasOne(x => x.Claim)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // Approval -> Claim: Restrict (important)
            b.Entity<Approval>()
                .HasOne(x => x.Claim)
                .WithMany(x => x.Approvals)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            // Approval -> Approver (User): Restrict (important)
            b.Entity<Approval>()
                .HasOne(x => x.Approver)
                .WithMany(x => x.Approvals)
                .HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            // >>> Seed 3 demo users (lecturer, coordinator, manager)
            b.Entity<User>().HasData(
                new User
                {
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "A. Naidoo",
                    Email = "a@cmcs.local",
                    Role = UserRole.Lecturer
                },
                new User
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "C. Mthembu",
                    Email = "c@cmcs.local",
                    Role = UserRole.Coordinator
                },
                new User
                {
                    UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "M. Dlamini",
                    Email = "m@cmcs.local",
                    Role = UserRole.Manager
                }
            );
        }
    }
}
