using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.DatabaseContext.Entities
{
    [Table("Submissions")]
    public class Submission
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        [StringLength(1000)]
        public string? Feedback { get; set; }

        public DateTime? GradedAt { get; set; }

        public Assignment? Assignment { get; set; }

        public Student? Student { get; set; }
    }
}