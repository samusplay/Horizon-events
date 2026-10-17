using Horizon.Events.Common.CQRS;
using Horizon.Events.Common.Exceptions;

namespace Horizon.Events.Events.GetById;

// Query: consulta, no modifica datos
public record GetEventByIdQuery(Guid Id) : IQuery<GetEventByIdResult>;

public record GetEventByIdResult(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    string Status,
    Guid CreatorId,
    DateTime CreatedAt
);

internal class GetEventByIdHandler(EventsDbContext dbContext)
    : IQueryHandler<GetEventByIdQuery, GetEventByIdResult>
{
    public async Task<GetEventByIdResult> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var e = await dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException($"El evento {query.Id} no existe");

        return new GetEventByIdResult(
            e.Id, e.Title, e.Description, e.StartsAt, e.ImageUrl, e.Status.ToString(), e.CreatorId, e.CreatedAt);
    }
}