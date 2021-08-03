namespace Common.ResponsesModels
{
    public class LoginResponseModel
    {
        public LoginResponseModel(string token , bool isAdmin)
        {
            this.Token = token;
            this.IsAdmin = isAdmin;
        }
        public string Token { get; set; }
        public bool IsAdmin { get; set; }
    }
}
