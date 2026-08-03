using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentAssignmentController : Controller
    {
        private readonly SimsDbContext _context;
        private readonly IConfiguration _configuration;

        public StudentAssignmentController(
            SimsDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                ViewBag.Error =
                    "Your account has not been linked to a student profile.";

                return View(new List<Assignment>());
            }

            var courseIds = await _context.CourseStudents
                .Where(cs => cs.StudentId == student.Id)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var submittedAssignmentIds =
                await _context.Submissions
                    .Where(s => s.StudentId == student.Id)
                    .Select(s => s.AssignmentId)
                    .ToListAsync();

            ViewBag.SubmittedAssignmentIds =
                submittedAssignmentIds;

            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return Forbid();
            }

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            bool enrolled = await _context.CourseStudents
                .AnyAsync(cs =>
                    cs.StudentId == student.Id &&
                    cs.CourseId == assignment.CourseId);

            if (!enrolled)
            {
                return Forbid();
            }

            bool alreadySubmitted = await _context.Submissions
                .AnyAsync(s =>
                    s.AssignmentId == id &&
                    s.StudentId == student.Id);

            if (alreadySubmitted)
            {
                TempData["Error"] =
                    "You have already submitted this assignment.";

                return RedirectToAction(nameof(Index));
            }

            return View(new SubmitAssignmentViewModel
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                CourseName =
                    assignment.Course?.CourseName ?? "N/A",
                DueDate = assignment.DueDate
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            SubmitAssignmentViewModel model)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return Forbid();
            }

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a =>
                    a.Id == model.AssignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            model.AssignmentTitle = assignment.Title;
            model.CourseName =
                assignment.Course?.CourseName ?? "N/A";
            model.DueDate = assignment.DueDate;

            bool enrolled = await _context.CourseStudents
                .AnyAsync(cs =>
                    cs.StudentId == student.Id &&
                    cs.CourseId == assignment.CourseId);

            if (!enrolled)
            {
                return Forbid();
            }

            bool alreadySubmitted = await _context.Submissions
                .AnyAsync(s =>
                    s.AssignmentId == assignment.Id &&
                    s.StudentId == student.Id);

            if (alreadySubmitted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You have already submitted this assignment.");
            }

            if (DateTime.Now > assignment.DueDate)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The assignment deadline has passed.");
            }

            ValidatePdf(model.PdfFile);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string storagePath =
                _configuration["FileStorage:SubmissionPath"]
                ?? Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data",
                    "Submissions");

            Directory.CreateDirectory(storagePath);

            string originalFileName =
                Path.GetFileName(model.PdfFile!.FileName);

            string storedFileName =
                $"{Guid.NewGuid():N}.pdf";

            string physicalPath =
                Path.Combine(storagePath, storedFileName);

            await using (var stream =
                new FileStream(
                    physicalPath,
                    FileMode.CreateNew))
            {
                await model.PdfFile.CopyToAsync(stream);
            }

            var submission = new Submission
            {
                AssignmentId = assignment.Id,
                StudentId = student.Id,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                FilePath = physicalPath,
                FileSize = model.PdfFile.Length,
                SubmittedAt = DateTime.Now
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Assignment submitted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private void ValidatePdf(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(SubmitAssignmentViewModel.PdfFile),
                    "Please select a PDF file.");

                return;
            }

            long maximumSizeMb =
                _configuration.GetValue<long>(
                    "FileStorage:MaximumPdfSizeMb",
                    10);

            long maximumBytes =
                maximumSizeMb * 1024 * 1024;

            if (file.Length > maximumBytes)
            {
                ModelState.AddModelError(
                    nameof(SubmitAssignmentViewModel.PdfFile),
                    $"PDF file must not exceed {maximumSizeMb} MB.");
            }

            string extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (extension != ".pdf")
            {
                ModelState.AddModelError(
                    nameof(SubmitAssignmentViewModel.PdfFile),
                    "Only PDF files are allowed.");
            }

            if (!string.Equals(
                file.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(SubmitAssignmentViewModel.PdfFile),
                    "The selected file is not a valid PDF.");
            }

            if (file.Length >= 5)
            {
                using var reader =
                    new BinaryReader(file.OpenReadStream());

                byte[] header = reader.ReadBytes(5);

                string signature =
                    System.Text.Encoding.ASCII
                        .GetString(header);

                if (signature != "%PDF-")
                {
                    ModelState.AddModelError(
                        nameof(SubmitAssignmentViewModel.PdfFile),
                        "The file content is not a valid PDF.");
                }
            }
        }

        private async Task<Student?>
            GetCurrentStudentAsync()
        {
            string? username =
                User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username);

            if (user == null)
            {
                return null;
            }

            return await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.UserId == user.Id);
        }
    }
}