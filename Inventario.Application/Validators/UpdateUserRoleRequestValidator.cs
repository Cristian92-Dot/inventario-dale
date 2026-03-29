using FluentValidation;
using Inventario.Application.Features.Users.Dtos;

namespace Inventario.Application.Validators;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(role => role is "ADMIN" or "EMPLEADO").WithMessage("El rol seleccionado no es válido.");
    }
}
