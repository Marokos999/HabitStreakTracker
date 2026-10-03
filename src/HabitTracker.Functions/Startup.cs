using Amazon.DynamoDBv2;
using HabitTracker.Application.Repositories;
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
            })
            .Build();

        return host.Services;
    }
}
