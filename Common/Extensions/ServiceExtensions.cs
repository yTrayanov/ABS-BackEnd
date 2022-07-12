using Abs.Common.Constants;
using Abs.Common.Enumerations;
using ABS_Common.ResponsesModels;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace ABS_Common.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigurePasswordSettings(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });
        }

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {

            IConfigurationSection appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            AppSettings appSettings = appSettingsSection.Get<AppSettings>();
            byte[] key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    context.Response.ContentType = "application/json";
                    if (contextFeature != null)
                    {
                        var errorType = contextFeature.Error.GetType().Name;
                        if (errorType == nameof(ArgumentException) || errorType == nameof(ValidationException))
                        {

                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync(new ResponseObject("Something went wrong", contextFeature.Error.Message).ToString());
                        }
                        else if (errorType == nameof(SqlException))
                        {
                            var sqlErrorNumber = (ErrorCodes)((SqlException)contextFeature.Error).Number;
                            string errorMessage;
                            switch (sqlErrorNumber)
                            {
                                case ErrorCodes.UsernameExists:
                                    errorMessage = ErrorMessages.UsernameExists;
                                    break;
                                case ErrorCodes.EmailExists:
                                    errorMessage = ErrorMessages.EmailExists;
                                    break;
                                case ErrorCodes.AirportExists:
                                    errorMessage = ErrorMessages.AirportExists;
                                    break;
                                case ErrorCodes.AirlineExists:
                                    errorMessage = ErrorMessages.AirlineExists;
                                    break;
                                case ErrorCodes.SeatClassExists:
                                    errorMessage = ErrorMessages.SeatClassExists;
                                    break;
                                case ErrorCodes.FlightNumberExists:
                                    errorMessage = ErrorMessages.FlightNumberExists;
                                    break;
                                case ErrorCodes.UserNotFound:
                                    errorMessage = ErrorMessages.UserNotFound;
                                    break;
                                case ErrorCodes.FlightNotFound:
                                    errorMessage = ErrorMessages.FlightNotFound;
                                    break;
                                case ErrorCodes.SeatNotFound:
                                    errorMessage = ErrorMessages.SeatNotFound;
                                    break;
                                case ErrorCodes.AirportNotFound:
                                    errorMessage = ErrorMessages.AirportNotFound;
                                    break;
                                case ErrorCodes.AirlineNotFound:
                                    errorMessage = ErrorMessages.AirlineNotFound;
                                    break;
                                case ErrorCodes.InvalidCredentials:
                                    errorMessage = ErrorMessages.InvalidCredentials;
                                    errorMessage = ErrorMessages.InvalidCredentials;
                                    break;
                                default:
                                    errorMessage = contextFeature.Error.Message;
                                    break;
                            }

                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync(new ResponseObject("Something went wrong", errorMessage).ToString());

                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            await context.Response.WriteAsync(new ResponseObject("Something went wrong", contextFeature.Error.Message).ToString());
                        }
                    }

                });
            });
        }

        public static async void CreateDatabaseTables(this IApplicationBuilder app , IConfiguration configuration)
        {

            var section = configuration.GetSection("DynamoDb");
            var url = section.GetSection("Url").Value;
            var accessKey = section.GetSection("AccessKey").Value;
            var secretKey = section.GetSection("SecretAccessKey").Value;

            IAmazonDynamoDB dynamoDb = new AmazonDynamoDBClient(accessKey, secretKey, new AmazonDynamoDBConfig { ServiceURL = url});
            string absTable = "ABS-Table";
            string userTable = "ABS-UsersTable";

            await dynamoDb.DeleteTableAsync(absTable);
            await dynamoDb.DeleteTableAsync(userTable);


            var tables = await dynamoDb.ListTablesAsync();

            if (!tables.TableNames.Contains(absTable))
            {
                var tableRequest = new CreateTableRequest()
                {
                    TableName = absTable,
                    AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        AttributeName = "PK",
                        AttributeType = ScalarAttributeType.S,
                    },
                    new AttributeDefinition()
                    {
                        AttributeName = "SK",
                        AttributeType = ScalarAttributeType.S,
                    }
                },
                    KeySchema = new List<KeySchemaElement>()
                {
                    new KeySchemaElement("PK", KeyType.HASH),
                    new KeySchemaElement("SK", KeyType.RANGE),
                },
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>()
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "GSI1",
                        KeySchema = new List<KeySchemaElement>()
                        {
                            new KeySchemaElement
                            {
                                AttributeName = "PK",
                                KeyType = KeyType.HASH
                            }
                        },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5),
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.ALL
                        }
                    },
                    new GlobalSecondaryIndex()
                    {
                        IndexName = "GSI2",
                        KeySchema = new List<KeySchemaElement>()
                        {
                            new KeySchemaElement("PK", KeyType.HASH)
                        },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5),
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.ALL
                        }

                    }
                },
                    ProvisionedThroughput = new ProvisionedThroughput(5, 5),
                };

                await dynamoDb.CreateTableAsync(tableRequest);
            }

            if (!tables.TableNames.Contains(userTable))
            {
                var tableRequest = new CreateTableRequest()
                {
                    TableName = "ABS-UsersTable",
                    AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        AttributeName = "PK",
                        AttributeType = ScalarAttributeType.S,
                    },
                },
                    KeySchema = new List<KeySchemaElement>()
                {
                    new KeySchemaElement("PK", KeyType.HASH),
                },

                    ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                };

                await dynamoDb.CreateTableAsync(tableRequest);
            }


            var adminUser = await dynamoDb.GetItemAsync(new GetItemRequest
            {
                TableName = userTable,
                Key = new Dictionary<string, AttributeValue> { { "PK", new AttributeValue("admin") } }
            });

            if(adminUser.Item.Count <= 0)
            {
                await dynamoDb.PutItemAsync(new PutItemRequest()
                {
                    TableName = "ABS-UsersTable",
                    Item = new Dictionary<string, AttributeValue>
                {
                    {"PK" , new AttributeValue("admin") },
                    {"Status" , new AttributeValue("Registered") },
                    {"Roles" , new AttributeValue("Admin") },
                    {"Email" , new AttributeValue("admin@admin.bg") },
                    {"Password", new AttributeValue("user123") }
                }
                });
            }

            dynamoDb.Dispose();
        }
    }
}
