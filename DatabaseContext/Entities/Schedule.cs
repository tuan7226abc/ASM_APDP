using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.DatabaseContext.Entities
{
    [Table("Schedules")]
    public class Schedule
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Teacher is required.")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Day of week is required.")]
        [Range(2, 8)]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Room is required.")]
        [StringLength(50)]
        public string Room { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Course? Course { get; set; }

        public Teacher? Teacher { get; set; }
    }
}