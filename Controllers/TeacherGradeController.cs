using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherGradeController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherGradeController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                TempData["Error"] =
                    "Your teacher account has not been linked to a teacher profile.";

                return View(new List<Course>());
            }

            var courses = await _context.Schedules
                .Where(s => s.TeacherId == teacher.Id)
                .Include(s => s.Course)
                .Select(s => s.Course!)
                .Distinct()
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int courseId)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            bool teachesCourse = await _context.Schedules
                .AnyAsync(s =>
                    s.CourseId == courseId &&
                    s.TeacherId == teacher.Id);

            if (!teachesCourse)
            {
                return Forbid();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            var students = await (
                from cs in _context.CourseStudents
                join student in _context.Students
                    on cs.StudentId equals student.Id
                where cs.CourseId == courseId
                orderby student.FullName
                select student
            ).ToListAsync();

            var existingGrades = await _context.Grades
                .Where(g => g.CourseId == courseId)
                .ToDictionaryAsync(
                    g => g.StudentId,
                    g => g);

            var viewModel = new GradeViewModel
            {
                CourseId = course.Id,
                CourseName = course.CourseName,
                TeacherName = teacher.FullName
            };

            foreach (var student in students)
            {
                existingGrades.TryGetValue(
                    student.Id,
                    out Grade? grade);

                viewModel.Students.Add(
                    new GradeStudentViewModel
                    {
                        StudentId = student.Id,
                        StudentCode = student.StudentCode,
                        FullName = student.FullName,
                        AssignmentScore = grade?.AssignmentScore,
                        MidtermScore = grade?.MidtermScore,
                        FinalScore = grade?.FinalScore,
                        TotalScore = grade?.TotalScore,
                        Note = grade?.Note
                    });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(
            GradeViewModel viewModel)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            bool teachesCourse = await _context.Schedules
                .AnyAsync(s =>
                    s.CourseId == viewModel.CourseId &&
                    s.TeacherId == teacher.Id);

            if (!teachesCourse)
            {
                return Forbid();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c =>
                    c.Id == viewModel.CourseId);

            if (course == null)
            {
                return NotFound();
            }

            foreach (var item in viewModel.Students)
            {
                ValidateScore(
                    item.AssignmentScore,
                    $"Assignment score of {item.FullName}");

                ValidateScore(
                    item.MidtermScore,
                    $"Midterm score of {item.FullName}");

                ValidateScore(
                    item.FinalScore,
                    $"Final score of {item.FullName}");
            }

            if (!ModelState.IsValid)
            {
                viewModel.CourseName = course.CourseName;
                viewModel.TeacherName = teacher.FullName;

                return View(viewModel);
            }

            foreach (var item in viewModel.Students)
            {
                bool belongsToCourse =
                    await _context.CourseStudents.AnyAsync(cs =>
                        cs.CourseId == viewModel.CourseId &&
                        cs.StudentId == item.StudentId);

                if (!belongsToCourse)
                {
                    continue;
                }

                var grade = await _context.Grades
                    .FirstOrDefaultAsync(g =>
                        g.CourseId == viewModel.CourseId &&
                        g.StudentId == item.StudentId);

                if (grade == null)
                {
                    grade = new Grade
                    {
                        CourseId = viewModel.CourseId,
                        StudentId = item.StudentId,
                        TeacherId = teacher.Id,
                        CreatedAt = DateTime.Now
                    };

                    _context.Grades.Add(grade);
                }

                grade.AssignmentScore = item.AssignmentScore;
                grade.MidtermScore = item.MidtermScore;
                grade.FinalScore = item.FinalScore;
                grade.Note = item.Note?.Trim();
                grade.TeacherId = teacher.Id;
                grade.UpdatedAt = DateTime.Now;

                grade.CalculateTotalScore();
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Grades saved successfully.";

            return RedirectToAction(
                nameof(Manage),
                new
                {
                    courseId = viewModel.CourseId
                });
        }

        private void ValidateScore(
            decimal? score,
            string fieldName)
        {
            if (score.HasValue &&
                (score.Value < 0 || score.Value > 10))
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"{fieldName} must be between 0 and 10.");
            }
        }

        private async Task<Teacher?> GetCurrentTeacherAsync()
        {
            string? username = User.Identity?.Name;

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

            return await _context.Teachers
                .FirstOrDefaultAsync(t =>
                    t.UserId == user.Id);
        }
    }
}