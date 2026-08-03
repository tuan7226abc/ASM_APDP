namespace SIMS.DatabaseContext.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int? MaxStudents { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}