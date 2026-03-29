using FluentValidation;
using Inventario.Application.Features.Sales.Dtos;

namespace Inventario.Application.Validators;

public class RegisterSaleRequestValidator : AbstractValidator<RegisterSaleRequest>
{
    public RegisterSaleRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Debes incluir al menos un producto en la venta.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("Debes seleccionar un producto válido.");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("La cantidad debe ser mayor que cero.");
            });
    }
}
