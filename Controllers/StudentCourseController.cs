using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentCourseController : Controller
    {
        private readonly SimsDbContext _context;

        public StudentCourseController(SimsDbContext context)
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

                return View(new List<Course>());
            }

            var courses = await (
                from cs in _context.CourseStudents
                join course in _context.Courses
                    on cs.CourseId equals course.Id
                where cs.StudentId == student.Id
                orderby course.CourseName
                select course
            )
            .Distinct()
            .ToListAsync();

            ViewBag.StudentName = student.FullName;

            return View(courses);
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