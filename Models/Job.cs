
using Microsoft.AspNetCore.Builder;
using System.ComponentModel.DataAnnotations;

namespace AIRecruitmentSystem.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Requirements { get; set; }

        [Range(0, 100000000)]
        public decimal? MinimumSalary { get; set; }

        [Range(0, 100000000)]
        public decimal? MaximumSalary { get; set; }

        [Required]
        [StringLength(50)]
        public string EmploymentType { get; set; } = "Full-time";

        public bool IsActive { get; set; } = true;

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosingDate { get; set; }

        [Required]
        public string RecruiterId { get; set; } = string.Empty;

        public ApplicationUser? Recruiter { get; set; }

        public ICollection<JobApplication> Applications { get; set; }
            = new List<JobApplication>();
    }
}
