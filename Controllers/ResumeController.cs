
using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using AIRecruitmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ResumeTextExtractor _textExtractor;
        private readonly ResumeSkillDetector _skillDetector;
        private readonly GroqResumeAnalyzer _groqAnalyzer;

        private const long MaxFileSize = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".docx"
        };

        public ResumeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ResumeTextExtractor textExtractor,
            ResumeSkillDetector skillDetector,
            GroqResumeAnalyzer groqAnalyzer)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _textExtractor = textExtractor;
            _skillDetector = skillDetector;
            _groqAnalyzer = groqAnalyzer;
        }

        // Display the logged-in candidate's resumes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var resumes = await _context.Resumes
                .AsNoTracking()
                .Where(r => r.CandidateId == userId)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            return View(resumes);
        }

        // Upload resume, extract text and detect skills
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? resumeFile)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            if (resumeFile == null || resumeFile.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a resume file.";

                return RedirectToAction(nameof(Index));
            }

            if (resumeFile.Length > MaxFileSize)
            {
                TempData["ErrorMessage"] =
                    "Resume size must not exceed 5 MB.";

                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(resumeFile.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] =
                    "Only PDF and DOCX files are allowed.";

                return RedirectToAction(nameof(Index));
            }

            var expectedContentType = extension == ".pdf"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            if (!string.Equals(
                resumeFile.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "The uploaded file type could not be verified.";

                return RedirectToAction(nameof(Index));
            }

            var storageFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                "Resumes");

            Directory.CreateDirectory(storageFolder);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";

            var fullPath = Path.Combine(
                storageFolder,
                storedFileName);

            try
            {
                await using (var stream = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await resumeFile.CopyToAsync(stream);
                }

                var extractedText =
                    await _textExtractor.ExtractTextAsync(
                        fullPath,
                        extension);

                var detectedSkills =
                    _skillDetector.DetectSkills(extractedText);

                var skillsText = string.Join(
                    ", ",
                    detectedSkills);

                var resume = new Resume
                {
                    CandidateId = userId,
                    OriginalFileName =
                        Path.GetFileName(resumeFile.FileName),
                    StoredFileName = storedFileName,
                    ContentType = expectedContentType,
                    FileSizeBytes = resumeFile.Length,
                    ExtractedText = extractedText,
                    DetectedSkills = skillsText,
                    UploadedAt = DateTime.UtcNow,
                    AnalyzedAt = null
                };

                _context.Resumes.Add(resume);

                await _context.SaveChangesAsync();

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    TempData["SuccessMessage"] =
                        "Resume uploaded, but no readable text was found. " +
                        "Scanned PDFs may require OCR.";
                }
                else if (detectedSkills.Count == 0)
                {
                    TempData["SuccessMessage"] =
                        "Resume uploaded and text extracted. " +
                        "No skills from the current detection list were found.";
                }
                else
                {
                    TempData["SuccessMessage"] =
                        "Resume uploaded successfully. Detected skills: " +
                        skillsText;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                TempData["ErrorMessage"] =
                    "Resume processing failed. Please check the file and try again.";

                return RedirectToAction(nameof(Index));
            }
        }

        // Analyze only the logged-in candidate's resume with Groq AI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var resume = await _context.Resumes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == id && r.CandidateId == userId,
                    cancellationToken);

            if (resume == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(resume.ExtractedText))
            {
                TempData["ErrorMessage"] =
                    "This resume has no readable text to analyze. " +
                    "Try uploading a text-based PDF or DOCX file.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                var analysis = await _groqAnalyzer.AnalyzeAsync(
                    resume.ExtractedText,
                    cancellationToken);

                var viewModel = new ResumeAnalysisPageViewModel
                {
                    Resume = resume,
                    Analysis = analysis
                };

                return View("Analysis", viewModel);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "AI analysis could not be completed. Please check your " +
                    "Groq API key, model availability, internet connection " +
                    "and account quota, then try again.";

                return RedirectToAction(nameof(Index));
            }
        }

        // Download only the logged-in candidate's resume
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var resume = await _context.Resumes
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Id == id && r.CandidateId == userId);

            if (resume == null)
                return NotFound();

            var storageFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                "Resumes");

            var fullPath = Path.Combine(
                storageFolder,
                resume.StoredFileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Resume file not found.");

            return PhysicalFile(
                fullPath,
                resume.ContentType,
                resume.OriginalFileName);
        }

        // Delete only the logged-in candidate's resume
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r =>
                    r.Id == id && r.CandidateId == userId);

            if (resume == null)
                return NotFound();

            var storageFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                "Resumes");

            var fullPath = Path.Combine(
                storageFolder,
                resume.StoredFileName);

            _context.Resumes.Remove(resume);

            await _context.SaveChangesAsync();

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            TempData["SuccessMessage"] =
                "Resume deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
