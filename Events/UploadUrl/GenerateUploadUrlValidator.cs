namespace Horizon.Events.Events.UploadUrl;

public class GenerateUploadUrlValidator : AbstractValidator<GenerateUploadUrlCommand>
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];

    public GenerateUploadUrlValidator()
    {
        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("El tipo de archivo es requerido.")
            .Must(ct => AllowedTypes.Contains(ct))
            .WithMessage("Solo se permiten imágenes JPEG, PNG o WEBP.");
    }
}