using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Repositories;
using HabitTracker.Application.Stats.GetSummary;
using HabitTracker.Functions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Functions.Functions;

public class StatsFunctionHandler
{
  private readonly ICheckInRepository _checkInRepo;
  private readonly IHabitRepository _habitRepo;

  public StatsFunctionHandler()
  {
    var scope = Startup.ServiceProvider.CreateScope();
    _checkInRepo = scope.ServiceProvider.GetRequiredService<ICheckInRepository>();
    _habitRepo = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
  }

  public Task<APIGatewayProxyResponse> GetHabitStatsAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
    ApiResponse.ExecuteAsync(request, context, async userId =>
    {
      var habitId = ApiResponse.GetGuid(request, "id");
      var result = await new GetHabitStatsHandler(_checkInRepo).Handle(new GetHabitStatsQuery(userId, habitId));
      return ApiResponse.Ok(result.Streak);
    });

  public Task<APIGatewayProxyResponse> GetSummaryAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
    ApiResponse.ExecuteAsync(request, context, async userId =>
    {
      var result = await new GetSummaryHandler(_habitRepo, _checkInRepo).Handle(new GetSummaryQuery(userId));
      return ApiResponse.Ok(result);
    });
}
