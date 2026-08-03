using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.DatabaseContext.Entities
{
    [Table("Teachers")]
    public class Teacher
    {
        [Key]
        [Column("TeacherId")]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TeacherCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public int? UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}