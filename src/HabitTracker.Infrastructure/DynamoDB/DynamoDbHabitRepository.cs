using HabitTracker.Domain.Enums;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.Repositories;

namespace HabitTracker.Infrastructure.DynamoDB;

public class DynamoDbHabitRepository(IAmazonDynamoDB clinet) : IHabitRepository
{
    public async Task CreateAsync(Habit habit)
    {
        var request = new PutItemRequest
        {
            TableName = TableConstants.TableName,
            Item = MapToItem(habit)
        };

        await clinet.PutItemAsync(request);
    }

    public async Task<List<Habit>> GetAllAsync(string userId)
    {
        var request = new QueryRequest
        {
            TableName = TableConstants.TableName,
            KeyConditionExpression = "PK = :pk AND begins_with(SK, :prefix)",
            FilterExpression = "IsArchived = :archived",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
      {
        { ":pk", new AttributeValue { S = TableConstants.UserPrefix + userId } },
        { ":prefix", new AttributeValue { S = TableConstants.HabitPrefix } },
        { ":archived", new AttributeValue { BOOL = false } }
      }
        };

        var response = await clinet.QueryAsync(request);
        return response.Items.Select(MapToHabit).ToList();
    }

    public async Task<Habit?> GetByIdAsync(string userId, Guid habitId)
    {
        var request = new GetItemRequest
        {
            TableName = TableConstants.TableName,
            Key = new Dictionary<string, AttributeValue>
      {
        {"PK", new AttributeValue {S = TableConstants.UserPrefix + userId}},
        {"SK", new AttributeValue {S = TableConstants.HabitPrefix + habitId}}
      }
        };

        var response = await clinet.GetItemAsync(request);
        return response.Item.Count == 0 ? null : MapToHabit(response.Item);
    }

    public async Task SoftDeleteAsync(string userId, Guid habitId)
    {
        var habit = await GetByIdAsync(userId, habitId);
        if (habit is null) return;
        habit.IsArchived = true;
        await UpdateAsync(habit);
    }

    public async Task UpdateAsync(Habit habit)
    {
        await CreateAsync(habit);
    }

    private static Habit MapToHabit(Dictionary<string, AttributeValue> item) => new()
    {
        Id = Guid.Parse(item["Id"].S),
        UserId = item["UserId"].S,
        Name = item["Name"].S,
        Description = item["Description"].S,
        Frequency = Enum.Parse<HabitFrequency>(item["Frequency"].S),
        Color = item["Color"].S,
        TargetDaysPerWeek = int.Parse(item["TargetDaysPerWeek"].N),
        IsArchived = item["IsArchived"].BOOL,
        CreatedAt = DateTime.Parse(item["CreatedAt"].S)
    };

    private static Dictionary<string, AttributeValue> MapToItem(Habit h) => new()
    {
        { "PK", new AttributeValue { S = TableConstants.UserPrefix + h.UserId } },
        { "SK", new AttributeValue { S = TableConstants.HabitPrefix + h.Id } },
        { "Id", new AttributeValue { S = h.Id.ToString() } },
        { "UserId", new AttributeValue { S = h.UserId } },
        { "Name", new AttributeValue { S = h.Name } },
        { "Description", new AttributeValue { S = h.Description ?? "" } },
        { "Frequency", new AttributeValue { S = h.Frequency.ToString() } },
        { "Color", new AttributeValue { S = h.Color } },
        { "TargetDaysPerWeek", new AttributeValue { N = h.TargetDaysPerWeek.ToString() } },
        { "IsArchived", new AttributeValue { BOOL = h.IsArchived } },
        { "CreatedAt", new AttributeValue { S = h.CreatedAt.ToString("O") } }
    };
}
