using ABS.Data.DynamoDBHelpers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABS.Data.DynamoDb
{
    public class DynamoDBClient
    {
        private string _tableName;

        private IAmazonDynamoDB _dynamoDbClient;
        public DynamoDBClient(IConfiguration configuration)
        {
            var section = configuration.GetSection("DynamoDb");
            var connectionUrl = section.GetSection("Url").Value;
            var accessKey = section.GetSection("AccessKey").Value;
            var secretAccessKey = section.GetSection("SecretAccessKey").Value;
            this._tableName = section.GetSection("TableName").Value;

            this._dynamoDbClient = new AmazonDynamoDBClient(accessKey, secretAccessKey, new AmazonDynamoDBConfig { ServiceURL = connectionUrl });
        }

        private DynamoDBItem DynamoDBKey(string pk, string sk,string prefix = "")
        {
            var dbItem = new DynamoDBItem();
            dbItem.AddPK(pk ,prefix);

            if(sk != null)
                dbItem.AddSK(sk);
            return dbItem;
        }



        public async Task PutItemAsync(string pk, string sk, DynamoDBItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var dbItemKey = DynamoDBKey(pk, sk);
            var dbItemData = dbItemKey.MergeData(item);

            var putItemRq = new PutItemRequest
            {
                TableName = _tableName,
                Item = dbItemData.ToDictionary(),
                Expected = new Dictionary<string, ExpectedAttributeValue>()
                {
                    {DynamoDBConstants.PK, new ExpectedAttributeValue(false)}
                },
            };

            await _dynamoDbClient.PutItemAsync(putItemRq);
        }

        public async Task<List<DynamoDBItem>> ScanItemsAsync(string filterExpression = null, Dictionary<string, AttributeValue> expressionValues = null , Dictionary<string , string> expressionNames = null)
        {
            var scanItemsRequest = new ScanRequest()
            {
                TableName = _tableName,
            };

            if (filterExpression != null)
                scanItemsRequest.FilterExpression = filterExpression;

            if (expressionValues != null)
                scanItemsRequest.ExpressionAttributeValues = expressionValues;

            if (expressionNames != null)
                scanItemsRequest.ExpressionAttributeNames = expressionNames;

            var scanResponse = await _dynamoDbClient.ScanAsync(scanItemsRequest);
            var items = new List<DynamoDBItem>();
            foreach (var item in scanResponse.Items)
            {
                items.Add(new DynamoDBItem(item));
            }

            return items;
        }


        public async Task DeleteItemAsync(string pkId, string skId)
        {
            var dbItemKey = DynamoDBKey(pkId, skId);
            var delItemRq = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = dbItemKey.ToDictionary()
            };
            var result = await _dynamoDbClient.DeleteItemAsync(delItemRq);
        }

        public async Task<DynamoDBItem> GetItemAsync(string pk, string sk)
        {
            var dbItemKey = DynamoDBKey(pk, sk);
            var getitemRq = new GetItemRequest
            {
                TableName = _tableName,
                Key = dbItemKey.ToDictionary()
            };
            var getitemResponse = await _dynamoDbClient.GetItemAsync(getitemRq);
            return new DynamoDBItem(getitemResponse.Item);
        }

        public async Task BatchAddItemsAsync(IEnumerable<DynamoDBItem> items)
        {
            var requests = new List<WriteRequest>();

            foreach (var item in items)
            {
                var putRq = new PutRequest(item.ToDictionary());
                requests.Add(new WriteRequest(putRq));
            }

            var index = 0;
            var length = Math.Ceiling(items.Count() / 25f);
            for (int i = 0; i < length ; i++)
            {
                var upperLimit = (items.Count() - index * 25 > 25) ? 25 : items.Count() - index * 25;
                var requestPortion = requests.GetRange(index * 25, upperLimit);

                var batchRq = new Dictionary<string, List<WriteRequest>> { { _tableName, requestPortion } };
                await _dynamoDbClient.BatchWriteItemAsync(batchRq);
                index++;
            }

        }

        public async Task BatchDeleteItemsAsync(IEnumerable<DynamoDBItem> items)
        {
            var requests = new List<WriteRequest>();
            foreach (var item in items)
            {
                var deleteRq = new DeleteRequest(item.ToDictionary());
                requests.Add(new WriteRequest(deleteRq));
            }

            var batchRq = new Dictionary<string, List<WriteRequest>> { { _tableName, requests } };
            var result = await _dynamoDbClient.BatchWriteItemAsync(batchRq);
        }

        public async Task<IList<DynamoDBItem>> QueryAsync(QueryRequest queryRequest)
        {
            var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);
            return queryResponse.Items.Select(x => new DynamoDBItem(x)).ToList();
        }

        public QueryRequest GetGSI1QueryRequest(string gsi1, string skPrefix)
        {
            return new QueryRequest
            {
                TableName = _tableName,
                IndexName = DynamoDBConstants.GSI1,
                KeyConditionExpression = $"{DynamoDBConstants.GSI1} = :gsi1_value and begins_with({DynamoDBConstants.SK}, :sk_prefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":gsi1_value", new AttributeValue(gsi1) },
                    { ":sk_prefix", new AttributeValue(skPrefix) }
                }
            };
        }

        public QueryRequest GetTableQueryRequest(string pk, string skPrefix)
        {
            return new QueryRequest
            {
                TableName = _tableName,
                KeyConditionExpression = $"{DynamoDBConstants.PK} = :pk_value and begins_with({DynamoDBConstants.SK}, :sk_prefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":pk_value", new AttributeValue(pk) },
                    { ":sk_prefix", new AttributeValue(skPrefix) }
                }
            };
        }

        public async Task<DynamoDBItem> UpdateItemAsync(Dictionary<string , AttributeValue> key, string updateExpression, string conditionalExrpession = null , Dictionary<string , AttributeValue> expressionAttributeValues = null , Dictionary<string ,string> expressionAttributeNames = null, UpdateReturnValues returnValues = UpdateReturnValues.NONE)
        {
            var request = new UpdateItemRequest()
            {
                TableName = _tableName,
                Key = key,
                UpdateExpression = updateExpression,
                ReturnValues = new ReturnValue(returnValues.ToString())
            };

            if (conditionalExrpession != null)
                request.ConditionExpression = conditionalExrpession;

            if (expressionAttributeValues != null)
                request.ExpressionAttributeValues = expressionAttributeValues;

            if (expressionAttributeNames != null)
                request.ExpressionAttributeNames = expressionAttributeNames;



            var responseAttributes = (await _dynamoDbClient.UpdateItemAsync(request)).Attributes;
            var item = new DynamoDBItem(responseAttributes);

            return item;
        }

        public async Task BatchUpdateItemAsync(List<DynamoDBItem> items , string updateExpression,Dictionary<string , AttributeValue> expressionAttributeValues ,Dictionary<string,string> expressionAttributeNames = null)
        {
            var transactItems = new List<TransactWriteItem>();

            foreach (var item in items)
            {
                var request = new TransactWriteItem()
                {
                    Update = new Update()
                    {
                        TableName = _tableName,
                        Key = item.ToDictionary(),
                        UpdateExpression = updateExpression,
                    }
                };
                if (expressionAttributeValues != null)
                    request.Update.ExpressionAttributeValues = expressionAttributeValues;
                if (expressionAttributeNames != null)
                    request.Update.ExpressionAttributeNames = expressionAttributeNames;

                transactItems.Add(request);
            }

            await _dynamoDbClient.TransactWriteItemsAsync(new TransactWriteItemsRequest() { TransactItems = transactItems });
        }
    }
}
