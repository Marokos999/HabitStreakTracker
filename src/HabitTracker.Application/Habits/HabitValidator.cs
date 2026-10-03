using System.Text.RegularExpressions;
using HabitTracker.Application.Common;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits;

public static partial class HabitValidator
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;

    public static void Validate(string? name, string? description, HabitFrequency frequency, string? color,
                                int targetDaysPerWeek)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");
        if (name.Length > MaxNameLength)
            throw new ValidationException($"Name must be at most {MaxNameLength} characters.");
        if (description is { Length: > MaxDescriptionLength })
            throw new ValidationException($"Description must be at most {MaxDescriptionLength} characters.");
        if (!Enum.IsDefined(frequency))
            throw new ValidationException("Frequency is invalid.");
        if (color is null || !HexColor().IsMatch(color))
            throw new ValidationException("Color must be a hex value like #6366F1.");
        if (targetDaysPerWeek is < 1 or > 7)
            throw new ValidationException("TargetDaysPerWeek must be between 1 and 7.");
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();
}
