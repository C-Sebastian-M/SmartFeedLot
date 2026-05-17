using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Queries.ObtenerLotes;

public sealed record ObtenerLotesQuery(
    bool SoloActivos = false
) : IRequest<Result<IReadOnlyList<LoteResumenDto>>>;
