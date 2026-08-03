using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentScheduleController : Controller
    {
        private readonly SimsDbContext _context;

        public StudentScheduleController(SimsDbContext context)
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

                return View(new List<Schedule>());
            }

            var courseIds = await _context.CourseStudents
                .Where(cs => cs.StudentId == student.Id)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .Where(s => courseIds.Contains(s.CourseId))
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;

            return View(schedules);
        }

        private async Task<Student?> GetCurrentStudentAsync()
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

            return await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
        }
    }
}