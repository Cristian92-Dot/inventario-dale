using FluentValidation;
using Inventario.Application.Features.Users.Dtos;

namespace Inventario.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de usuario no puede superar los 100 caracteres.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("El nombre visible es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre visible no puede superar los 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo ingresado no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(role => role is "ADMIN" or "EMPLEADO").WithMessage("El rol seleccionado no es válido.");

        RuleFor(x => x.AvatarPath)
            .MaximumLength(300).WithMessage("La ruta del avatar no puede superar los 300 caracteres.");
    }
}
