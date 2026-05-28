using Feedlot.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Login con email y password. Retorna JWT Bearer token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, ct);

        if (!result.Succeeded)
            return Unauthorized(new { error = result.Error });

        return Ok(new
        {
            token = result.Token,
            usuario = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                nombre = result.User.NombreCompleto,
                roles = result.User.Roles.Select(r => r.Role.Nombre).ToList()
            },
            expiraEn = DateTime.UtcNow.AddMinutes(480)
        });
    }

    /// <summary>Registro de nuevo usuario.</summary>
    [HttpPost("registro")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro(
        [FromBody] RegistroRequest request,
        CancellationToken ct = default)
    {
        var result = await _authService.RegistrarAsync(
            request.Email,
            request.NombreCompleto,
            request.Password,
            request.Rol ?? "Operador",
            ct);

        if (!result.Succeeded)
            return Conflict(new { error = result.Error });

        return Created(string.Empty, new
        {
            id = result.User!.Id,
            email = result.User.Email,
            nombre = result.User.NombreCompleto
        });
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record RegistroRequest(
    string Email,
    string NombreCompleto,
    string Password,
    string? Rol);
