using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherStudentController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherStudentController(SimsDbContext context)
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
        public async Task<IActionResult> List(int courseId)
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

            ViewBag.CourseName = course.CourseName;

            return View(students);
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