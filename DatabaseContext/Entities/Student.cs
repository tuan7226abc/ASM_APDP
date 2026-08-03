namespace SIMS.DatabaseContext.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public User? User { get; set; }
    }
}