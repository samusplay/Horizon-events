

namespace Horizon.Events.Events.MyEvents;

//Heredamos abstarcvalidator que es de fluent validation
public class GetMyEventsValidator : AbstractValidator<GetMyEventsQuery>
{
    public GetMyEventsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("El tamaño de página debe estar entre 1 y 50.");
    }
}