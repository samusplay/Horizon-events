using Horizon.Events.Common.CQRS;
using Horizon.Events.Domain;

namespace Horizon.Events.Events.Catalog;

// Query: catálogo público de eventos
public record GetCatalogQuery(
    EventStatus? Status,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize
    ) : IQuery<GetCatalogResult>;

public record CatalogItem(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    string Status,
    Guid CreatorId
);

public record GetCatalogResult(List<CatalogItem> Items, int Page, int PageSize, int TotalCount);

internal class GetCatalogHandler(EventsDbContext dbContext)
    : IQueryHandler<GetCatalogQuery, GetCatalogResult>
{
    public async Task<GetCatalogResult> Handle(GetCatalogQuery query, CancellationToken cancellationToken)
    {
        var events = dbContext.Events.AsNoTracking();

        //sin estado: los que no han terminado (Scheduled y Live), como pide HU-06
        events = query.Status is null
            ? events.Where(e => e.Status != EventStatus.Finished)
            : events.Where(e => e.Status == query.Status);

        if (query.From is not null)
            events = events.Where(e => e.StartsAt >= query.From.Value);

        if (query.To is not null)
            events = events.Where(e => e.StartsAt <= query.To.Value);

        var totalCount = await events.CountAsync(cancellationToken);

        var items = await events
            .OrderBy(e => e.StartsAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new CatalogItem(
                e.Id, e.Title, e.Description, e.StartsAt, e.ImageUrl, e.Status.ToString(), e.CreatorId))
            .ToListAsync(cancellationToken);

        return new GetCatalogResult(items, query.Page, query.PageSize, totalCount);
    }
}