
using System.ComponentModel.DataAnnotations;

namespace AIRecruitmentSystem.ViewModels
{
    public class JobApplicationFormViewModel
    {
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string CompanyOrRecruiter { get; set; } = string.Empty;

        public string JobLocation { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public decimal? MinimumSalary { get; set; }

        public decimal? MaximumSalary { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Current Job Title")]
        public string? CurrentJobTitle { get; set; }

        [StringLength(100)]
        [Display(Name = "Highest Education")]
        public string? HighestEducation { get; set; }

        [Range(0, 60)]
        [Display(Name = "Years of Experience")]
        public int ExperienceYears { get; set; }

        [StringLength(100)]
        [Display(Name = "Preferred Location")]
        public string? PreferredLocation { get; set; }

        [StringLength(2000)]
        public string? Skills { get; set; }

        [StringLength(1000)]
        [Url]
        [Display(Name = "LinkedIn Profile")]
        public string? LinkedInUrl { get; set; }

        [StringLength(1000)]
        [Url]
        [Display(Name = "Portfolio Website")]
        public string? PortfolioUrl { get; set; }

        [Required(ErrorMessage = "Please select a resume.")]
        [Display(Name = "Resume")]
        public int? ResumeId { get; set; }

        public List<ResumeOptionViewModel> Resumes { get; set; } = new();
    }

    public class ResumeOptionViewModel
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}
