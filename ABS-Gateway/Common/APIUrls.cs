namespace ABS_Gateway.Common
{
    public class APIUrls
    {

        public const string AUTH = "auth";
        public const string AUTH_ENDPOINT = "auth/authorize";
        public const string ADMIN_ENDPOINT = "auth/authorize/admin";

        public string AuthApi { get; set; }
        public string FlightApi { get; set; }

        public string TicketApi { get; set; }

        /// <summary>
        /// Url for creating sections , airlines , airports.
        /// </summary>
        public string CreateApi { get; set; }
    }
}
