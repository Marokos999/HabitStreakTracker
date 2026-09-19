using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Services;

namespace HabitTracker.Application.CheckIns.GetHabitStats;

public class GetHabitStatsHandler(ICheckInRepository repo)
{
  public async Task<GetHabitStatsResult> Handle(GetHabitStatsQuery query)
  {
    var checkIns = await repo.GetByHabitAsync(query.UsrId, query.HabitId);
    var dates = checkIns.Select(c => c.Date);
    var streak = StreakCalculator.Calculate(dates);
    return new GetHabitStatsResult(streak);
  }
}