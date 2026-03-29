using FluentValidation;
using Inventario.Application.Features.Users.Dtos;

namespace Inventario.Application.Validators;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
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

        RuleFor(x => x.AvatarPath)
            .MaximumLength(300).WithMessage("La ruta del avatar no puede superar los 300 caracteres.");
    }
}
