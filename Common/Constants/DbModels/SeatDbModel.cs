using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abs.Common.Constants.DbModels
{
    public static class SeatDbModel
    {
        public const string Prefix = "SEAT#";
        public const string Id = DbConstants.PK;
        public const string SectionId = DbConstants.GSI1;
        public const string FlightId = DbConstants.SK;
        public const string Data = DbConstants.Data;

    }
}
