using FluentValidation;
using Inventario.Application.Features.Categories.Dtos;

namespace Inventario.Application.Validators;

public class CreateProductCategoryRequestValidator : AbstractValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
            .WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El nombre de la categoría no puede superar los 100 caracteres.");

        RuleFor(x => x.Name)
            .Must(name => name == name.Trim())
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("La categoría no debe iniciar ni terminar con espacios.");

        RuleFor(x => x.Description)
            .MaximumLength(240)
            .WithMessage("La descripción no puede superar los 240 caracteres.");
    }
}
