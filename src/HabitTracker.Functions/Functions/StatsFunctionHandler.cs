using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Repositories;
using HabitTracker.Application.Stats.GetSummary;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Functions.Functions;

public class StatsFunctionHandler
{
    private readonly ICheckInRepository _checkInRepo;
    private readonly IHabitRepository _habitRepo;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StatsFunctionHandler()
    {
        var scope = Startup.ServiceProvider.CreateScope();
        _checkInRepo = scope.ServiceProvider.GetRequiredService<ICheckInRepository>();
        _habitRepo = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
    }

    public async Task<APIGatewayProxyResponse> GetHabitStatsAsync(
        APIGatewayProxyRequest request, ILambdaContext context)
    {
        var habitId = Guid.Parse(request.PathParameters["id"]);
        var query = new GetHabitStatsQuery(GetUserId(request), habitId);
        var result = await new GetHabitStatsHandler(_checkInRepo).Handle(query);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result.Streak, JsonOptions),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }

    public async Task<APIGatewayProxyResponse> GetSummaryAsync(
        APIGatewayProxyRequest request, ILambdaContext context)
    {
        var query = new GetSummaryQuery(GetUserId(request));
        var result = await new GetSummaryHandler(_habitRepo, _checkInRepo).Handle(query);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result, JsonOptions),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }

    private static string GetUserId(APIGatewayProxyRequest request)
    {
        if (request.RequestContext?.Authorizer?.Claims == null)
            return "local-test-user";
        return request.RequestContext.Authorizer.Claims["sub"];
    }
}
