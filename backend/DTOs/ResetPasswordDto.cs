using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Otp is required.")]
        public required string Otp { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 15 characters long.")]
        public required string NewPassword { get; set; }
    }
}
