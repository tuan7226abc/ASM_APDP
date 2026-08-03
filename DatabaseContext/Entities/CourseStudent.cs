namespace SIMS.DatabaseContext.Entities
{
    public class CourseStudent
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime? EnrollmentDate { get; set; } = DateTime.Now;
        public string? Grade { get; set; }
    }
}