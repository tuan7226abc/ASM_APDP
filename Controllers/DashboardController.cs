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

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalUsers = await _context.Users.CountAsync()
            };

            return View(viewModel);
        }
    }
}