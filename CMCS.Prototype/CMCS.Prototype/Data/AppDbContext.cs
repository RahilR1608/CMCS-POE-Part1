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
            b.Entity<Claim>().Property(x => x.HoursWorked).HasColumnType("decimal(18,2)");
            b.Entity<Claim>().Property(x => x.HourlyRate).HasColumnType("decimal(18,2)");
            b.Entity<Claim>().Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Entity<ClaimLine>().Property(x => x.Hours).HasColumnType("decimal(18,2)");

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
        }


    }
}
