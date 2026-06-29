using Feedlot.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

/// <summary>
/// Gestión de usuarios. Todos los endpoints requieren rol Admin.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/usuarios")]
public sealed class UsuariosController : ApiControllerBase
{
    private readonly AuthService _authService;

    public UsuariosController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Lista todos los usuarios con sus roles y estado.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var usuarios = await _authService.ListarUsuariosAsync(ct);
        return Ok(usuarios);
    }

    /// <summary>Crea un nuevo usuario con el rol indicado.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request, CancellationToken ct)
    {
        var result = await _authService.RegistrarAsync(
            request.Email, request.NombreCompleto, request.Password, request.Rol ?? "Operador", ct);

        if (!result.Succeeded)
            return Conflict(new { error = result.Error });

        return Created(string.Empty, new
        {
            id = result.User!.Id,
            email = result.User.Email,
            nombre = result.User.NombreCompleto,
        });
    }

    /// <summary>Activa o desactiva un usuario.</summary>
    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoUsuarioRequest request, CancellationToken ct)
    {
        var ok = await _authService.CambiarEstadoUsuarioAsync(id, request.Activo, ct);
        return ok ? NoContent() : NotFound(new { error = "Usuario no encontrado." });
    }

    /// <summary>Cambia el rol de un usuario.</summary>
    [HttpPut("{id:guid}/rol")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarRol(Guid id, [FromBody] CambiarRolUsuarioRequest request, CancellationToken ct)
    {
        var ok = await _authService.CambiarRolUsuarioAsync(id, request.Rol, ct);
        return ok ? NoContent() : NotFound(new { error = "Usuario o rol no encontrado." });
    }
}

public sealed record CrearUsuarioRequest(string Email, string NombreCompleto, string Password, string? Rol);
public sealed record CambiarEstadoUsuarioRequest(bool Activo);
public sealed record CambiarRolUsuarioRequest(string Rol);
