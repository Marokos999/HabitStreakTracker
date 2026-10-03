using System.Text.Json;

namespace HabitTracker.Mobile.Services;

// Loaded from Resources/Raw/cognito.json. Values come from the `sam deploy` outputs
// (CognitoDomain, UserPoolClientId); neither is a secret.
public record CognitoSettings(string Domain, string ClientId, string CallbackUrl)
{
    public const string FileName = "cognito.json";
    private const string DefaultCallbackUrl = "habittracker://callback";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Domain) && !Domain.StartsWith("YOUR_", StringComparison.Ordinal) &&
        !string.IsNullOrWhiteSpace(ClientId) && !ClientId.StartsWith("YOUR_", StringComparison.Ordinal);

    public static CognitoSettings Load()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(FileName).GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        return new CognitoSettings(
            root.GetProperty("domain").GetString() ?? "",
            root.GetProperty("clientId").GetString() ?? "",
            root.TryGetProperty("callbackUrl", out var callback)
                ? callback.GetString() ?? DefaultCallbackUrl
                : DefaultCallbackUrl);
    }
}
