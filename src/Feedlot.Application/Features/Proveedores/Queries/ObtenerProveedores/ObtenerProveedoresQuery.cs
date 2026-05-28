using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Queries.ObtenerProveedores;

public sealed record ObtenerProveedoresQuery : IRequest<Result<IReadOnlyList<ProveedorDto>>>;
