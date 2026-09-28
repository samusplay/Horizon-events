using Horizon.Events.Common.CQRS;
using Horizon.Events.Domain;

namespace Horizon.Events.Events.Create;

//Record para modifcar los datos
public record CreateEventCommand(
    string Title,
    string? Description,
    DateTime StartsAt,
    string? ImageUrl,
    Guid CreatorId
    ) : ICommand<CreateEventResult>;
//el resultado que vamos devolver
public record CreateEventResult(Guid Id);

internal class CreateEventHandler 
    //inyectamos dependencias para hablar con la base de datos
    (EventsDbContext dbContext)
    : ICommandHandler<CreateEventCommand, CreateEventResult>
{
    public  async Task<CreateEventResult> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        //Logica de Crear un evento
        var newEvent = new Event
        {
            Title = command.Title,
            Description = command.Description,
            StartsAt = command.StartsAt,
            ImageUrl = command.ImageUrl,
            Status = EventStatus.Scheduled,
            CreatorId = command.CreatorId
        };
        dbContext.Events.Add(newEvent);
        //esperamos un llamada inmediata 
        await dbContext.SaveChangesAsync(cancellationToken);

        //devolvemos lo que prometimos a la firma
        return new CreateEventResult(newEvent.Id);
    }
}

