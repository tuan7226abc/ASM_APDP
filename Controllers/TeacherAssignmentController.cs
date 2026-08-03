using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherAssignmentController : Controller
    {
        private readonly SimsDbContext context;

        public TeacherAssignmentController(SimsDbContext db)
        {
            context = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                ViewBag.Error =
                    "Your teacher account has not been linked to a teacher profile.";

                return View(new List<Assignment>());
            }

            var assignments = await context.Assignments
                .Include(a => a.Course)
                .Where(a => a.TeacherId == teacher.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            await LoadCourseListAsync(teacher.Id);

            return View(new Assignment
            {
                DueDate = DateTime.Now.AddDays(7),
                MaxScore = 10
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            bool teachesCourse = await context.Schedules.AnyAsync(s =>
                s.TeacherId == teacher.Id &&
                s.CourseId == assignment.CourseId);

            if (!teachesCourse)
            {
                ModelState.AddModelError(
                    nameof(assignment.CourseId),
                    "You are not assigned to this course.");
            }

            if (assignment.DueDate <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(assignment.DueDate),
                    "Due date must be in the future.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCourseListAsync(
                    teacher.Id,
                    assignment.CourseId);

                return View(assignment);
            }

            assignment.TeacherId = teacher.Id;
            assignment.CreatedAt = DateTime.Now;

            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();

            TempData["Success"] =
                "Assignment created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Submissions(int id)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            var assignment = await context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return NotFound();
            }

            var submissions = await context.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == id)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            ViewBag.AssignmentTitle = assignment.Title;
            ViewBag.CourseName =
                assignment.Course?.CourseName ?? "N/A";

            return View(submissions);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            var submission = await context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    s.Assignment != null &&
                    s.Assignment.TeacherId == teacher.Id);

            if (submission == null)
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(submission.FilePath))
            {
                return NotFound("The submitted PDF file no longer exists.");
            }

            var bytes = await System.IO.File
                .ReadAllBytesAsync(submission.FilePath);

            return File(
                bytes,
                "application/pdf",
                submission.OriginalFileName);
        }

        [HttpGet]
        public async Task<IActionResult> Grade(int id)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            var submission = await context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    s.Assignment != null &&
                    s.Assignment.TeacherId == teacher.Id);

            if (submission == null)
            {
                return NotFound();
            }

            return View(new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                StudentCode =
                    submission.Student?.StudentCode ?? "N/A",
                StudentName =
                    submission.Student?.FullName ?? "N/A",
                AssignmentTitle =
                    submission.Assignment?.Title ?? "N/A",
                FileName = submission.OriginalFileName,
                SubmittedAt = submission.SubmittedAt,
                Score = submission.Score,
                Feedback = submission.Feedback
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(
            GradeSubmissionViewModel model)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            var submission = await context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s =>
                    s.Id == model.SubmissionId &&
                    s.Assignment != null &&
                    s.Assignment.TeacherId == teacher.Id);

            if (submission == null)
            {
                return NotFound();
            }

            decimal maximumScore =
                submission.Assignment?.MaxScore ?? 10;

            if (model.Score.HasValue &&
                (model.Score < 0 ||
                 model.Score > maximumScore))
            {
                ModelState.AddModelError(
                    nameof(model.Score),
                    $"Score must be between 0 and {maximumScore}.");
            }

            if (!ModelState.IsValid)
            {
                model.StudentCode =
                    submission.Student?.StudentCode ?? "N/A";

                model.StudentName =
                    submission.Student?.FullName ?? "N/A";

                model.AssignmentTitle =
                    submission.Assignment?.Title ?? "N/A";

                model.FileName =
                    submission.OriginalFileName;

                model.SubmittedAt =
                    submission.SubmittedAt;

                return View(model);
            }

            submission.Score = model.Score;
            submission.Feedback = model.Feedback?.Trim();
            submission.GradedAt = DateTime.Now;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Submission graded successfully.";

            return RedirectToAction(
                nameof(Submissions),
                new
                {
                    id = submission.AssignmentId
                });
        }

        private async Task LoadCourseListAsync(
            int teacherId,
            int? selectedCourseId = null)
        {
            var courses = await context.Schedules
                .Where(s => s.TeacherId == teacherId)
                .Include(s => s.Course)
                .Select(s => s.Course!)
                .Distinct()
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            ViewBag.CourseId = new SelectList(
                courses,
                "Id",
                "CourseName",
                selectedCourseId);
        }

        private async Task<Teacher?> GetCurrentTeacherAsync()
        {
            string? username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username);

            if (user == null)
            {
                return null;
            }

            return await context.Teachers
                .FirstOrDefaultAsync(t =>
                    t.UserId == user.Id);
        }
    }
}