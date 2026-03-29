using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Users.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Web.Controllers.V1;

/// <summary>
/// Expone la administración de usuarios internos desde la API.
/// </summary>
/// <remarks>
/// Este controlador está reservado para administradores.
/// Permite consultar usuarios, registrar nuevas cuentas, editar datos básicos, cambiar roles y activar o desactivar accesos.
/// </remarks>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOnly")]
public class UsersController : BaseApiController
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    /// <summary>
    /// Devuelve el listado administrativo de usuarios internos.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetAllAsync(cancellationToken);
        return Ok(Success(result));
    }

    /// <summary>
    /// Obtiene el detalle administrativo de un usuario específico.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(Failure(new[] { "No se encontró el usuario solicitado." }, "No se encontró el usuario solicitado."));
        }

        return Ok(Success(result));
    }

    /// <summary>
    /// Registra un nuevo usuario interno y le asigna un rol inicial.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.CreateAsync(request, cancellationToken);
        return Ok(Success(result, "Usuario creado correctamente."));
    }

    /// <summary>
    /// Actualiza la información principal de un usuario existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UpdateProfileAsync(id, request, cancellationToken);
        return Ok(Success(result, "Usuario actualizado correctamente."));
    }

    /// <summary>
    /// Actualiza el rol principal de un usuario existente.
    /// </summary>
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UpdateRoleAsync(id, request, cancellationToken);
        return Ok(Success(result, "Rol actualizado correctamente."));
    }

    /// <summary>
    /// Activa o desactiva la cuenta de un usuario.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(string id, CancellationToken cancellationToken)
    {
        await _userManagementService.ToggleStatusAsync(id, cancellationToken);
        return Ok(Success(new { }, "Estado del usuario actualizado correctamente."));
    }
}
