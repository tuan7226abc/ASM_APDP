using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly SimsDbContext _context;

        public StudentController(
            IStudentService studentService,
            SimsDbContext context)
        {
            _studentService = studentService;
            _context = context;
        }

        // =====================================================
        // STUDENT LIST
        // GET: /Student
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.StudentCode)
                .ThenBy(s => s.FullName)
                .ToListAsync();

            return View(students);
        }

        // =====================================================
        // CREATE STUDENT - GET
        // GET: /Student/Create
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var student = new Student
            {
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return View(student);
        }

        // =====================================================
        // CREATE STUDENT - POST
        // POST: /Student/Create
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            NormalizeStudent(student);

            // Những trường này không được lấy từ form khi tạo mới
            student.Id = 0;
            student.UserId = null;
            student.CreatedAt = DateTime.Now;
            student.UpdatedAt = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return View(student);
            }

            // Kiểm tra mã sinh viên bị trùng
            if (!string.IsNullOrWhiteSpace(student.StudentCode))
            {
                bool studentCodeExists = await _context.Students
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.StudentCode == student.StudentCode);

                if (studentCodeExists)
                {
                    ModelState.AddModelError(
                        nameof(student.StudentCode),
                        "Student code already exists.");

                    return View(student);
                }
            }

            // Kiểm tra email bị trùng
            if (!string.IsNullOrWhiteSpace(student.Email))
            {
                bool emailExists = await _context.Students
                    .AsNoTracking()
                    .AnyAsync(s => s.Email == student.Email);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        nameof(student.Email),
                        "Email already exists.");

                    return View(student);
                }
            }

            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Student added successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot save student: " +
                    GetDatabaseError(ex));

                return View(student);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot save student: " + ex.Message);

                return View(student);
            }
        }

        // =====================================================
        // EDIT STUDENT - GET
        // GET: /Student/Edit/5
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // =====================================================
        // EDIT STUDENT - POST
        // POST: /Student/Edit/5
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Student formStudent)
        {
            if (id <= 0 || id != formStudent.Id)
            {
                return BadRequest();
            }

            NormalizeStudent(formStudent);

            if (!ModelState.IsValid)
            {
                return View(formStudent);
            }

            // Kiểm tra mã sinh viên bị trùng với bản ghi khác
            if (!string.IsNullOrWhiteSpace(formStudent.StudentCode))
            {
                bool duplicatedCode = await _context.Students
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.StudentCode == formStudent.StudentCode &&
                        s.Id != id);

                if (duplicatedCode)
                {
                    ModelState.AddModelError(
                        nameof(formStudent.StudentCode),
                        "Student code already exists.");

                    return View(formStudent);
                }
            }

            // Kiểm tra email bị trùng với bản ghi khác
            if (!string.IsNullOrWhiteSpace(formStudent.Email))
            {
                bool duplicatedEmail = await _context.Students
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.Email == formStudent.Email &&
                        s.Id != id);

                if (duplicatedEmail)
                {
                    ModelState.AddModelError(
                        nameof(formStudent.Email),
                        "Email already exists.");

                    return View(formStudent);
                }
            }

            // Lấy entity thật đang được lưu trong database
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            /*
             * Chỉ cập nhật các trường có trong form.
             * Không cập nhật Id, UserId và CreatedAt.
             */
            existingStudent.StudentCode =
                formStudent.StudentCode;

            existingStudent.FullName =
                formStudent.FullName;

            existingStudent.Email =
                formStudent.Email;

            existingStudent.DateOfBirth =
                formStudent.DateOfBirth;

            existingStudent.Program =
                formStudent.Program;

            existingStudent.Phone =
                formStudent.Phone;

            existingStudent.Address =
                formStudent.Address;

            existingStudent.Gender =
                formStudent.Gender;

            existingStudent.UpdatedAt =
                DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Student updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                bool stillExists = await _context.Students
                    .AsNoTracking()
                    .AnyAsync(s => s.Id == id);

                if (!stillExists)
                {
                    return NotFound();
                }

                ModelState.AddModelError(
                    string.Empty,
                    "The student was changed by another user. " +
                    "Please reload the page and try again.");

                return View(formStudent);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot update student: " +
                    GetDatabaseError(ex));

                return View(formStudent);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot update student: " + ex.Message);

                return View(formStudent);
            }
        }

        // =====================================================
        // DELETE STUDENT - GET
        // GET: /Student/Delete/5
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // =====================================================
        // DELETE STUDENT - POST
        // POST: /Student/Delete/5
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Xóa bài nộp liên quan
                var submissions = await _context.Submissions
                    .Where(s => s.StudentId == id)
                    .ToListAsync();

                _context.Submissions.RemoveRange(submissions);

                // Xóa điểm danh liên quan
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == id)
                    .ToListAsync();

                _context.Attendances.RemoveRange(attendances);

                // Xóa điểm liên quan
                var grades = await _context.Grades
                    .Where(g => g.StudentId == id)
                    .ToListAsync();

                _context.Grades.RemoveRange(grades);

                // Xóa đăng ký khóa học liên quan
                var courseStudents = await _context.CourseStudents
                    .Where(cs => cs.StudentId == id)
                    .ToListAsync();

                _context.CourseStudents.RemoveRange(courseStudents);

                /*
                 * Chỉ xóa hồ sơ Student.
                 * Không tự động xóa User để tránh xóa nhầm tài khoản.
                 */
                _context.Students.Remove(student);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Student deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Cannot delete student: " +
                    GetDatabaseError(ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Cannot delete student: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ENROLL STUDENT
        // GET: /Student/Enroll/5
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Enroll(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // =====================================================
        // SUPPORT METHODS
        // =====================================================

        private static void NormalizeStudent(Student student)
        {
            student.StudentCode =
                NullIfWhiteSpace(student.StudentCode);

            student.FullName =
                student.FullName?.Trim() ?? string.Empty;

            student.Email =
                NullIfWhiteSpace(student.Email);

            student.Phone =
                NullIfWhiteSpace(student.Phone);

            student.Address =
                NullIfWhiteSpace(student.Address);

            student.Program =
                NullIfWhiteSpace(student.Program);

            student.Gender =
                NullIfWhiteSpace(student.Gender);
        }

        private static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string GetDatabaseError(
            DbUpdateException exception)
        {
            return exception.InnerException?.InnerException?.Message
                ?? exception.InnerException?.Message
                ?? exception.Message;
        }
    }
}