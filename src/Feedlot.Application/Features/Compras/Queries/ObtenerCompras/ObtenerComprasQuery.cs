using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Compras.Queries.ObtenerCompras;

public sealed record ObtenerComprasQuery : IRequest<Result<IReadOnlyList<CompraDto>>>;
