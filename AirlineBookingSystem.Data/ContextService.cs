using System;
using System.Data;

namespace ABS_Data.Data
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
