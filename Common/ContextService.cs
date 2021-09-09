﻿using AirlineBookingSystem.Data;
using System;
using System.Data;

namespace AirlineBookingSystem.Common
{
    public class ContextService : IDisposable
    {
        public ContextService(ABSContext context)
        {
            Connection = context.CreateConnection();
        }

        public IDbConnection Connection { get; set; }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
