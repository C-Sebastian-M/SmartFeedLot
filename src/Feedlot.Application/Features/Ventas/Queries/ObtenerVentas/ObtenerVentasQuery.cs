using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Queries.ObtenerVentas;

public sealed record ObtenerVentasQuery : IRequest<Result<IReadOnlyList<VentaDto>>>;
