using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.Habits.CreateHabit;
using HabitTracker.Application.Habits.DeleteHabit;
using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Habits.UpdateHabit;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Enums;
using HabitTracker.Functions.Http;
using Microsoft.Extensions.DependencyInjection;

[assembly:
LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HabitTracker.Functions.Functions;

public class HabitsFunctionHandler
{
    private readonly IHabitRepository repo;

    public HabitsFunctionHandler()
    {
        var scope = Startup.ServiceProvider.CreateScope();
        repo = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
    }

    public Task<APIGatewayProxyResponse> GetHabitsAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
      ApiResponse.ExecuteAsync(request, context, async userId =>
      {
          var result = await new GetHabitsHandler(repo).Handle(new GetHabitsQuery(userId));
          return ApiResponse.Ok(result.Habits);
      });

    public Task<APIGatewayProxyResponse> CreateHabitAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
      ApiResponse.ExecuteAsync(request, context, async userId =>
      {
          var body = ApiResponse.ParseBody<HabitRequest>(request);
          var command = new CreateHabitCommand(userId, body.Name, body.Description, body.Frequency,
                                             body.Color, body.TargetDaysPerWeek);
          var result = await new CreateHabitHandler(repo).Handle(command);
          return ApiResponse.Created(result.Habit);
      });

    public Task<APIGatewayProxyResponse> UpdateHabitAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
      ApiResponse.ExecuteAsync(request, context, async userId =>
      {
          var habitId = ApiResponse.GetGuid(request, "id");
          var body = ApiResponse.ParseBody<HabitRequest>(request);
          var command = new UpdateHabitCommand(userId, habitId, body.Name, body.Description, body.Frequency,
                                             body.Color, body.TargetDaysPerWeek);
          await new UpdateHabitHandler(repo).Handle(command);
          return ApiResponse.NoContent();
      });

    public Task<APIGatewayProxyResponse> DeleteHabitAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
      ApiResponse.ExecuteAsync(request, context, async userId =>
      {
          var habitId = ApiResponse.GetGuid(request, "id");
          await new DeleteHabitHandler(repo).Handle(new DeleteHabitCommand(userId, habitId));
          return ApiResponse.NoContent();
      });

    public record HabitRequest
    (
      string Name,
      string? Description,
      HabitFrequency Frequency,
      string Color,
      int TargetDaysPerWeek
    );
}
