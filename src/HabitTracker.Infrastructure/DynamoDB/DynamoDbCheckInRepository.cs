using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.Repositories;

namespace HabitTracker.Infrastructure.DynamoDB;

public class DynamoDbCheckInRepository(IAmazonDynamoDB client) : ICheckInRepository
{
    public async Task<List<CheckIn>> GetByDateRangeAsync(string userId, DateOnly from, DateOnly to)
    {
        var request = new QueryRequest
        {
            TableName = TableConstants.TableName,
            IndexName = TableConstants.Gsi1IndexName,
            KeyConditionExpression = "GSI1PK = :pk AND GSI1SK BETWEEN :from AND :to",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue { S = TableConstants.UserPrefix + userId } },
                { ":from", new AttributeValue { S = TableConstants.DatePrefix + from.ToString("yyyy-MM-dd") } },
                { ":to", new AttributeValue { S = TableConstants.DatePrefix + to.ToString("yyyy-MM-dd") } }
            }
        };

        var response = await client.QueryAsync(request);
        return response.Items.Select(MapToCheckIn).ToList();
    }

    public async Task<List<CheckIn>> GetByHabitAsync(string userId, Guid habitId)
    {
        var request = new QueryRequest
        {
            TableName = TableConstants.TableName,
            KeyConditionExpression = "PK = :pk AND begins_with(SK, :prefix)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue { S = TableConstants.UserPrefix + userId } },
                { ":prefix", new AttributeValue { S = TableConstants.CheckInPrefix + habitId } }
            }
        };

        var response = await client.QueryAsync(request);
        return response.Items.Select(MapToCheckIn).ToList();
    }

    public async Task CreateAsync(CheckIn checkIn)
    {
        var request = new PutItemRequest
        {
            TableName = TableConstants.TableName,
            Item = MapToItem(checkIn)
        };

        await client.PutItemAsync(request);
    }

    public async Task DeleteAsync(string userId, Guid habitId, DateOnly date)
    {
        var request = new DeleteItemRequest
        {
            TableName = TableConstants.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = TableConstants.UserPrefix + userId } },
                { "SK", new AttributeValue { S = TableConstants.CheckInPrefix + habitId + "#" + date.ToString("yyyy-MM-dd") } }
            }
        };

        await client.DeleteItemAsync(request);
    }

    private static Dictionary<string, AttributeValue> MapToItem(CheckIn c) => new()
    {
        { "PK", new AttributeValue { S = TableConstants.UserPrefix + c.UserId } },
        { "SK", new AttributeValue { S = TableConstants.CheckInPrefix + c.HabitId + "#" + c.Date.ToString("yyyy-MM-dd") } },
        { "GSI1PK", new AttributeValue { S = TableConstants.UserPrefix + c.UserId } },
        { "GSI1SK", new AttributeValue { S = TableConstants.DatePrefix + c.Date.ToString("yyyy-MM-dd") } },
        { "Id", new AttributeValue { S = c.Id.ToString() } },
        { "HabitId", new AttributeValue { S = c.HabitId.ToString() } },
        { "UserId", new AttributeValue { S = c.UserId } },
        { "Date", new AttributeValue { S = c.Date.ToString("yyyy-MM-dd") } },
        { "Note", new AttributeValue { S = c.Note ?? "" } },
        { "CreatedAt", new AttributeValue { S = c.CreatedAt.ToString("O") } }
    };

    private static CheckIn MapToCheckIn(Dictionary<string, AttributeValue> item) => new()
    {
        Id = Guid.Parse(item["Id"].S),
        HabitId = Guid.Parse(item["HabitId"].S),
        UserId = item["UserId"].S,
        Date = DateOnly.Parse(item["Date"].S),
        Note = item["Note"].S,
        CreatedAt = DateTime.Parse(item["CreatedAt"].S)
    };
}