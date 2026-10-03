# Creates demo habits and check-ins through the local API (sam local start-api) for screenshots.
# Usage: pwsh scripts/seed-demo-data.ps1 [-BaseUrl http://127.0.0.1:3000]
param([string]$BaseUrl = "http://127.0.0.1:3000")

$today = (Get-Date).Date
function Format-Day([int]$daysAgo) { $today.AddDays(-$daysAgo).ToString("yyyy-MM-dd") }

# daysAgo = 0 means checked in today (shows up under "Done Today")
$habits = @(
    @{ name = "Morning Run"; description = "5 km before work"; color = "#EF4444"; target = 5
       days = (0..13) + (16, 17, 18, 21, 22, 25, 28, 29, 30, 33, 35, 36, 37, 40, 44, 45, 49, 50, 52, 56, 63, 64, 70) },
    @{ name = "Read 20 pages"; description = "Before bed"; color = "#3B82F6"; target = 7
       days = (1..6) + (8, 9, 10, 12, 14, 15, 19, 20, 24, 26, 27, 31, 32, 38, 39, 41, 42, 47, 53, 58, 60) },
    @{ name = "Meditate"; description = "10 minutes"; color = "#8B5CF6"; target = 3
       days = (0, 2, 4, 7, 9, 11, 14, 16, 18, 21, 23, 25, 28, 30, 32, 35, 37, 39, 42, 44, 46, 49, 51, 53, 56, 58) },
    @{ name = "Drink 2L of water"; description = $null; color = "#14B8A6"; target = 7
       days = (1, 2, 3, 5, 6, 8, 12, 13, 17, 22, 29) }
)

foreach ($habit in $habits) {
    $body = @{
        name = $habit.name; description = $habit.description; frequency = 0
        color = $habit.color; targetDaysPerWeek = $habit.target
    } | ConvertTo-Json
    $created = Invoke-RestMethod -Method Post -Uri "$BaseUrl/habits" -ContentType "application/json" -Body $body
    Write-Host "Created habit '$($habit.name)'"

    foreach ($daysAgo in $habit.days) {
        $checkIn = @{ habitId = $created.id; date = (Format-Day $daysAgo) } | ConvertTo-Json
        Invoke-RestMethod -Method Post -Uri "$BaseUrl/checkins" -ContentType "application/json" -Body $checkIn | Out-Null
    }
    Write-Host "  $($habit.days.Count) check-ins"
}

Write-Host "Done. Pull to refresh in the app."
