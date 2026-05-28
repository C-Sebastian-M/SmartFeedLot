using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Queries.ObtenerProveedorPorId;

public sealed record ObtenerProveedorPorIdQuery(Guid Id) : IRequest<Result<ProveedorDto>>;
