
using System.ComponentModel.DataAnnotations;

namespace AIRecruitmentSystem.Models
{
    public class CandidateProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [StringLength(100)]
        public string? CurrentJobTitle { get; set; }

        [StringLength(100)]
        public string? HighestEducation { get; set; }

        [Range(0, 60)]
        public int ExperienceYears { get; set; }

        [StringLength(100)]
        public string? PreferredLocation { get; set; }

        [StringLength(2000)]
        public string? Skills { get; set; }

        [StringLength(1000)]
        public string? LinkedInUrl { get; set; }

        [StringLength(1000)]
        public string? PortfolioUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
