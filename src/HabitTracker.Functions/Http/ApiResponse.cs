using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using HabitTracker.Application.Common;

namespace HabitTracker.Functions.Http;

public static class ApiResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Dictionary<string, string> JsonHeaders = new() { ["Content-Type"] = "application/json" };

    public static APIGatewayProxyResponse Ok(object body) => Json(HttpStatusCode.OK, body);

    public static APIGatewayProxyResponse Created(object body) => Json(HttpStatusCode.Created, body);

    public static APIGatewayProxyResponse NoContent() => new() { StatusCode = (int)HttpStatusCode.NoContent };

    public static APIGatewayProxyResponse Error(HttpStatusCode status, string message) =>
      Json(status, new { error = message });

    // Runs the action for the authenticated user and maps known exceptions to HTTP status codes.
    public static async Task<APIGatewayProxyResponse> ExecuteAsync(
      APIGatewayProxyRequest request, ILambdaContext context, Func<string, Task<APIGatewayProxyResponse>> action)
    {
        var userId = GetUserId(request);
        if (userId is null)
        {
            Logger.LogWarning("Request without a user identity was rejected");
            return Error(HttpStatusCode.Unauthorized, "Unauthorized.");
        }

        try
        {
            return await action(userId);
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning($"Validation failed: {ex.Message}");
            return Error(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            Logger.LogInformation($"Not found: {ex.Message}");
            return Error(HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConditionalCheckFailedException)
        {
            Logger.LogWarning("Conditional check failed");
            return Error(HttpStatusCode.Conflict, "Resource already exists or was modified.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception");
            return Error(HttpStatusCode.InternalServerError, "Internal server error.");
        }
    }

    public static T ParseBody<T>(APIGatewayProxyRequest request) where T : class
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ValidationException("Request body is required.");

        try
        {
            return JsonSerializer.Deserialize<T>(request.Body, JsonOptions)
                   ?? throw new ValidationException("Request body is invalid.");
        }
        catch (JsonException)
        {
            throw new ValidationException("Request body is not valid JSON.");
        }
    }

    public static Guid GetGuid(APIGatewayProxyRequest request, string name)
    {
        if (request.PathParameters is null
            || !request.PathParameters.TryGetValue(name, out var raw)
            || !Guid.TryParse(raw, out var id))
            throw new ValidationException($"Path parameter '{name}' must be a valid GUID.");
        return id;
    }

    public static DateOnly ParseDate(string? raw, string name)
    {
        if (!DateOnly.TryParse(raw, out var date))
            throw new ValidationException($"'{name}' must be a valid date (yyyy-MM-dd).");
        return date;
    }

    // Local runs (sam local, no authorizer) fall back to a fixed user; deployed API requires a Cognito "sub" claim.
    private static string? GetUserId(APIGatewayProxyRequest request)
    {
        var claims = request.RequestContext?.Authorizer?.Claims;
        if (claims is not null && claims.TryGetValue("sub", out var sub) && !string.IsNullOrEmpty(sub))
            return sub;

        return Environment.GetEnvironmentVariable("IS_LOCAL") == "true" ? "local-test-user" : null;
    }

    private static APIGatewayProxyResponse Json(HttpStatusCode status, object body) => new()
    {
        StatusCode = (int)status,
        Body = JsonSerializer.Serialize(body, JsonOptions),
        Headers = JsonHeaders
    };
}
