namespace Feedlot.Application.Features.Analitica.Queries.ObtenerVacunasProximas;

public sealed record VacunasProximasDto(
    Guid AnimalId,
    string CodigoAnimal,
    string? NombreAnimal,
    string Diagnostico,
    DateOnly ProximaDosis,
    string? Responsable);