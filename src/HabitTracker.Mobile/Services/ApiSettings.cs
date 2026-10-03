using System.Text.Json;

namespace HabitTracker.Mobile.Services;

// Loaded from Resources/Raw/api.json. Release builds use the `ApiUrl` output of `sam deploy`.
public record ApiSettings(string BaseUrl)
{
    public const string FileName = "api.json";

    public static ApiSettings Load()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(FileName).GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(stream);
        var baseUrl = doc.RootElement.GetProperty("baseUrl").GetString() ?? "";

        if (string.IsNullOrWhiteSpace(baseUrl) || baseUrl.StartsWith("YOUR_", StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"API base URL is not configured. Set baseUrl in Resources/Raw/{FileName}.");

        return new ApiSettings(baseUrl);
    }
}
