using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Animals.Queries.ObtenerAnimalPorId;

public sealed record ObtenerAnimalPorIdQuery(Guid AnimalId)
    : IRequest<Result<AnimalDto>>;
