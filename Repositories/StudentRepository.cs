using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SimsDbContext _context;

        public StudentRepository(SimsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.StudentCode)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Student student)
        {
            try
            {
                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                string message =
                    ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                throw new InvalidOperationException(
                    "Database error while adding student: " + message,
                    ex);
            }
        }

        public async Task UpdateAsync(Student student)
        {
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == student.Id);

            if (existingStudent == null)
            {
                throw new KeyNotFoundException("Student not found.");
            }

            try
            {
                existingStudent.StudentCode =
                    string.IsNullOrWhiteSpace(student.StudentCode)
                        ? null
                        : student.StudentCode.Trim();

                existingStudent.FullName =
                    student.FullName?.Trim() ?? string.Empty;

                existingStudent.Email =
                    string.IsNullOrWhiteSpace(student.Email)
                        ? null
                        : student.Email.Trim();

                existingStudent.DateOfBirth =
                    student.DateOfBirth;

                existingStudent.Program =
                    string.IsNullOrWhiteSpace(student.Program)
                        ? null
                        : student.Program.Trim();

                existingStudent.Phone =
                    string.IsNullOrWhiteSpace(student.Phone)
                        ? null
                        : student.Phone.Trim();

                existingStudent.Address =
                    string.IsNullOrWhiteSpace(student.Address)
                        ? null
                        : student.Address.Trim();

                existingStudent.Gender =
                    string.IsNullOrWhiteSpace(student.Gender)
                        ? null
                        : student.Gender.Trim();

                // Không cập nhật UserId từ form Edit.
                // Giữ nguyên tài khoản đang liên kết.
                // existingStudent.UserId = student.UserId;

                existingStudent.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                string message =
                    ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                throw new InvalidOperationException(
                    "Database error while updating student: " + message,
                    ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return;
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}