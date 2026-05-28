using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Queries.ObtenerCompradores;

public sealed record ObtenerCompradoresQuery : IRequest<Result<IReadOnlyList<CompradorDto>>>;
