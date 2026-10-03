using Amazon.DynamoDBv2;
using HabitTracker.Application.CheckIns.CreateCheckIn;
using HabitTracker.Application.CheckIns.DeleteCheckIn;
using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Habits.CreateHabit;
using HabitTracker.Application.Habits.DeleteHabit;
using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Habits.UpdateHabit;
using HabitTracker.Application.Repositories;
using HabitTracker.Application.Stats.GetSummary;
using HabitTracker.Infrastructure.DynamoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HabitTracker.Functions;

public static class Startup
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider ServiceProvider =>
        _serviceProvider ??= BuildServiceProvider();

    private static IServiceProvider BuildServiceProvider()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var isLocal = Environment.GetEnvironmentVariable("IS_LOCAL") == "true";

                if (isLocal)
                    services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(
                        new AmazonDynamoDBConfig { ServiceURL = "http://dynamodb-local:8000" }));
                else
                    services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

                services.AddScoped<IHabitRepository, DynamoDbHabitRepository>();
                services.AddScoped<ICheckInRepository, DynamoDbCheckInRepository>();

                services.AddScoped<CreateHabitHandler>();
                services.AddScoped<UpdateHabitHandler>();
                services.AddScoped<DeleteHabitHandler>();
                services.AddScoped<GetHabitsHandler>();
                services.AddScoped<CreateCheckInHandler>();
                services.AddScoped<DeleteCheckInHandler>();
                services.AddScoped<GetHabitStatsHandler>();
                services.AddScoped<GetSummaryHandler>();
            })
            .Build();

        return host.Services;
    }
}
