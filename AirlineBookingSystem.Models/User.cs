namespace AirlineBookingSystem.Models
{

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }
        public UserStatus Status { get; set; }
    }
}
