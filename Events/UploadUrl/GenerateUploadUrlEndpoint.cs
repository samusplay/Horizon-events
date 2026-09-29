using System.Security.Claims;

namespace Horizon.Events.Events.UploadUrl;

public record GenerateUploadUrlRequest(string ContentType);
public record GenerateUploadUrlResponse(string UploadUrl, string PublicUrl, int ExpiresInSeconds);

public class GenerateUploadUrlEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events/image-upload-url",
            async (GenerateUploadUrlRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                var sub = user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var creatorId))
                    return Results.Unauthorized();

                var command = request.Adapt<GenerateUploadUrlCommand>() with { CreatorId = creatorId };
                var result = await sender.Send(command);
                var response = result.Adapt<GenerateUploadUrlResponse>();
                return Results.Ok(response);
            })
        .RequireAuthorization("CreatorOnly")
        .WithName("GenerateEventImageUploadUrl")
        .Produces<GenerateUploadUrlResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithSummary("Genera una URL firmada para subir la portada de un evento")
        .WithDescription("Solo un Creador puede pedirla. La imagen se sube directo a S3, sin pasar por este backend.")
        .WithTags("Events");
    }
}