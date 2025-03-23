namespace RedmineDocs.Services.Interfaces;

public interface ITrackerService
{
    List<Tracker> GetTrackersAsync(List<Project> projects);
}