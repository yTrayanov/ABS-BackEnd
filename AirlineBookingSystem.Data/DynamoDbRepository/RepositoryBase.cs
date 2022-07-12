using ABS.Data.DynamoDb;
using Microsoft.Extensions.Configuration;
using System;

namespace ABS.Data.DynamoDbRepository
{
    public abstract class RepositoryBase <TKey , TEntity> where TEntity : class
    {
        protected string PKPrefix = "";
        protected string SKPrefix = "";
        protected string GSI1Prefix = "";
        protected string GSI2Prefix = "";

        protected string PKPattern { get { return $"{PKPrefix}{DynamoDBConstants.Separator}{{0}}"; } }
        protected string SKPattern { get { return $"{SKPrefix}{DynamoDBConstants.Separator}{{0}}"; } }
        protected string GSI1Pattern { get { return $"{GSI1Prefix}{DynamoDBConstants.Separator}{{0}}"; } }
        protected string GSI2Pattern { get { return $"{GSI2Prefix}{DynamoDBConstants.Separator}{{0}}"; } }

        protected DynamoDBClient _dynamoDbClient;

        public RepositoryBase(IConfiguration configuration)
        {
            _dynamoDbClient = new DynamoDBClient(configuration);
        }

        protected abstract DynamoDBItem ToDynamoDb(TEntity item);
        protected abstract TEntity FromDynamoDb(DynamoDBItem item);

        protected string PKValue(object id)
        {
            return string.Format(PKPattern, Convert.ToString(id));
        }

        protected string SKValue(object id)
        {
            return string.Format(SKPattern, Convert.ToString(id));
        }

        protected string GSI1Value(object id)
        {
            return string.Format(GSI1Pattern, Convert.ToString(id));
        }

        protected string GSI2Value(object id)
        {
            return string.Format(GSI2Pattern, Convert.ToString(id));
        }

    }
}
