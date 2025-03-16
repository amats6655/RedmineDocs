namespace RedmineDocs.Services.Implementation;

public class TrackerService : ITrackerService
{
    public List<Tracker> GetTrackersAsync(List<Project> projects)
    {
        Log.Information("Начинаем обработку трекеров в {Count} проектах", projects.Count);
        var trackers = new List<Tracker>();

        foreach (var project in projects)
        {
            if (project.Trackers.Count > 0)
            {
                foreach (var tracker in project.Trackers)
                {
                    if (trackers.All(t => t.Id != tracker.Id))
                    {
                        trackers.Add(tracker);
                        Log.Debug("Добавлен новый трекер: Id={Id}, Name={Name}", tracker.Id, tracker.Name);
                    }
                }
            }
        }
        
        Log.Information("Обработка трекеров завершена. Всего трекеров: {Count}", trackers.Count);
        return trackers;
    }
}