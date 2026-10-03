using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HabitTracker.Application.Common;
using HabitTracker.Functions.Http;
using Moq;

namespace HabitTracker.Tests.Functions;

public class ApiResponseTests : IDisposable
{
    private readonly ILambdaContext _context = Mock.Of<ILambdaContext>();
    private readonly string? _previousIsLocal = Environment.GetEnvironmentVariable("IS_LOCAL");

    public ApiResponseTests() => Environment.SetEnvironmentVariable("IS_LOCAL", null);

    public void Dispose() => Environment.SetEnvironmentVariable("IS_LOCAL", _previousIsLocal);

    private static APIGatewayProxyRequest RequestFor(string? sub) => new()
    {
        RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
        {
            Authorizer = sub is null
                ? null
                : new APIGatewayCustomAuthorizerContext { Claims = new Dictionary<string, string> { ["sub"] = sub } }
        }
    };

    [Fact]
    public async Task ExecuteAsync_PassesUserIdFromClaimsToAction()
    {
        string? receivedUserId = null;

        var response = await ApiResponse.ExecuteAsync(RequestFor("user-123"), _context, userId =>
        {
            receivedUserId = userId;
            return Task.FromResult(ApiResponse.Ok(new { }));
        });

        Assert.Equal("user-123", receivedUserId);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_NoIdentityAndNotLocal_Returns401WithoutRunningAction()
    {
        var actionRan = false;

        var response = await ApiResponse.ExecuteAsync(RequestFor(null), _context, _ =>
        {
            actionRan = true;
            return Task.FromResult(ApiResponse.NoContent());
        });

        Assert.Equal((int)HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(actionRan);
    }

    [Fact]
    public async Task ExecuteAsync_NoIdentityButLocal_UsesLocalTestUser()
    {
        Environment.SetEnvironmentVariable("IS_LOCAL", "true");
        string? receivedUserId = null;

        await ApiResponse.ExecuteAsync(RequestFor(null), _context, userId =>
        {
            receivedUserId = userId;
            return Task.FromResult(ApiResponse.NoContent());
        });

        Assert.Equal("local-test-user", receivedUserId);
    }

    [Theory]
    [InlineData(typeof(ValidationException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(ConditionalCheckFailedException), HttpStatusCode.Conflict)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_MapsExceptionsToStatusCodes(Type exceptionType, HttpStatusCode expected)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "boom")!;

        var response = await ApiResponse.ExecuteAsync(RequestFor("user-1"), _context,
            _ => throw exception);

        Assert.Equal((int)expected, response.StatusCode);
        using var body = JsonDocument.Parse(response.Body);
        Assert.True(body.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task ExecuteAsync_UnexpectedException_DoesNotLeakDetails()
    {
        var response = await ApiResponse.ExecuteAsync(RequestFor("user-1"), _context,
            _ => throw new InvalidOperationException("secret connection string"));

        Assert.DoesNotContain("secret", response.Body);
    }

    [Fact]
    public void ParseBody_EmptyBody_ThrowsValidationException()
    {
        var request = new APIGatewayProxyRequest { Body = "" };

        Assert.Throws<ValidationException>(() => ApiResponse.ParseBody<Dictionary<string, string>>(request));
    }

    [Fact]
    public void ParseBody_InvalidJson_ThrowsValidationException()
    {
        var request = new APIGatewayProxyRequest { Body = "{not json" };

        Assert.Throws<ValidationException>(() => ApiResponse.ParseBody<Dictionary<string, string>>(request));
    }

    [Fact]
    public void GetGuid_InvalidValue_ThrowsValidationException()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = "not-a-guid" }
        };

        Assert.Throws<ValidationException>(() => ApiResponse.GetGuid(request, "id"));
    }

    [Fact]
    public void ParseDate_InvalidValue_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => ApiResponse.ParseDate("2026-13-45", "date"));
    }
}
