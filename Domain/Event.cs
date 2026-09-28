namespace Horizon.Events.Domain;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartsAt { get; set; }
    public string? ImageUrl { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    public Guid CreatorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}