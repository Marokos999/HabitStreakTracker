using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.CheckIns.CreateCheckIn;
using HabitTracker.Application.CheckIns.DeleteCheckIn;
using HabitTracker.Application.Common;
using HabitTracker.Functions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Functions.Functions;

public class CheckInsFunctionHandler
{
    private readonly CreateCheckInHandler createCheckIn;
    private readonly DeleteCheckInHandler deleteCheckIn;

    public CheckInsFunctionHandler()
    {
        var scope = Startup.ServiceProvider.CreateScope();
        createCheckIn = scope.ServiceProvider.GetRequiredService<CreateCheckInHandler>();
        deleteCheckIn = scope.ServiceProvider.GetRequiredService<DeleteCheckInHandler>();
    }

    public Task<APIGatewayProxyResponse> CreateCheckInAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var body = ApiResponse.ParseBody<CheckInRequest>(request);
            if (body.HabitId == Guid.Empty)
                throw new ValidationException("HabitId is required.");

            var date = ApiResponse.ParseDate(body.Date, "date");
            var command = new CreateCheckInCommand(userId, body.HabitId, date, body.Note);
            var result = await createCheckIn.Handle(command);
            return ApiResponse.Created(result.CheckIn);
        });

    public Task<APIGatewayProxyResponse> DeleteCheckInAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var habitId = ApiResponse.GetGuid(request, "habitId");
            string? rawDate = null;
            request.PathParameters?.TryGetValue("date", out rawDate);
            var date = ApiResponse.ParseDate(rawDate, "date");
            await deleteCheckIn.Handle(new DeleteCheckInCommand(userId, habitId, date));
            return ApiResponse.NoContent();
        });

    public record CheckInRequest(Guid HabitId, string Date, string? Note);
}
