using Feedlot.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default)
    {
        // LOG explícito para diagnóstico — visible en la terminal del backend
        _logger.LogInformation("Login intento para: {Email}", request.Email);

        try
        {
            var result = await _authService.LoginAsync(request.Email, request.Password, ct);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login fallido para {Email}: {Error}", request.Email, result.Error);
                return Unauthorized(new { error = result.Error });
            }

            _logger.LogInformation("Login exitoso para {Email}", request.Email);

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
        catch (Exception ex)
        {
            // En desarrollo devuelve el error completo al frontend para diagnóstico rápido
            _logger.LogError(ex, "Excepción en Login para {Email}", request.Email);
            return StatusCode(500, new
            {
                error = ex.Message,
                tipo = ex.GetType().Name,
                detalle = ex.InnerException?.Message,
                // Quitar esta línea en producción:
                stackTrace = ex.ToString()
            });
        }
    }

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

    /// <summary>
    /// Endpoint de diagnóstico — verifica que la BD tiene usuarios y roles sembrados.
    /// Eliminar antes de producción.
    /// </summary>
    [HttpGet("diagnostico")]
    public async Task<IActionResult> Diagnostico(
        [FromServices] Feedlot.Infrastructure.Persistence.FeedlotDbContext ctx,
        CancellationToken ct)
    {
        try
        {
            var usuarios = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .CountAsync(ctx.Users, ct);
            var roles = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .CountAsync(ctx.Roles, ct);

            return Ok(new
            {
                conexion = "OK",
                totalUsuarios = usuarios,
                totalRoles = roles,
                mensaje = usuarios == 0
                    ? "⚠️ Sin usuarios — el seed no corrió o falló silenciosamente"
                    : "✅ Usuarios encontrados"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, tipo = ex.GetType().Name });
        }
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record RegistroRequest(string Email, string NombreCompleto, string Password, string? Rol);
