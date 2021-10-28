namespace Abs.Common.Constants.DbModels
{
    public class TicketDbModel
    {

        public const string Prefix = "TICKET#";

        public const string Id = DbConstants.PK;
        public const string UserId = DbConstants.SK;
        public const string Data = DbConstants.Data;
        public const string FlightId = DbConstants.GSI1;
        public const string SeatId = DbConstants.GSI2;
    }
}
