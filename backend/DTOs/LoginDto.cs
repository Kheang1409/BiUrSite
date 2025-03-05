using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public required string password { get; set; }
    }
}