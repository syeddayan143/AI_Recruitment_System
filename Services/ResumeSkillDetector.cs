
using System.Text.RegularExpressions;

namespace AIRecruitmentSystem.Services
{
    public class ResumeSkillDetector
    {
        private static readonly string[] KnownSkills =
        {
            "C#",
            "ASP.NET Core",
            "ASP.NET MVC",
            "SQL Server",
            "Entity Framework Core",
            "HTML",
            "CSS",
            "JavaScript",
            "TypeScript",
            "Angular",
            "React",
            "Bootstrap",
            "Python",
            "Machine Learning",
            "Artificial Intelligence",
            "Git",
            "GitHub",
            "REST API",
            "Web API",
            "Azure",
            "AWS",
            "Docker",
            "LINQ",
            "Microsoft .NET"
        };

        public List<string> DetectSkills(string? resumeText)
        {
            var detectedSkills = new List<string>();

            if (string.IsNullOrWhiteSpace(resumeText))
                return detectedSkills;

            foreach (var skill in KnownSkills)
            {
                var pattern = skill switch
                {
                    "C#" => @"(?<![A-Za-z0-9])C#(?![A-Za-z0-9])",
                    "ASP.NET Core" =>
                        @"(?<![A-Za-z0-9])ASP\.NET\s+Core(?![A-Za-z0-9])",
                    "ASP.NET MVC" =>
                        @"(?<![A-Za-z0-9])ASP\.NET\s+MVC(?![A-Za-z0-9])",
                    "SQL Server" =>
                        @"(?<![A-Za-z0-9])SQL\s+Server(?![A-Za-z0-9])",
                    "Entity Framework Core" =>
                        @"(?<![A-Za-z0-9])Entity\s+Framework\s+Core(?![A-Za-z0-9])",
                    "Machine Learning" =>
                        @"(?<![A-Za-z0-9])Machine\s+Learning(?![A-Za-z0-9])",
                    "Artificial Intelligence" =>
                        @"(?<![A-Za-z0-9])Artificial\s+Intelligence(?![A-Za-z0-9])",
                    "REST API" =>
                        @"(?<![A-Za-z0-9])REST\s+API(?![A-Za-z0-9])",
                    "Web API" =>
                        @"(?<![A-Za-z0-9])Web\s+API(?![A-Za-z0-9])",
                    "Microsoft .NET" =>
                        @"(?<![A-Za-z0-9])Microsoft\s+\.NET(?![A-Za-z0-9])",
                    _ => $@"(?<![A-Za-z0-9]){Regex.Escape(skill)}(?![A-Za-z0-9])"
                };

                if (Regex.IsMatch(
                    resumeText,
                    pattern,
                    RegexOptions.IgnoreCase))
                {
                    detectedSkills.Add(skill);
                }
            }

            return detectedSkills;
        }
    }
}
