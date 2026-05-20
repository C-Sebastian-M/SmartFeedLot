using Feedlot.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Persistence;

/// <summary>
/// Partial class que extiende FeedlotDbContext con los DbSets de Identity.
/// Separar en partial class mantiene el DbContext principal limpio.
/// </summary>
public sealed partial class FeedlotDbContext
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
    public DbSet<ApplicationUserRole> UserRoles => Set<ApplicationUserRole>();
}
