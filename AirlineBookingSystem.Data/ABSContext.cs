using AirlineBookingSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AirlineBookingSystem.Data
{
    public class ABSContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ABSContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("AbsContext");
        }

        public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);

    }
}
