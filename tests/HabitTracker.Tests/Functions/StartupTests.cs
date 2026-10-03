using HabitTracker.Application.CheckIns.CreateCheckIn;
using HabitTracker.Application.CheckIns.DeleteCheckIn;
using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Habits.CreateHabit;
using HabitTracker.Application.Habits.DeleteHabit;
using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Habits.UpdateHabit;
using HabitTracker.Application.Stats.GetSummary;
using HabitTracker.Functions;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Tests.Functions;

public class StartupTests
{
    [Theory]
    [InlineData(typeof(CreateHabitHandler))]
    [InlineData(typeof(UpdateHabitHandler))]
    [InlineData(typeof(DeleteHabitHandler))]
    [InlineData(typeof(GetHabitsHandler))]
    [InlineData(typeof(CreateCheckInHandler))]
    [InlineData(typeof(DeleteCheckInHandler))]
    [InlineData(typeof(GetHabitStatsHandler))]
    [InlineData(typeof(GetSummaryHandler))]
    public void ServiceProvider_ResolvesHandler(Type handlerType)
    {
        Environment.SetEnvironmentVariable("IS_LOCAL", "true");

        using var scope = Startup.ServiceProvider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService(handlerType));
    }
}
