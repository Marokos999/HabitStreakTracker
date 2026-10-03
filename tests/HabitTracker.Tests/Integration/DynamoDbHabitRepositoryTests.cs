using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using HabitTracker.Infrastructure.DynamoDB;

namespace HabitTracker.Tests.Integration;

[Trait("Category", "Integration")]
[Collection(DynamoDbCollection.Name)]
public class DynamoDbHabitRepositoryTests(DynamoDbFixture fixture)
{
    private readonly DynamoDbHabitRepository _repo = new(fixture.Client);
    private readonly string UserId = $"user-{Guid.NewGuid()}";

    [Fact]
    public async Task CreateAndGetById_ReturnsHabit()
    {
        var habit = new Habit
        {
            UserId = UserId,
            Name = "Integration Test Habit",
            Frequency = HabitFrequency.Daily,
            Color = "#6366F1",
            TargetDaysPerWeek = 7
        };

        await _repo.CreateAsync(habit);
        var fetched = await _repo.GetByIdAsync(UserId, habit.Id);

        Assert.NotNull(fetched);
        Assert.Equal("Integration Test Habit", fetched.Name);
        Assert.Equal(UserId, fetched.UserId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCreatedHabits()
    {
        var habit = new Habit
        {
            UserId = UserId,
            Name = "List Test Habit",
            Frequency = HabitFrequency.Daily,
            Color = "#6366F1",
            TargetDaysPerWeek = 5
        };

        await _repo.CreateAsync(habit);
        var habits = await _repo.GetAllAsync(UserId);

        Assert.Contains(habits, h => h.Name == "List Test Habit");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesHabit()
    {
        var habit = new Habit
        {
            UserId = UserId,
            Name = "Before Update",
            Frequency = HabitFrequency.Daily,
            Color = "#6366F1",
            TargetDaysPerWeek = 7
        };

        await _repo.CreateAsync(habit);
        habit.Name = "After Update";
        await _repo.UpdateAsync(habit);

        var fetched = await _repo.GetByIdAsync(UserId, habit.Id);
        Assert.Equal("After Update", fetched!.Name);
    }

    [Fact]
    public async Task SoftDeleteAsync_ArchivesHabit()
    {
        var habit = new Habit
        {
            UserId = UserId,
            Name = "To Be Deleted",
            Frequency = HabitFrequency.Daily,
            Color = "#6366F1",
            TargetDaysPerWeek = 7
        };

        await _repo.CreateAsync(habit);
        await _repo.SoftDeleteAsync(UserId, habit.Id);

        var habits = await _repo.GetAllAsync(UserId);
        Assert.DoesNotContain(habits, h => h.Id == habit.Id);
    }
}
