using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.DatabaseContext.Entities
{
    [Table("Attendances")]
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Present";

        [StringLength(250)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public Schedule? Schedule { get; set; }

        public Student? Student { get; set; }
    }
}