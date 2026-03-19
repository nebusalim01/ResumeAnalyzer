using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Core.Models;

namespace ResumeAnalyzer.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Each DbSet = one table in SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Analysis> Analyses { get; set; }
        public DbSet<JobMatch> JobMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User table configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // Resume table configuration
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.FileName).IsRequired().HasMaxLength(255);
                entity.Property(r => r.FilePath).IsRequired();
                entity.Property(r => r.ParsedText).IsRequired(false);

                // One User has many Resumes
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Resumes)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Analysis table configuration
            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.SkillsJson).IsRequired(false);
                entity.Property(a => a.SuggestionsJson).IsRequired(false);
                entity.Property(a => a.ExperienceSummary).IsRequired(false);
                entity.Property(a => a.Strengths).IsRequired(false);

                // One Resume has many Analyses
                entity.HasOne(a => a.Resume)
                      .WithMany(r => r.Analyses)
                      .HasForeignKey(a => a.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // JobMatch table configuration
            modelBuilder.Entity<JobMatch>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.JobDescription).IsRequired(false);
                entity.Property(j => j.MissingKeywords).IsRequired(false);

                // One Analysis has one JobMatch
                entity.HasOne(j => j.Analysis)
                      .WithOne(a => a.JobMatch)
                      .HasForeignKey<JobMatch>(j => j.AnalysisId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}