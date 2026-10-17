namespace Horizon.Events.Events.Catalog;

public class GetCatalogValidator : AbstractValidator<GetCatalogQuery>
{
    public GetCatalogValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("El tamaño de página debe estar entre 1 y 50.");

        RuleFor(x => x)
            .Must(x => x.To >= x.From)
            .When(x => x.From is not null && x.To is not null)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");
    }
}