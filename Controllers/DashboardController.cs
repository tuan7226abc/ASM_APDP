using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly SimsDbContext _context;

        public DashboardController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                StudentCount = await _context.Students.CountAsync(),

                TeacherCount = await _context.Teachers.CountAsync(),

                CourseCount = await _context.Courses.CountAsync(),

                UserCount = await _context.Users.CountAsync(),

                ScheduleCount = await _context.Schedules.CountAsync(),

                CurrentUsername = User.Identity?.Name ?? "Admin",

                ChartLabels = new List<string>
                {
                    "Mon",
                    "Tue",
                    "Wed",
                    "Thu",
                    "Fri",
                    "Sat",
                    "Sun"
                },

                StudentChartData = new List<int>
                {
                    20,
                    42,
                    58,
                    74,
                    88,
                    102,
                    83
                }
            };

            return View(model);
        }
    }
}