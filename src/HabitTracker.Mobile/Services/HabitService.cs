using System.Net.Http.Json;
using HabitTracker.Mobile.Models;

namespace HabitTracker.Mobile.Services;

public class HabitService(HttpClient http) : IHabitService
{
    public async Task<List<Habit>> GetHabitsAsync()
    {
        var result = await http.GetFromJsonAsync<List<Habit>>("/habits");
        return result ?? [];
    }

    public async Task<Habit> CreateHabitAsync(string name, string? description, string color, int targetDaysPerWeek)
    {
        var response = await http.PostAsJsonAsync("/habits", new
        {
            name,
            description,
            frequency = 0,
            color,
            targetDaysPerWeek
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Habit>())!;
    }

    public async Task DeleteHabitAsync(Guid id)
    {
        await http.DeleteAsync($"/habits/{id}");
    }

    public async Task<StreakResult> GetStatsAsync(Guid habitId)
    {
        var result = await http.GetFromJsonAsync<StreakResult>($"/habits/{habitId}/stats");
        return result ?? new StreakResult();
    }

    public async Task CheckInAsync(Guid habitId)
    {
        await http.PostAsJsonAsync("/checkins", new
        {
            habitId,
            date = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            note = (string?)null
        });
    }

    public async Task UpdateHabitAsync(Guid id, string name, string? description, string color, int targetDaysPerWeek)
    {
        var response = await http.PutAsJsonAsync($"/habits/{id}", new
        {
            name,
            description,
            frequency = 0,
            color,
            targetDaysPerWeek
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task<SummaryResult> GetSummaryAsync()
    {
        var result = await http.GetFromJsonAsync<SummaryResult>("/stats/summary");
        return result ?? new SummaryResult();
    }
}
