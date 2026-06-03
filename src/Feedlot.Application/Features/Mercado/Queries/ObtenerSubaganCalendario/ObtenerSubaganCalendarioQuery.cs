using Feedlot.Application.Common;
using Feedlot.Application.Services;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Queries.ObtenerSubaganCalendario;

/// <summary>
/// Lista los eventos del calendario de SUBAGAN para que el usuario elija cuál importar,
/// sin tener que ir a la web de SUBAGAN.
///
/// Filtros opcionales:
/// - Sede: subcadena que debe contener el título (p. ej. "PLANETA RICA"). Si es null, todas.
/// - SoloPasadas: si es true, oculta las subastas futuras (que aún no tienen lotes).
/// </summary>
public sealed record ObtenerSubaganCalendarioQuery(
    string? Sede = null,
    bool SoloPasadas = false) : IRequest<Result<IReadOnlyList<SubaganCalendarioDto>>>;

public sealed record SubaganCalendarioDto(
    int EventId,
    string Titulo,
    DateOnly Fecha,
    bool EsPasada,
    bool YaImportado);

public sealed class ObtenerSubaganCalendarioQueryHandler
    : IRequestHandler<ObtenerSubaganCalendarioQuery, Result<IReadOnlyList<SubaganCalendarioDto>>>
{
    private readonly ISubaganHttpService _subagan;
    private readonly ISubaganEventoRepository _repo;

    public ObtenerSubaganCalendarioQueryHandler(
        ISubaganHttpService subagan,
        ISubaganEventoRepository repo)
    {
        _subagan = subagan;
        _repo = repo;
    }

    public async Task<Result<IReadOnlyList<SubaganCalendarioDto>>> Handle(
        ObtenerSubaganCalendarioQuery request, CancellationToken ct)
    {
        var loginOk = await _subagan.LoginAsync(ct);
        if (!loginOk)
            return Result<IReadOnlyList<SubaganCalendarioDto>>.Failure(
                "No se pudo autenticar en SUBAGAN. Verifica las credenciales en la configuración.",
                ResultErrorType.BusinessRule);

        var eventos = await _subagan.ObtenerEventosCalendarioAsync(ct);

        // IDs ya importados, para marcarlos en la lista.
        var importados = (await _repo.ObtenerTodosAsync(ct))
            .Select(e => e.SubaganEventoId)
            .ToHashSet();

        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var query = eventos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Sede))
            query = query.Where(e =>
                e.Titulo.Contains(request.Sede, StringComparison.OrdinalIgnoreCase));

        if (request.SoloPasadas)
            query = query.Where(e => e.Fecha <= hoy);

        var dtos = query
            .OrderByDescending(e => e.Fecha)
            .Select(e => new SubaganCalendarioDto(
                e.EventId,
                e.Titulo,
                e.Fecha,
                EsPasada: e.Fecha <= hoy,
                YaImportado: importados.Contains(e.EventId)))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<SubaganCalendarioDto>>.Success(dtos);
    }
}
