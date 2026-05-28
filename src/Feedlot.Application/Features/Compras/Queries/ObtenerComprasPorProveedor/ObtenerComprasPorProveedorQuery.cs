using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Compras.Queries.ObtenerComprasPorProveedor;

public sealed record ObtenerComprasPorProveedorQuery(Guid ProveedorId) : IRequest<Result<IReadOnlyList<CompraDto>>>;
