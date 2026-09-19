using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.Habits.CreateHabit;
using HabitTracker.Application.Habits.DeleteHabit;
using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Habits.UpdateHabit;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

[assembly:
LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HabitTracker.Functions.Functions;

public class HabitsFunctionHandler
{
  private readonly IHabitRepository repo;
  private static readonly JsonSerializerOptions JsonOptions = new ()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public HabitsFunctionHandler()
  {
    var scope = Startup.ServiceProvider.CreateScope();
    repo = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
  }

  public async Task<APIGatewayProxyResponse> GetHabitsAsync(APIGatewayProxyRequest request, ILambdaContext context)
  {
    var result = await new GetHabitsHandler(repo).Handle(new GetHabitsQuery(GetUserId(request)));
        return Ok(result.Habits);
  }

  public async Task<APIGatewayProxyResponse> CreateHabitAsync(APIGatewayProxyRequest request, ILambdaContext context)
  {
    var body = JsonSerializer.Deserialize<HabitRequest>(request.Body, JsonOptions)!;
    var command = new CreateHabitCommand(GetUserId(request), body.Name, body.Description, body.Frequency,
                                                              body.Color, body.TargetDaysPerWeek);
    var result = await new CreateHabitHandler(repo).Handle(command);
    return Created(result.Habit);
  }


  public async Task<APIGatewayProxyResponse> UpdateHabitAsync(APIGatewayProxyRequest request, ILambdaContext context)
  {
    var habitId = Guid.Parse(request.PathParameters["id"]);
    var body = JsonSerializer.Deserialize<HabitRequest>(request.Body, JsonOptions)!;
    var command = new UpdateHabitCommand(GetUserId(request), habitId,body.Name, body.Description, body.Frequency,
                                                              body.Color, body.TargetDaysPerWeek);
     await new UpdateHabitHandler(repo).Handle(command);
    return NoContent();
  }

  public async Task<APIGatewayProxyResponse> DeleteHabitAsync(APIGatewayProxyRequest request, ILambdaContext context)
  {
    var habitId = Guid.Parse(request.PathParameters["id"]);
    await new DeleteHabitHandler(repo).Handle(new DeleteHabitCommand(GetUserId(request), habitId));
    return NoContent();
  }


  private static string GetUserId(APIGatewayProxyRequest request)
  {
    if (request.RequestContext?.Authorizer?.Claims == null)
      return "local-test-user";
    return request.RequestContext.Authorizer.Claims["sub"];
  }

  private static APIGatewayProxyResponse Ok(object body) => new ()
  {
    StatusCode = (int)HttpStatusCode.OK,
    Body = JsonSerializer.Serialize(body, JsonOptions),
    Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
  };

  private static APIGatewayProxyResponse Created(object body) => new()
  {
    StatusCode = (int)HttpStatusCode.Created,
    Body = JsonSerializer.Serialize(body, JsonOptions),
    Headers = new Dictionary<string, string>{["Content-Type"] = "application/json"}
  };private static APIGatewayProxyResponse NoContent() => new()
  {
    StatusCode = (int)HttpStatusCode.NoContent
  };

  public record HabitRequest
  (
    string Name,
    string? Description,
    HabitFrequency Frequency,
    string Color,
    int TargetDaysPerWeek
  );
}