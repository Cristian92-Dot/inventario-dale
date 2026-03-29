using FluentValidation;
using Inventario.Application.Features.Products.Dtos;

namespace Inventario.Application.Validators;

public class ProductInputValidator<T> : AbstractValidator<T> where T : IProductInput
{
    public ProductInputValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
            .WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(150);

        RuleFor(x => x.Category)
            .Must(category => !string.IsNullOrWhiteSpace(category?.Trim()))
            .WithMessage("La categoría es obligatoria.")
            .MaximumLength(100);

        RuleFor(x => x.Brand)
            .Must(brand => !string.IsNullOrWhiteSpace(brand?.Trim()))
            .WithMessage("La marca es obligatoria.")
            .MaximumLength(100);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(240)
            .WithMessage("La descripción corta no puede superar los 240 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("La descripción larga no puede superar los 2000 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor que cero.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock actual no puede ser negativo.");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.Name)
            .Must(name => name == name.Trim())
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("El nombre no debe iniciar ni terminar con espacios.");

        RuleFor(x => x.Category)
            .Must(category => category == category.Trim())
            .When(x => !string.IsNullOrWhiteSpace(x.Category))
            .WithMessage("La categoría no debe iniciar ni terminar con espacios.");

        RuleFor(x => x.Brand)
            .Must(brand => brand == brand.Trim())
            .When(x => !string.IsNullOrWhiteSpace(x.Brand))
            .WithMessage("La marca no debe iniciar ni terminar con espacios.");
    }
}
