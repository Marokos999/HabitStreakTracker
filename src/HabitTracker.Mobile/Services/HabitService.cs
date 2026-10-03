using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HabitTracker.Mobile.Models;
using Polly;

namespace HabitTracker.Mobile.Services;

public class HabitService(HttpClient http) : IHabitService
{
    public async Task<List<Habit>> GetHabitsAsync()
    {
        var result = await SendAsync<List<Habit>>(() => http.GetAsync($"/habits?date={Today()}"));
        return result ?? [];
    }

    public async Task<Habit> CreateHabitAsync(string name, string? description, string color, int targetDaysPerWeek)
    {
        var habit = await SendAsync<Habit>(() => http.PostAsJsonAsync("/habits", new
        {
            name,
            description,
            frequency = 0,
            color,
            targetDaysPerWeek
        }));
        return habit ?? throw new ApiException(HttpStatusCode.InternalServerError, "The server returned an empty response.");
    }

    public async Task DeleteHabitAsync(Guid id) =>
        await SendAsync(() => http.DeleteAsync($"/habits/{id}"));

    public async Task<StreakResult> GetStatsAsync(Guid habitId)
    {
        var result = await SendAsync<StreakResult>(() => http.GetAsync($"/habits/{habitId}/stats"));
        return result ?? new StreakResult();
    }

    public async Task CheckInAsync(Guid habitId) =>
        await SendAsync(() => http.PostAsJsonAsync("/checkins", new
        {
            habitId,
            date = Today(),
            note = (string?)null
        }));

    public async Task UpdateHabitAsync(Guid id, string name, string? description, string color, int targetDaysPerWeek) =>
        await SendAsync(() => http.PutAsJsonAsync($"/habits/{id}", new
        {
            name,
            description,
            frequency = 0,
            color,
            targetDaysPerWeek
        }));

    public async Task<SummaryResult> GetSummaryAsync()
    {
        var result = await SendAsync<SummaryResult>(() => http.GetAsync("/stats/summary"));
        return result ?? new SummaryResult();
    }

    // Device-local date, formatted independently of the device culture
    private static string Today() =>
        DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        using var response = await ExecuteAsync(send);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        using var response = await ExecuteAsync(send);
    }

    // Sends the request and converts failures into ApiException with a user-friendly message.
    private static async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> send)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ExecutionRejectedException)
        {
            throw new ApiException(null, "Could not reach the server. Check your connection and try again.", ex);
        }

        if (response.IsSuccessStatusCode)
            return response;

        var message = await ReadErrorMessageAsync(response);
        var status = response.StatusCode;
        response.Dispose();
        throw new ApiException(status, message);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return "Your session has expired. Please sign in again.";
        if ((int)response.StatusCode >= 500)
            return "Server error. Please try again later.";

        try
        {
            var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
            if (!string.IsNullOrWhiteSpace(body?.Error))
                return body.Error;
        }
        catch (JsonException)
        {
            // Fall through to the generic message.
        }

        return response.StatusCode == HttpStatusCode.NotFound
            ? "The requested item was not found."
            : "The request was rejected by the server.";
    }

    private record ErrorBody(string? Error);
}
