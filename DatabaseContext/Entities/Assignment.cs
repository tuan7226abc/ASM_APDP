using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.DatabaseContext.Entities
{
    [Table("Assignments")]
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxScore { get; set; } = 10;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public Course? Course { get; set; }

        public Teacher? Teacher { get; set; }
    }
}