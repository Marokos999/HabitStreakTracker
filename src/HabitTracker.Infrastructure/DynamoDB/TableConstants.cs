namespace HabitTracker.Infrastructure.DynamoDB;

public static class TableConstants
{
    public static readonly string TableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "HabitTrackerTable";

    public const string UserPrefix = "USER#";
    public const string HabitPrefix = "HABIT#";
    public const string CheckInPrefix = "CHECKIN#";
    public const string DatePrefix = "DATE#";
    public const string ProfileSk = "PROFILE";
    public const string Gsi1IndexName = "GSI1";
}
