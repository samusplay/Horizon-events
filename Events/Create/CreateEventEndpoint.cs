using System.Security.Claims;

namespace Horizon.Events.Events.Create;

// Lo que envía el cliente. No incluye CreatorId: sale del token
public record CreateEventRequest(
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl
);

// Lo que devolvemos al cliente
public record CreateEventResponse(Guid Id);

// Carter reúne los endpoints
public class CreateEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events",
            async (CreateEventRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                // El creador es el dueño del token (claim "sub"), nunca lo manda el cliente
                var sub = user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var creatorId))
                    return Results.Unauthorized();

                // Mapeo Request -> Command usando Mapster, y completamos el CreatorId
                var command = request.Adapt<CreateEventCommand>() with { CreatorId = creatorId };

                // MediatR manda el comando al Handler (pasando primero por ValidationBehavior)
                var result = await sender.Send(command);

                // Mapeo Result -> Response usando Mapster
                var response = result.Adapt<CreateEventResponse>();

                // 201 Created con la ubicación del recurso creado
                return Results.Created($"/api/events/{response.Id}", response);
            })
        .RequireAuthorization("CreatorOnly")
        .WithName("CreateEvent")
        .Produces<CreateEventResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithSummary("Crea un nuevo evento")
        .WithDescription("Solo un usuario con rol Creador puede crear eventos. Nacen en estado Programado.")
        .WithTags("Events");
    }
}