using System.ComponentModel.DataAnnotations;

namespace ABS_Auth.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
