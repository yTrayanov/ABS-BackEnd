using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;

namespace ABS_Data.Data
{
    public class ABSContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionUrl;
        private readonly string _accessKey;
        private readonly string _secretAccessKey;

        public ABSContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var section = _configuration.GetSection("DynamoDb");
            _connectionUrl = section.GetSection("Url").Value;
            _accessKey = section.GetSection("AccessKey").Value;
            _secretAccessKey = section.GetSection("SecretAccessKey").Value;
        }

        public IAmazonDynamoDB CreateConnection()
        => new AmazonDynamoDBClient(_accessKey, _secretAccessKey, new AmazonDynamoDBConfig {ServiceURL=_connectionUrl});

    }
}
