namespace StudentPortal.ServiceDefaults.Background.Interfaces;

public interface IEventTracker 
{
    Task<bool> IsEventProcessedAsync(Guid eventId);
    Task MarkEventAsProcessedAsync(Guid eventId);
}