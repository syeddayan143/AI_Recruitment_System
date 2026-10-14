
using System.ComponentModel.DataAnnotations;

namespace AIRecruitmentSystem.Models
{
    public class Resume
    {
        public int Id { get; set; }

        [Required]
        public string CandidateId { get; set; } = string.Empty;

        public ApplicationUser? Candidate { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public string? ExtractedText { get; set; }

        public string? DetectedSkills { get; set; }

        public decimal? ResumeScore { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AnalyzedAt { get; set; }
    }
}
