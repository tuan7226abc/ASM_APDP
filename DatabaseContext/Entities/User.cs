using System.ComponentModel.DataAnnotations;

namespace SIMS.DatabaseContext.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must contain at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        public string Role { get; set; } = "Student";

        public byte Status { get; set; } = 1;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}