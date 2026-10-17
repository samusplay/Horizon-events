using Horizon.Events.Domain;

namespace Horizon.Events.Events.Catalog;

public record CatalogItemResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    string Status,
    Guid CreatorId
);

public record GetCatalogResponse(List<CatalogItemResponse> Items, int Page, int PageSize, int TotalCount);

public class GetCatalogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        //DateTimeOffset: el "Z" de la fecha se interpreta como UTC, que es lo único que acepta Npgsql
        app.MapGet("/api/events",
            async (ISender sender, string? status, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 12) =>
            {
                EventStatus? parsedStatus = null;
                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (!Enum.TryParse<EventStatus>(status, ignoreCase: true, out var s) || !Enum.IsDefined(s))
                        return Results.BadRequest(new { error = $"Estado inválido: {status}" });
                    parsedStatus = s;
                }

                var query = new GetCatalogQuery(parsedStatus, from?.UtcDateTime, to?.UtcDateTime, page, pageSize);
                var result = await sender.Send(query);
                var response = result.Adapt<GetCatalogResponse>();
                return Results.Ok(response);
            })
        .WithName("GetCatalog")
        .Produces<GetCatalogResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Catálogo público de eventos")
        .WithDescription("Público. Sin status trae Scheduled y Live. Filtra por ventana de fechas con from y to (UTC).")
        .WithTags("Events");
    }
}