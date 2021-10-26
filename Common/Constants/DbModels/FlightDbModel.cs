namespace Abs.Common.Constants.DbModels
{
    public static class FlightDbModel
    {
        public const string Prefix = "FLIGHT#";
        public const string Id = DbConstants.PK;
        public const string FlightNumber = DbConstants.PK;
        public const string AirlineId = DbConstants.SK;
        public const string OriginAirportId = DbConstants.GSI1;
        public const string DestinationAirportId = DbConstants.GSI2;
        public const string Data = DbConstants.Data;
    }
}
