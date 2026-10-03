namespace HabitTracker.Mobile.Controls;

// GitHub-style heatmap: one column per week (Monday to Sunday), the last column is the current week.
public class StreakHeatmap : ContentView
{
    public const int Weeks = 12;
    private const double CellSize = 20;
    private const double CellSpacing = 4;

    private static readonly Color EmptyLight = Color.FromArgb("#E2E8F0");
    private static readonly Color EmptyDark = Color.FromArgb("#334155");

    public static readonly BindableProperty DatesProperty = BindableProperty.Create(
        nameof(Dates), typeof(IEnumerable<DateOnly>), typeof(StreakHeatmap),
        propertyChanged: (b, _, _) => ((StreakHeatmap)b).Render());

    public static readonly BindableProperty CellColorProperty = BindableProperty.Create(
        nameof(CellColor), typeof(Color), typeof(StreakHeatmap), Color.FromArgb("#6366F1"),
        propertyChanged: (b, _, _) => ((StreakHeatmap)b).Render());

    public IEnumerable<DateOnly>? Dates
    {
        get => (IEnumerable<DateOnly>?)GetValue(DatesProperty);
        set => SetValue(DatesProperty, value);
    }

    public Color CellColor
    {
        get => (Color)GetValue(CellColorProperty);
        set => SetValue(CellColorProperty, value);
    }

    public StreakHeatmap()
    {
        HorizontalOptions = LayoutOptions.Center;
        Render();
    }

    private void Render()
    {
        var checkedIn = Dates?.ToHashSet() ?? [];
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Monday of the current week; DayOfWeek.Sunday is 0, so shift it to the end of the week.
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var firstDay = today.AddDays(-daysSinceMonday - (Weeks - 1) * 7);

        var grid = new Grid { RowSpacing = CellSpacing, ColumnSpacing = CellSpacing };
        for (var i = 0; i < 7; i++)
            grid.RowDefinitions.Add(new RowDefinition(CellSize));
        for (var i = 0; i < Weeks; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(CellSize));

        for (var week = 0; week < Weeks; week++)
        {
            for (var day = 0; day < 7; day++)
            {
                var date = firstDay.AddDays(week * 7 + day);
                if (date > today)
                    continue; // future days of the current week stay empty

                var cell = new BoxView { CornerRadius = 4 };
                if (checkedIn.Contains(date))
                    cell.Color = CellColor;
                else
                    cell.SetAppThemeColor(BoxView.ColorProperty, EmptyLight, EmptyDark);

                SemanticProperties.SetDescription(cell,
                    $"{date:ddd d MMM}: {(checkedIn.Contains(date) ? "checked in" : "no check-in")}");

                grid.Add(cell, week, day);
            }
        }

        Content = grid;
    }
}
