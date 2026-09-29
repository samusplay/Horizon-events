using Horizon.Events.Domain;
using Horizon.Events.Events.MyEvents;
using System.Security.Claims;


public record EventSummaryResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    string Status,
    Guid CreatorId
);

public record GetMyEventsResponse(List<EventSummaryResponse> Items, int Page, int PageSize, int TotalCount);

public class GetMyEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events/mine",
            async (ClaimsPrincipal user, ISender sender, string? status, int page = 1, int pageSize = 12) =>
            {
                var sub = user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var creatorId))
                    return Results.Unauthorized();

                EventStatus? parsedStatus = null;
                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (!Enum.TryParse<EventStatus>(status, ignoreCase: true, out var s))
                        return Results.BadRequest(new { error = $"Estado inválido: {status}" });
                    parsedStatus = s;
                }

                var query = new GetMyEventsQuery(creatorId, parsedStatus, page, pageSize);
                var result = await sender.Send(query);
                var response = result.Adapt<GetMyEventsResponse>();
                return Results.Ok(response);
            })
        .RequireAuthorization("CreatorOnly")
        .WithName("GetMyEvents")
        .Produces<GetMyEventsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithSummary("Lista los eventos del creador autenticado")
        .WithDescription("Incluye eventos en cualquier estado, incluido Finished, para el panel del creador.")
        .WithTags("Events");
    }
}