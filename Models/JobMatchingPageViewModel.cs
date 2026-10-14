using AIRecruitmentSystem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AIRecruitmentSystem.Models
{
    public class JobMatchingPageViewModel
    {
        public int ResumeId { get; set; }

        public int JobId { get; set; }

        public List<SelectListItem> Resumes { get; set; } = new();

        public List<SelectListItem> Jobs { get; set; } = new();

        public JobMatchAiResult? Result { get; set; }

        public string? SelectedResumeName { get; set; }

        public string? SelectedJobTitle { get; set; }
    }
}