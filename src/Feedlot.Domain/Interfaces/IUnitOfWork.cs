namespace Feedlot.Domain.Interfaces;

/// <summary>
/// Contrato de Unit of Work. Agrupa todas las operaciones de repositorio
/// en una única transacción de base de datos.
/// 
/// Patrón: Application layer llama SaveChangesAsync() al final del Handler
/// para persistir todos los cambios del aggregate en una sola transacción.
/// Infrastructure lo implementa con EF Core DbContext.SaveChangesAsync().
/// Antes de guardar, el UoW despacha los domain events acumulados.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
