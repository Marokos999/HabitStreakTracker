using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using HabitTracker.Application.Habits.CreateHabit;
using HabitTracker.Application.Habits.DeleteHabit;
using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Habits.UpdateHabit;
using HabitTracker.Domain.Enums;
using HabitTracker.Functions.Http;
using Microsoft.Extensions.DependencyInjection;

[assembly:
LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HabitTracker.Functions.Functions;

public class HabitsFunctionHandler
{
    private readonly GetHabitsHandler getHabits;
    private readonly CreateHabitHandler createHabit;
    private readonly UpdateHabitHandler updateHabit;
    private readonly DeleteHabitHandler deleteHabit;

    public HabitsFunctionHandler()
    {
        var scope = Startup.ServiceProvider.CreateScope();
        getHabits = scope.ServiceProvider.GetRequiredService<GetHabitsHandler>();
        createHabit = scope.ServiceProvider.GetRequiredService<CreateHabitHandler>();
        updateHabit = scope.ServiceProvider.GetRequiredService<UpdateHabitHandler>();
        deleteHabit = scope.ServiceProvider.GetRequiredService<DeleteHabitHandler>();
    }

    [Logging(ClearState = true)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> GetHabitsAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        await ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var result = await getHabits.Handle(new GetHabitsQuery(userId));
            return ApiResponse.Ok(result.Habits);
        });

    [Logging(ClearState = true)]
    [Tracing]
    [Metrics(CaptureColdStart = true)]
    public async Task<APIGatewayProxyResponse> CreateHabitAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        await ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var body = ApiResponse.ParseBody<HabitRequest>(request);
            var command = new CreateHabitCommand(userId, body.Name, body.Description, body.Frequency,
                                                 body.Color, body.TargetDaysPerWeek);
            var result = await createHabit.Handle(command);
            Metrics.AddMetric("HabitCreated", 1, MetricUnit.Count);
            return ApiResponse.Created(result.Habit);
        });

    [Logging(ClearState = true)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> UpdateHabitAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        await ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var habitId = ApiResponse.GetGuid(request, "id");
            var body = ApiResponse.ParseBody<HabitRequest>(request);
            var command = new UpdateHabitCommand(userId, habitId, body.Name, body.Description, body.Frequency,
                                                 body.Color, body.TargetDaysPerWeek);
            await updateHabit.Handle(command);
            return ApiResponse.NoContent();
        });

    [Logging(ClearState = true)]
    [Tracing]
    [Metrics(CaptureColdStart = true)]
    public async Task<APIGatewayProxyResponse> DeleteHabitAsync(APIGatewayProxyRequest request, ILambdaContext context) =>
        await ApiResponse.ExecuteAsync(request, context, async userId =>
        {
            var habitId = ApiResponse.GetGuid(request, "id");
            await deleteHabit.Handle(new DeleteHabitCommand(userId, habitId));
            Metrics.AddMetric("HabitDeleted", 1, MetricUnit.Count);
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
