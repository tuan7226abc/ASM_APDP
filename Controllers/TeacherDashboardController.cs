using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherDashboardController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherDashboardController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                ViewBag.Error =
                    "Your account has not been linked to a teacher profile.";

                return View(new TeacherDashboardViewModel());
            }

            var courseIds = await _context.Schedules
                .Where(s => s.TeacherId == teacher.Id)
                .Select(s => s.CourseId)
                .Distinct()
                .ToListAsync();

            int totalStudents = await _context.CourseStudents
                .Where(cs => courseIds.Contains(cs.CourseId))
                .Select(cs => cs.StudentId)
                .Distinct()
                .CountAsync();

            int currentDay = ConvertDayOfWeek(DateTime.Today.DayOfWeek);

            var todaySchedules = await _context.Schedules
                .Include(s => s.Course)
                .Where(s =>
                    s.TeacherId == teacher.Id &&
                    s.DayOfWeek == currentDay &&
                    (!s.StartDate.HasValue ||
                     s.StartDate.Value.Date <= DateTime.Today) &&
                    (!s.EndDate.HasValue ||
                     s.EndDate.Value.Date >= DateTime.Today))
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var viewModel = new TeacherDashboardViewModel
            {
                TeacherName = teacher.FullName,
                TotalCourses = courseIds.Count,
                TotalStudents = totalStudents,
                TodayClasses = todaySchedules.Count,
                TodaySchedules = todaySchedules
            };

            return View(viewModel);
        }

        private async Task<Teacher?> GetCurrentTeacherAsync()
        {
            string? username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            return await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);
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