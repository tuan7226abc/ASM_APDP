using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly SimsDbContext _context;

        public StudentDashboardController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                ViewBag.Error =
                    "Your account has not been linked to a student profile.";

                return View(new StudentDashboardViewModel());
            }

            var courseIds = await _context.CourseStudents
                .Where(cs => cs.StudentId == student.Id)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            int currentDay = ConvertDayOfWeek(
                DateTime.Today.DayOfWeek);

            var todaySchedules = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .Where(s =>
                    courseIds.Contains(s.CourseId) &&
                    s.DayOfWeek == currentDay &&
                    (!s.StartDate.HasValue ||
                     s.StartDate.Value.Date <= DateTime.Today) &&
                    (!s.EndDate.HasValue ||
                     s.EndDate.Value.Date >= DateTime.Today))
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            int totalGrades = await _context.Grades
                .CountAsync(g => g.StudentId == student.Id);

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = student.FullName,
                TotalCourses = courseIds.Count,
                TodayClasses = todaySchedules.Count,
                TotalGrades = totalGrades,
                TodaySchedules = todaySchedules
            };

            return View(viewModel);
        }

        private async Task<Student?> GetCurrentStudentAsync()
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

            return await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.UserId == user.Id);
        }

        private static int ConvertDayOfWeek(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                DayOfWeek.Saturday => 7,
                DayOfWeek.Sunday => 8,
                _ => 2
            };
        }
    }
}