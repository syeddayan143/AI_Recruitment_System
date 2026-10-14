using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs => Set<Job>();

        public DbSet<CandidateProfile> CandidateProfiles
            => Set<CandidateProfile>();

        public DbSet<Resume> Resumes => Set<Resume>();

        public DbSet<JobApplication> JobApplications
            => Set<JobApplication>();

        public DbSet<Interview> Interviews => Set<Interview>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity tables
            builder.Entity<ApplicationUser>()
                .ToTable("AIRecruitmentSystemUsers", "dbo");

            builder.Entity<IdentityRole>()
                .ToTable("AIRecruitmentSystemRoles", "dbo");

            builder.Entity<IdentityUserRole<string>>()
                .ToTable("AIRecruitmentSystemUserRoles", "dbo");

            builder.Entity<IdentityUserClaim<string>>()
                .ToTable("AIRecruitmentSystemUserClaims", "dbo");

            builder.Entity<IdentityUserLogin<string>>()
                .ToTable("AIRecruitmentSystemUserLogins", "dbo");

            builder.Entity<IdentityRoleClaim<string>>()
                .ToTable("AIRecruitmentSystemRoleClaims", "dbo");

            builder.Entity<IdentityUserToken<string>>()
                .ToTable("AIRecruitmentSystemUserTokens", "dbo");

            // Recruitment tables
            builder.Entity<Job>()
                .ToTable("AIRecruitmentSystemJobs", "dbo");

            builder.Entity<CandidateProfile>()
                .ToTable("AIRecruitmentSystemCandidateProfiles", "dbo");

            builder.Entity<Resume>()
                .ToTable("AIRecruitmentSystemResumes", "dbo");

            builder.Entity<JobApplication>()
                .ToTable("AIRecruitmentSystemJobApplications", "dbo");

            builder.Entity<Interview>()
                .ToTable("AIRecruitmentSystemInterviews", "dbo");

            // Job -> Recruiter
            builder.Entity<Job>()
                .HasOne(j => j.Recruiter)
                .WithMany()
                .HasForeignKey(j => j.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);

            // One profile per candidate
            builder.Entity<CandidateProfile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            builder.Entity<CandidateProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<CandidateProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Resume -> Candidate
            builder.Entity<Resume>()
                .HasOne(r => r.Candidate)
                .WithMany()
                .HasForeignKey(r => r.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Application -> Candidate
            builder.Entity<JobApplication>()
                .HasOne(a => a.Candidate)
                .WithMany()
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Application -> Job
            builder.Entity<JobApplication>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // Application -> Resume
            builder.Entity<JobApplication>()
                .HasOne(a => a.Resume)
                .WithMany()
                .HasForeignKey(a => a.ResumeId)
                .OnDelete(DeleteBehavior.Restrict);

            // A candidate can apply to a job only once
            builder.Entity<JobApplication>()
                .HasIndex(a => new { a.CandidateId, a.JobId })
                .IsUnique();

            // JobApplication -> Interviews
            // Explicitly use the existing JobApplicationId foreign key.
            builder.Entity<JobApplication>()
                .HasMany(a => a.Interviews)
                .WithOne(i => i.JobApplication)
                .HasForeignKey(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision
            builder.Entity<Job>()
                .Property(j => j.MinimumSalary)
                .HasPrecision(18, 2);

            builder.Entity<Job>()
                .Property(j => j.MaximumSalary)
                .HasPrecision(18, 2);

            builder.Entity<Resume>()
                .Property(r => r.ResumeScore)
                .HasPrecision(5, 2);

            builder.Entity<JobApplication>()
                .Property(a => a.MatchScore)
                .HasPrecision(5, 2);

            builder.Entity<Interview>()
                .Property(i => i.InterviewScore)
                .HasPrecision(5, 2);
        }
    }
}