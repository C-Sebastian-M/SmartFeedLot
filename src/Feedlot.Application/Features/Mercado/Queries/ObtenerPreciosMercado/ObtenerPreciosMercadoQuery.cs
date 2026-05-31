using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Queries.ObtenerPreciosMercado;

public sealed record ObtenerPreciosMercadoQuery : IRequest<Result<IReadOnlyList<PrecioMercadoDto>>>;
