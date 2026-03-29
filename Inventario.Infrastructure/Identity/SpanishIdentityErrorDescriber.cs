using Microsoft.AspNetCore.Identity;

namespace Inventario.Infrastructure.Identity;

public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Create(nameof(DefaultError), "Se produjo un error inesperado al procesar la solicitud de identidad.");
    public override IdentityError ConcurrencyFailure() => Create(nameof(ConcurrencyFailure), "La operación fue cancelada porque la información fue modificada por otro proceso. Intenta nuevamente.");
    public override IdentityError PasswordMismatch() => Create(nameof(PasswordMismatch), "La contraseña actual no es válida.");
    public override IdentityError InvalidToken() => Create(nameof(InvalidToken), "El token proporcionado no es válido o ya no se encuentra vigente.");
    public override IdentityError LoginAlreadyAssociated() => Create(nameof(LoginAlreadyAssociated), "La cuenta externa ya está asociada a otro usuario.");
    public override IdentityError InvalidUserName(string? userName) => Create(nameof(InvalidUserName), $"El nombre de usuario '{userName}' no es válido.");
    public override IdentityError InvalidEmail(string? email) => Create(nameof(InvalidEmail), $"El correo electrónico '{email}' no es válido.");
    public override IdentityError DuplicateUserName(string userName) => Create(nameof(DuplicateUserName), $"El nombre de usuario '{userName}' ya se encuentra registrado.");
    public override IdentityError DuplicateEmail(string email) => Create(nameof(DuplicateEmail), $"El correo electrónico '{email}' ya se encuentra registrado.");
    public override IdentityError InvalidRoleName(string? role) => Create(nameof(InvalidRoleName), $"El rol '{role}' no es válido.");
    public override IdentityError DuplicateRoleName(string role) => Create(nameof(DuplicateRoleName), $"El rol '{role}' ya existe.");
    public override IdentityError UserAlreadyHasPassword() => Create(nameof(UserAlreadyHasPassword), "El usuario ya tiene una contraseña configurada.");
    public override IdentityError UserLockoutNotEnabled() => Create(nameof(UserLockoutNotEnabled), "La cuenta no tiene habilitado el bloqueo por seguridad.");
    public override IdentityError UserAlreadyInRole(string role) => Create(nameof(UserAlreadyInRole), $"El usuario ya pertenece al rol '{role}'.");
    public override IdentityError UserNotInRole(string role) => Create(nameof(UserNotInRole), $"El usuario no pertenece al rol '{role}'.");
    public override IdentityError PasswordTooShort(int length) => Create(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Create(nameof(PasswordRequiresUniqueChars), $"La contraseña debe contener al menos {uniqueChars} caracteres diferentes.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Create(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe incluir al menos un carácter especial.");
    public override IdentityError PasswordRequiresDigit() => Create(nameof(PasswordRequiresDigit), "La contraseña debe incluir al menos un número.");
    public override IdentityError PasswordRequiresLower() => Create(nameof(PasswordRequiresLower), "La contraseña debe incluir al menos una letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => Create(nameof(PasswordRequiresUpper), "La contraseña debe incluir al menos una letra mayúscula.");
    public override IdentityError RecoveryCodeRedemptionFailed() => Create(nameof(RecoveryCodeRedemptionFailed), "El código de recuperación no es válido.");

    private static IdentityError Create(string code, string description) => new()
    {
        Code = code,
        Description = description
    };
}
