
using AIRecruitmentSystem.Services;

namespace AIRecruitmentSystem.Models
{
    public class ResumeAnalysisPageViewModel
    {
        public Resume Resume { get; set; } = new();

        public ResumeAnalysisResult Analysis { get; set; } = new();
    }
}
