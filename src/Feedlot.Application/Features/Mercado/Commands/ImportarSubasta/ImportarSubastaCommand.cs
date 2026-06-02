using Feedlot.Application.Common;
using Feedlot.Application.Services;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.ImportarSubasta;

public sealed record ImportarSubastaCommand(int EventId, int? NumeroSubasta = null) : ICommand<ImportarSubastaResult>;

public sealed record ImportarSubastaResult(Guid EventoId, int TotalLotes, DateOnly Fecha, bool YaExistia);

public sealed class ImportarSubastaCommandValidator : AbstractValidator<ImportarSubastaCommand>
{
    public ImportarSubastaCommandValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0).WithMessage("El EventId de SUBAGAN debe ser mayor a cero.");
    }
}

public sealed class ImportarSubastaCommandHandler
    : IRequestHandler<ImportarSubastaCommand, Result<ImportarSubastaResult>>
{
    private readonly ISubaganHttpService _subagan;
    private readonly ISubaganEventoRepository _repo;
    private readonly IUnitOfWork _uow;

    public ImportarSubastaCommandHandler(
        ISubaganHttpService subagan,
        ISubaganEventoRepository repo,
        IUnitOfWork uow)
    {
        _subagan = subagan;
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<ImportarSubastaResult>> Handle(
        ImportarSubastaCommand request, CancellationToken ct)
    {
        // Si ya fue importado, devolver existente
        if (await _repo.ExisteAsync(request.EventId, ct))
        {
            var existente = (await _repo.ObtenerTodosAsync(ct))
                .First(e => e.SubaganEventoId == request.EventId);
            return Result<ImportarSubastaResult>.Success(
                new ImportarSubastaResult(existente.Id, existente.Lotes.Count, existente.Fecha, true));
        }

        // Login en SUBAGAN
        var loginOk = await _subagan.LoginAsync(ct);
        if (!loginOk)
            return Result<ImportarSubastaResult>.Failure(
                "No se pudo autenticar en SUBAGAN. Verifica las credenciales en la configuración.",
                ResultErrorType.BusinessRule);

        // Obtener lotes
        var lotes = await _subagan.ObtenerLotesAsync(request.EventId, ct);
        if (lotes.Count == 0)
            return Result<ImportarSubastaResult>.Failure(
                $"No se encontraron lotes para el evento {request.EventId}. Verifica que el evento exista y haya finalizado.",
                ResultErrorType.NotFound);

        // Inferir fecha del primer lote (la más frecuente)
        var fecha = lotes
            .Select(l => l.Fecha).Where(f => f != DateOnly.MinValue)
            .GroupBy(f => f).OrderByDescending(g => g.Count())
            .First().Key;

        // Crear aggregate con todos sus lotes
        var evento = SubaganEvento.Crear(request.EventId, request.NumeroSubasta, fecha, "PLANETA RICA");
        foreach (var l in lotes)
        {
            evento.AgregarLote(
                l.LoteId, l.NumeroLote, l.CodigoTipo, l.DescripcionTipo,
                l.Cantidad, l.PesoTotal, l.PesoProm, l.PrecioPorKg,
                l.Procedencia, l.Observaciones);
        }

        await _repo.AgregarAsync(evento, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<ImportarSubastaResult>.Success(
            new ImportarSubastaResult(evento.Id, lotes.Count, fecha, false));
    }
}
