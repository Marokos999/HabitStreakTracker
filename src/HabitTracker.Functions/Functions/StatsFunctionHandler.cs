using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Stats.GetSummary;
using HabitTracker.Functions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Functions.Functions;

public class StatsFunctionHandler
{
    private readonly GetHabitStatsHandler getHabitStats;
    private readonly GetSummaryHandler getSummary;

    public StatsFunctionHandler()
    {
        var scope = Startup.ServiceProvider.CreateScope();
        getHabitStats = scope.ServiceProvider.GetRequiredService<GetHabitStatsHandler>();
        getSummary = scope.ServiceProvider.GetRequiredService<GetSummaryHandler>();
    }

    [Logging(ClearState = true)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> GetHabitStatsAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        await ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var habitId = ApiResponse.GetGuid(request, "id");
            var result = await getHabitStats.Handle(new GetHabitStatsQuery(userId, habitId));
            return ApiResponse.Ok(HabitStatsResponse.From(result));
        });

    [Logging(ClearState = true)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> GetSummaryAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        await ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var result = await getSummary.Handle(new GetSummaryQuery(userId));
            return ApiResponse.Ok(result);
        });

    // Streak fields stay at the top level, so existing clients keep working.
    public record HabitStatsResponse(
        int CurrentStreak,
        int LongestStreak,
        int TotalCheckIns,
        double CompletionRate,
        IReadOnlyList<DateOnly> CheckInDates)
    {
        public static HabitStatsResponse From(GetHabitStatsResult result) => new(
            result.Streak.CurrentStreak,
            result.Streak.LongestStreak,
            result.Streak.TotalCheckIns,
            result.Streak.CompletionRate,
            result.RecentCheckInDates);
    }
}
