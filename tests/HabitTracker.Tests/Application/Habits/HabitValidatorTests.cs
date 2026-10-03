using HabitTracker.Application.Common;
using HabitTracker.Application.Habits;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Tests.Application.Habits;

public class HabitValidatorTests
{
    [Fact]
    public void Validate_ValidInput_DoesNotThrow()
    {
        HabitValidator.Validate("Exercise", null, HabitFrequency.Daily, "#6366F1", 7);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_Throws(string? name)
    {
        Assert.Throws<ValidationException>(() =>
          HabitValidator.Validate(name, null, HabitFrequency.Daily, "#6366F1", 7));
    }

    [Fact]
    public void Validate_TooLongName_Throws()
    {
        var name = new string('a', HabitValidator.MaxNameLength + 1);

        Assert.Throws<ValidationException>(() =>
          HabitValidator.Validate(name, null, HabitFrequency.Daily, "#6366F1", 7));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void Validate_TargetDaysOutOfRange_Throws(int target)
    {
        Assert.Throws<ValidationException>(() =>
          HabitValidator.Validate("Exercise", null, HabitFrequency.Daily, "#6366F1", target));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("red")]
    [InlineData("#FFF")]
    [InlineData("6366F1")]
    [InlineData("#GGGGGG")]
    public void Validate_InvalidColor_Throws(string? color)
    {
        Assert.Throws<ValidationException>(() =>
          HabitValidator.Validate("Exercise", null, HabitFrequency.Daily, color, 7));
    }

    [Fact]
    public void Validate_UndefinedFrequency_Throws()
    {
        Assert.Throws<ValidationException>(() =>
          HabitValidator.Validate("Exercise", null, (HabitFrequency)99, "#6366F1", 7));
    }
}
