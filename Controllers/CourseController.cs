using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly SimsDbContext _context;

        public CourseController(
            ICourseService courseService,
            SimsDbContext context)
        {
            _courseService = courseService;
            _context = context;
        }

        // ===========================
        // Course List
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllAsync();
            return View(courses);
        }

        // ===========================
        // Create Course
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Course());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            try
            {
                await _courseService.AddAsync(course);

                TempData["Success"] =
                    "Course created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.InnerException?.Message ?? ex.Message);

                return View(course);
            }
        }

        // ===========================
        // Edit Course
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetByIdAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(course);
            }

            try
            {
                await _courseService.UpdateAsync(course);

                TempData["Success"] =
                    "Course updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.InnerException?.Message ?? ex.Message);

                return View(course);
            }
        }

        // ===========================
        // Delete Course - GET
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // ===========================
        // Delete Course - POST
        // ===========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Xóa đăng ký môn học
                var courseStudents = await _context.CourseStudents
                    .Where(cs => cs.CourseId == id)
                    .ToListAsync();

                _context.CourseStudents.RemoveRange(courseStudents);

                // Xóa điểm của môn học
                var grades = await _context.Grades
                    .Where(g => g.CourseId == id)
                    .ToListAsync();

                _context.Grades.RemoveRange(grades);

                // Xóa lịch học và điểm danh liên quan
                var scheduleIds = await _context.Schedules
                    .Where(s => s.CourseId == id)
                    .Select(s => s.Id)
                    .ToListAsync();

                if (scheduleIds.Count > 0)
                {
                    var attendances = await _context.Attendances
                        .Where(a => scheduleIds.Contains(a.ScheduleId))
                        .ToListAsync();

                    _context.Attendances.RemoveRange(attendances);

                    var schedules = await _context.Schedules
                        .Where(s => scheduleIds.Contains(s.Id))
                        .ToListAsync();

                    _context.Schedules.RemoveRange(schedules);
                }

                // Xóa bài nộp trước khi xóa Assignment
                var assignmentIds = await _context.Assignments
                    .Where(a => a.CourseId == id)
                    .Select(a => a.Id)
                    .ToListAsync();

                if (assignmentIds.Count > 0)
                {
                    var submissions = await _context.Submissions
                        .Where(s =>
                            assignmentIds.Contains(s.AssignmentId))
                        .ToListAsync();

                    _context.Submissions.RemoveRange(submissions);

                    var assignments = await _context.Assignments
                        .Where(a => assignmentIds.Contains(a.Id))
                        .ToListAsync();

                    _context.Assignments.RemoveRange(assignments);
                }

                // Xóa khóa học
                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Course and related data deleted successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Delete failed: " +
                    (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}