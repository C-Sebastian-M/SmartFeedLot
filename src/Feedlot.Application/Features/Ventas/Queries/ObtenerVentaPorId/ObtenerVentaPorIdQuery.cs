using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Queries.ObtenerVentaPorId;

public sealed record ObtenerVentaPorIdQuery(Guid Id) : IRequest<Result<VentaDto>>;
