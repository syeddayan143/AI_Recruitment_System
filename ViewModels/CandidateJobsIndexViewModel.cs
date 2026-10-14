
namespace AIRecruitmentSystem.ViewModels
{
    public class CandidateJobsIndexViewModel
    {
        public bool HasResume { get; set; }

        public string CandidateSkills { get; set; } = string.Empty;

        public List<CandidateJobMatchViewModel> Jobs { get; set; } = new();
    }

    public class CandidateJobMatchViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public decimal? MinimumSalary { get; set; }

        public decimal? MaximumSalary { get; set; }

        public DateTime PostedAt { get; set; }

        public int MatchPercentage { get; set; }

        public List<string> MatchedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();

        public string MatchMessage { get; set; } = string.Empty;
    }
}
