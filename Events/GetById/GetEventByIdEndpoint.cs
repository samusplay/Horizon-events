namespace Horizon.Events.Events.GetById;

public record EventDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    string Status,
    Guid CreatorId,
    DateTime CreatedAt
);

public class GetEventByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events/{id:guid}",
            async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetEventByIdQuery(id));
                var response = result.Adapt<EventDetailResponse>();
                return Results.Ok(response);
            })
        .WithName("GetEventById")
        .Produces<EventDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Detalle de un evento")
        .WithDescription("Público. Lo usan la sala en vivo, el catálogo y Flow para conocer el estado y el creador del evento.")
        .WithTags("Events");
    }
}