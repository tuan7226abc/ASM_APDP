using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherScheduleController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherScheduleController(SimsDbContext context)
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
                    "Your teacher account has not been linked to a teacher profile.";

                return View(new List<Schedule>());
            }

            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Where(s => s.TeacherId == teacher.Id)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.TeacherName = teacher.FullName;

            return View(schedules);
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
    }
}