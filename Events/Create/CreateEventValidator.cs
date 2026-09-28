namespace Horizon.Events.Events.Create;

// Reglas para validar los datos de entrada del evento
public class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido.")
            .MaximumLength(150).WithMessage("El título no puede superar 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres.");

        // Npgsql solo acepta fechas UTC en columnas timestamptz
        RuleFor(x => x.StartsAt)
            .Must(d => d.Kind == DateTimeKind.Utc)
            .WithMessage("La fecha de inicio debe enviarse en UTC (ej. 2026-10-05T20:00:00Z).")
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("La fecha de inicio debe ser futura.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("La URL de la imagen no puede superar 500 caracteres.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("La URL de la imagen no es válida.");

        // Defensa extra: el endpoint siempre debe llenarlo con el claim sub
        RuleFor(x => x.CreatorId)
            .NotEmpty().WithMessage("No se pudo identificar al creador.");
    }
}