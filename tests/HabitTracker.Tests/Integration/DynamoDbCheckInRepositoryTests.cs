using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.DynamoDB;

namespace HabitTracker.Tests.Integration;

[Trait("Category", "Integration")]
[Collection(DynamoDbCollection.Name)]
public class DynamoDbCheckInRepositoryTests(DynamoDbFixture fixture)
{
    private readonly DynamoDbCheckInRepository _repo = new(fixture.Client);

    private static CheckIn NewCheckIn(string userId, Guid habitId, DateOnly date, string? note = null) => new()
    {
        UserId = userId,
        HabitId = habitId,
        Date = date,
        Note = note
    };

    [Fact]
    public async Task CreateAndGetByHabit_ReturnsOnlyThatHabitsCheckIns()
    {
        var userId = $"user-{Guid.NewGuid()}";
        var habitA = Guid.NewGuid();
        var habitB = Guid.NewGuid();
        await _repo.CreateAsync(NewCheckIn(userId, habitA, new DateOnly(2026, 10, 1), "first"));
        await _repo.CreateAsync(NewCheckIn(userId, habitA, new DateOnly(2026, 10, 2)));
        await _repo.CreateAsync(NewCheckIn(userId, habitB, new DateOnly(2026, 10, 1)));

        var result = await _repo.GetByHabitAsync(userId, habitA);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(habitA, c.HabitId));
        Assert.Contains(result, c => c.Note == "first");
    }

    [Fact]
    public async Task GetByHabit_DoesNotReturnOtherUsersCheckIns()
    {
        var habitId = Guid.NewGuid();
        var owner = $"user-{Guid.NewGuid()}";
        var other = $"user-{Guid.NewGuid()}";
        await _repo.CreateAsync(NewCheckIn(owner, habitId, new DateOnly(2026, 10, 1)));

        var result = await _repo.GetByHabitAsync(other, habitId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Create_SameHabitAndDateTwice_KeepsSingleCheckIn()
    {
        var userId = $"user-{Guid.NewGuid()}";
        var habitId = Guid.NewGuid();
        var date = new DateOnly(2026, 10, 1);
        await _repo.CreateAsync(NewCheckIn(userId, habitId, date));
        await _repo.CreateAsync(NewCheckIn(userId, habitId, date, "updated"));

        var result = await _repo.GetByHabitAsync(userId, habitId);

        Assert.Single(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCheckIn()
    {
        var userId = $"user-{Guid.NewGuid()}";
        var habitId = Guid.NewGuid();
        var date = new DateOnly(2026, 10, 1);
        await _repo.CreateAsync(NewCheckIn(userId, habitId, date));
        await _repo.CreateAsync(NewCheckIn(userId, habitId, date.AddDays(1)));

        await _repo.DeleteAsync(userId, habitId, date);

        var result = await _repo.GetByHabitAsync(userId, habitId);
        Assert.Single(result);
        Assert.Equal(date.AddDays(1), result[0].Date);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsCheckInsInsideRangeOnly()
    {
        var userId = $"user-{Guid.NewGuid()}";
        var habitId = Guid.NewGuid();
        foreach (var day in new[] { 1, 5, 10, 20 })
            await _repo.CreateAsync(NewCheckIn(userId, habitId, new DateOnly(2026, 10, day)));

        var result = await _repo.GetByDateRangeAsync(userId, new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 10));

        Assert.Equal([new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 10)],
                     result.Select(c => c.Date).OrderBy(d => d));
    }
}
