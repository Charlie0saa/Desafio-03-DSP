using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs
{
    public class RegisterUserDto
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Estudiante"; // Admin, Docente, Estudiante
    }

    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
