using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Identity;

public sealed class AuthResult
{
    public bool Succeeded { get; init; }
    public string? Token { get; init; }
    public string? Error { get; init; }
    public ApplicationUser? User { get; init; }

    public static AuthResult Ok(string token, ApplicationUser user)
        => new() { Succeeded = true, Token = token, User = user };

    public static AuthResult Fail(string error)
        => new() { Succeeded = false, Error = error };
}

/// <summary>
/// Servicio de autenticación. Encapsula login, registro y gestión de usuarios.
/// Usa BCrypt para hashing — nunca almacena contraseñas en texto plano.
/// </summary>
public sealed class AuthService
{
    private readonly FeedlotDbContext _context;
    private readonly JwtTokenService _jwtService;

    public AuthService(FeedlotDbContext context, JwtTokenService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResult> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant() && u.Activo, ct);

        if (user is null)
            return AuthResult.Fail("Credenciales inválidas.");

        var passwordValido = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!passwordValido)
            return AuthResult.Fail("Credenciales inválidas.");

        user.RegistrarAcceso();
        await _context.SaveChangesAsync(ct);

        var token = _jwtService.GenerarToken(user);
        return AuthResult.Ok(token, user);
    }

    public async Task<AuthResult> RegistrarAsync(
        string email,
        string nombreCompleto,
        string password,
        string rolNombre = "Operador",
        CancellationToken ct = default)
    {
        var emailExiste = await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

        if (emailExiste)
            return AuthResult.Fail($"Ya existe un usuario con el email '{email}'.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var user = ApplicationUser.Crear(email, nombreCompleto, passwordHash);

        // Asignar rol por defecto.
        var rol = await _context.Roles
            .FirstOrDefaultAsync(r => r.Nombre == rolNombre, ct);

        if (rol is not null)
        {
            user.Roles.Add(new ApplicationUserRole
            {
                UserId = user.Id,
                RoleId = rol.Id,
                Role = rol,
                User = user
            });
        }

        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);

        var token = _jwtService.GenerarToken(user);
        return AuthResult.Ok(token, user);
    }
}
