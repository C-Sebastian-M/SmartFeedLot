using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerVacunasProximas;

public sealed record ObtenerVacunasProximasQuery(int Dias = 15)
    : IRequest<Result<IReadOnlyList<VacunasProximasDto>>>;