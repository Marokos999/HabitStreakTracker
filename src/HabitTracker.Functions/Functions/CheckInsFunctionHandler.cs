using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.CheckIns.CreateCheckIn;
using HabitTracker.Application.CheckIns.DeleteCheckIn;
using HabitTracker.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Functions.Functions;

public class CheckInsFunctionHandler
{
  private readonly ICheckInRepository repo;

  private static readonly JsonSerializerOptions JsonOptions = new ()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public CheckInsFunctionHandler()
  {
    var scope = Startup.ServiceProvider.CreateScope();
    repo = scope.ServiceProvider.GetRequiredService<ICheckInRepository>();
  }

  public async Task<APIGatewayProxyResponse> CreateCheckInAsync(APIGatewayProxyRequest request, ILambdaContext context)
  {
    var body = JsonSerializer.Deserialize<CheckInRequest>(request.Body, JsonOptions)!;
    var command = new CreateCheckInCommand(GetUserId(request), body.HabitId, DateOnly.Parse(body.Date), body.Note);
    var result = await new CreateCheckInHandler(repo).Handle(command);

    return new APIGatewayProxyResponse
    {
      StatusCode = (int)HttpStatusCode.Created,
      Body = JsonSerializer.Serialize(result.CheckIn, JsonOptions),
      Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
    };
  }

  public async Task<APIGatewayProxyResponse> DeleteCheckInAsync(APIGatewayProxyRequest request, ILambdaContext context)
  {
    var habitId = Guid.Parse(request.PathParameters["habitId"]);
    var date = DateOnly.Parse(request.PathParameters["date"]);
    await new DeleteCheckInHandler(repo).Handle(new DeleteCheckInCommand(GetUserId(request), habitId, date));

    return new APIGatewayProxyResponse
    {
      StatusCode = (int)HttpStatusCode.NoContent
    };
  }



  private static string GetUserId(APIGatewayProxyRequest request)
  {
    if (request.RequestContext?.Authorizer?.Claims == null)
      return "local-test-user";
    return request.RequestContext.Authorizer.Claims["sub"];
  }

  public record CheckInRequest(Guid HabitId, string Date, string? Note);
}