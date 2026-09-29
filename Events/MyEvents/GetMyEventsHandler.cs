using Horizon.Events.Common.CQRS;
using Horizon.Events.Domain;

namespace Horizon.Events.Events.MyEvents;

//lista de eventos autenticados
public record GetMyEventsQuery(
    Guid CreatorId,
    EventStatus? Status,
    int Page,
    int PageSize
    ) : IQuery<GetMyEventsResult>;

public record EventSummary(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    string Status,
    Guid CreatorId
);

public record GetMyEventsResult(List<EventSummary> Items, int Page, int PageSize, int TotalCount);

internal class GetMyEventsHandler(EventsDbContext dbContext)
    : IQueryHandler<GetMyEventsQuery, GetMyEventsResult>
{
    public async Task<GetMyEventsResult> Handle(GetMyEventsQuery query, CancellationToken cancellationToken)
    {
        var eventsQuery = dbContext.Events.Where(e => e.CreatorId == query.CreatorId);

        if (query.Status is not null)
            eventsQuery = eventsQuery.Where(e => e.Status == query.Status);

        var totalCount = await eventsQuery.CountAsync(cancellationToken);

        var items = await eventsQuery
            .OrderBy(e => e.StartsAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EventSummary(
                e.Id, e.Title, e.Description, e.StartsAt, e.ImageUrl, e.Status.ToString(), e.CreatorId))
            .ToListAsync(cancellationToken);

        return new GetMyEventsResult(items, query.Page, query.PageSize, totalCount);
    }
}

