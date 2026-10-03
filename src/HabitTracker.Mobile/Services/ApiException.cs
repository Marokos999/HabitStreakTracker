using System.Net;

namespace HabitTracker.Mobile.Services;

// Error from the HabitTracker API. StatusCode is null when the server could not be reached.
public class ApiException(HttpStatusCode? statusCode, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;

    public bool IsNetworkError => StatusCode is null;
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
}
