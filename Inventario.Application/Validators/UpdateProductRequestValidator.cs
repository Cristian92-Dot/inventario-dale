using FluentValidation;
using Inventario.Application.Features.Products.Dtos;

namespace Inventario.Application.Validators;

public class UpdateProductRequestValidator : ProductInputValidator<UpdateProductRequest>
{
}
