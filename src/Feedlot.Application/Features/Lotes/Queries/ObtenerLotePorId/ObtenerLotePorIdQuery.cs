using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Queries.ObtenerLotePorId;

public sealed record ObtenerLotePorIdQuery(Guid LoteId)
    : IRequest<Result<LoteDto>>;
