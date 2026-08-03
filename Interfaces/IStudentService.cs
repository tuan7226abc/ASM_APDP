using SIMS.DatabaseContext.Entities;

namespace SIMS.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(int id);
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(int id);

    }
}