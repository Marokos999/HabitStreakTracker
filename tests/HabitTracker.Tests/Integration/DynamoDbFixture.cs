using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using HabitTracker.Infrastructure.DynamoDB;
using Testcontainers.DynamoDb;

namespace HabitTracker.Tests.Integration;

// Starts a throwaway DynamoDB Local container and creates the table once for all integration tests.
public class DynamoDbFixture : IAsyncLifetime
{
    private readonly DynamoDbContainer _container = new DynamoDbBuilder()
        .WithImage("amazon/dynamodb-local:latest")
        .Build();

    public IAmazonDynamoDB Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Client = new AmazonDynamoDBClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonDynamoDBConfig { ServiceURL = _container.GetConnectionString() });

        await Client.CreateTableAsync(new CreateTableRequest
        {
            TableName = TableConstants.TableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions =
            [
                new AttributeDefinition("PK", ScalarAttributeType.S),
                new AttributeDefinition("SK", ScalarAttributeType.S),
                new AttributeDefinition("GSI1PK", ScalarAttributeType.S),
                new AttributeDefinition("GSI1SK", ScalarAttributeType.S)
            ],
            KeySchema =
            [
                new KeySchemaElement("PK", KeyType.HASH),
                new KeySchemaElement("SK", KeyType.RANGE)
            ],
            GlobalSecondaryIndexes =
            [
                new GlobalSecondaryIndex
                {
                    IndexName = TableConstants.Gsi1IndexName,
                    KeySchema =
                    [
                        new KeySchemaElement("GSI1PK", KeyType.HASH),
                        new KeySchemaElement("GSI1SK", KeyType.RANGE)
                    ],
                    Projection = new Projection { ProjectionType = ProjectionType.ALL }
                }
            ]
        });
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class DynamoDbCollection : ICollectionFixture<DynamoDbFixture>
{
    public const string Name = "DynamoDB";
}
