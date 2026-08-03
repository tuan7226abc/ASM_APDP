using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.DatabaseContext.Entities
{
    [Table("Grades")]
    public class Grade
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Range(0, 10)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AssignmentScore { get; set; }

        [Range(0, 10)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? MidtermScore { get; set; }

        [Range(0, 10)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalScore { get; set; }

        [Range(0, 10)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TotalScore { get; set; }

        [StringLength(250)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public Course? Course { get; set; }

        public Student? Student { get; set; }

        public Teacher? Teacher { get; set; }

        public void CalculateTotalScore()
        {
            decimal assignment = AssignmentScore ?? 0;
            decimal midterm = MidtermScore ?? 0;
            decimal final = FinalScore ?? 0;

            TotalScore = Math.Round(
                assignment * 0.20m +
                midterm * 0.30m +
                final * 0.50m,
                2);
        }
    }
}