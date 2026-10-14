using System.ComponentModel.DataAnnotations;

namespace AIRecruitmentSystem.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public string CandidateId { get; set; } = string.Empty;

        public ApplicationUser? Candidate { get; set; }

        public int JobId { get; set; }

        public Job? Job { get; set; }

        public int? ResumeId { get; set; }

        public Resume? Resume { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Applied";

        [Range(0, 100)]
        public decimal? MatchScore { get; set; }

        [StringLength(2000)]
        public string? RecruiterNotes { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        // Interviews scheduled for this job application
        public ICollection<Interview> Interviews { get; set; }
            = new List<Interview>();
    }
}