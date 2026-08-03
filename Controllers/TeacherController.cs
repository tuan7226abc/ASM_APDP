using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeacherController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherController(SimsDbContext context)
        {
            _context = context;
        }

        // ===========================
        // Teacher List
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers
                .OrderBy(t => t.TeacherCode)
                .ToListAsync();

            return View(teachers);
        }

        // ===========================
        // Teacher Details
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id.Value);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // ===========================
        // Create Teacher
        // ===========================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Teacher());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher)
        {
            teacher.TeacherCode = teacher.TeacherCode?.Trim() ?? string.Empty;
            teacher.FullName = teacher.FullName?.Trim() ?? string.Empty;
            teacher.Email = teacher.Email?.Trim() ?? string.Empty;
            teacher.Phone = teacher.Phone?.Trim();
            teacher.Department = teacher.Department?.Trim();

            bool duplicateCode = await _context.Teachers
                .AnyAsync(t => t.TeacherCode == teacher.TeacherCode);

            if (duplicateCode)
            {
                ModelState.AddModelError(
                    nameof(teacher.TeacherCode),
                    "Teacher code already exists.");
            }

            bool duplicateEmail = await _context.Teachers
                .AnyAsync(t => t.Email == teacher.Email);

            if (duplicateEmail)
            {
                ModelState.AddModelError(
                    nameof(teacher.Email),
                    "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(teacher);
            }

            teacher.CreatedDate = DateTime.Now;

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Teacher created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Edit Teacher
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FindAsync(id.Value);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Teacher model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            model.TeacherCode = model.TeacherCode?.Trim() ?? string.Empty;
            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Phone = model.Phone?.Trim();
            model.Department = model.Department?.Trim();

            bool duplicateCode = await _context.Teachers
                .AnyAsync(t =>
                    t.TeacherCode == model.TeacherCode &&
                    t.Id != id);

            if (duplicateCode)
            {
                ModelState.AddModelError(
                    nameof(model.TeacherCode),
                    "Teacher code already exists.");
            }

            bool duplicateEmail = await _context.Teachers
                .AnyAsync(t =>
                    t.Email == model.Email &&
                    t.Id != id);

            if (duplicateEmail)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            teacher.TeacherCode = model.TeacherCode;
            teacher.FullName = model.FullName;
            teacher.Email = model.Email;
            teacher.Phone = model.Phone;
            teacher.Department = model.Department;

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Teacher updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Teachers
                    .AnyAsync(t => t.Id == id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Delete Teacher - GET
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id.Value);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // ===========================
        // Delete Teacher - POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                TempData["Error"] = "Teacher not found.";
                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                int? linkedUserId = teacher.UserId;

                // Xóa điểm do giáo viên chấm nếu Grade có TeacherId.
                var grades = await _context.Grades
                    .Where(g => g.TeacherId == id)
                    .ToListAsync();

                _context.Grades.RemoveRange(grades);

                // Xóa lịch dạy. Attendance liên kết Schedule
                // phải được xóa trước.
                var scheduleIds = await _context.Schedules
                    .Where(s => s.TeacherId == id)
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

                // Xóa bài nộp trước vì Submission phụ thuộc Assignment.
                var assignmentIds = await _context.Assignments
                    .Where(a => a.TeacherId == id)
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

                // Xóa hồ sơ Teacher
                _context.Teachers.Remove(teacher);

                await _context.SaveChangesAsync();

                // Xóa tài khoản User liên kết
                if (linkedUserId.HasValue)
                {
                    var linkedUser = await _context.Users
                        .FirstOrDefaultAsync(
                            u => u.Id == linkedUserId.Value);

                    if (linkedUser != null)
                    {
                        _context.Users.Remove(linkedUser);
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Teacher and related data deleted successfully.";
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