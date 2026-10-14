
using AIRecruitmentSystem.Models;

namespace AIRecruitmentSystem.Services
{
    public class JobMatchingService
    {
        private readonly ResumeSkillDetector _skillDetector;

        public JobMatchingService(ResumeSkillDetector skillDetector)
        {
            _skillDetector = skillDetector;
        }

        public JobMatchResult CalculateMatch(
            string? candidateSkills,
            string? jobRequirements)
        {
            var candidateSkillList = SplitSkills(candidateSkills);

            var requiredSkills = _skillDetector
                .DetectSkills(jobRequirements)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new JobMatchResult();

            if (requiredSkills.Count == 0)
            {
                result.MatchPercentage = 0;
                result.Message =
                    "No recognized skills were found in the job requirements.";

                return result;
            }

            result.MatchedSkills = requiredSkills
                .Where(required => candidateSkillList.Contains(
                    required,
                    StringComparer.OrdinalIgnoreCase))
                .ToList();

            result.MissingSkills = requiredSkills
                .Except(
                    result.MatchedSkills,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            result.MatchPercentage = (int)Math.Round(
                result.MatchedSkills.Count * 100.0 / requiredSkills.Count);

            result.Message = result.MatchPercentage >= 70
                ? "Good skill match"
                : result.MatchPercentage >= 40
                    ? "Partial skill match"
                    : "More skills may be needed";

            return result;
        }

        private static List<string> SplitSkills(string? skills)
        {
            if (string.IsNullOrWhiteSpace(skills))
                return new List<string>();

            return skills
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    public class JobMatchResult
    {
        public int MatchPercentage { get; set; }

        public List<string> MatchedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();

        public string Message { get; set; } = string.Empty;
    }
}
