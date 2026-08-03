using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // ===========================
        // Student List
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> Index()
        {
            var students = await _studentService.GetAllAsync();
            return View(students);
        }

        // ===========================
        // Create Student
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                return View(student);
            }

            student.CreatedAt = DateTime.Now;
            student.UpdatedAt = DateTime.Now;

            await _studentService.AddAsync(student);

            TempData["Success"] = "Student added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Edit Student
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(student);
            }

            try
            {
                student.UpdatedAt = DateTime.Now;

                await _studentService.UpdateAsync(student);

                TempData["Success"] = "Student updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(student);
            }
        }

        // ===========================
        // Delete Student
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentService.DeleteAsync(id);

            TempData["Success"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Student Details
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // ===========================
        // Enroll Student
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Enroll(int id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Enroll(Student student)
        {
            // TODO:
            // Thực hiện đăng ký khóa học cho sinh viên

            TempData["Success"] = "Enrollment completed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}