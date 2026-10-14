using System.ComponentModel.DataAnnotations;

namespace AIRecruitmentSystem.Models
{
    public class Interview
    {
        public int Id { get; set; }

        [Required]
        public int JobApplicationId { get; set; }

        public JobApplication? JobApplication { get; set; }

        public DateTime ScheduledAt { get; set; }

        [Required]
        [StringLength(50)]
        public string InterviewType { get; set; } = "Online";

        [StringLength(500)]
        public string? MeetingLink { get; set; }

        [StringLength(2000)]
        public string? Feedback { get; set; }

        [Range(0, 100)]
        public decimal? InterviewScore { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        // Candidate interview confirmation
        [Required]
        [StringLength(30)]
        public string CandidateResponse { get; set; } = "Pending";

        public DateTime? CandidateRespondedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}