using System.ComponentModel.DataAnnotations;

namespace ABS_Auth.Models
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
