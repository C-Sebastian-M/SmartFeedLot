namespace Feedlot.Infrastructure.Identity;

/// <summary>
/// Entidad de usuario del sistema. Vive en Infrastructure (no en Domain)
/// porque la autenticación es un Generic Subdomain — podría reemplazarse
/// con un proveedor externo (Auth0, Azure AD) sin afectar el dominio.
///
/// No hereda de AggregateRoot porque su ciclo de vida es independiente
/// del modelo productivo — el dominio nunca referencia a User directamente.
/// </summary>
public sealed class ApplicationUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string NombreCompleto { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool Activo { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime? UltimoAcceso { get; private set; }
    public ICollection<ApplicationUserRole> Roles { get; private set; } = [];

    private ApplicationUser() { }

    public static ApplicationUser Crear(string email, string nombreCompleto, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es requerido.");

        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            NombreCompleto = nombreCompleto.Trim(),
            PasswordHash = passwordHash,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };
    }

    public void RegistrarAcceso() => UltimoAcceso = DateTime.UtcNow;
    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}

public sealed class ApplicationRole
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public ICollection<ApplicationUserRole> Usuarios { get; set; } = [];
}

public sealed class ApplicationUserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}
