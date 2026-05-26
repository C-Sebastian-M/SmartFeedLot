using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories
{
    public sealed class CostoOperativoRepository : ICostoOperativoRepository
    {
        private readonly FeedlotDbContext _context;
        public CostoOperativoRepository(FeedlotDbContext context)
        {
            _context = context;
        }

        public void Actualizar(CostoOperativo costo)
        {
            throw new NotImplementedException();
        }

        public Task AgregarAsync(CostoOperativo costo, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CostoOperativo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<CostoOperativo>> ObtenerPorLoteAsync(Guid loteId, DateOnly? desde = null, DateOnly? hasta = null, CategoriaCosto? categoria = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> SumarMontoPorLoteAsync(Guid loteId, DateOnly desde, DateOnly hasta, CategoriaCosto? categoria = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
