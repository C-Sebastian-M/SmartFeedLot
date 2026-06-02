using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.ModificarActividad;

public sealed record ModificarActividadCommand(
    Guid ActividadId,
    string Tipo,
    DateOnly Fecha,
    decimal Costo,
    string Moneda
) : ICommand;

public sealed class ModificarActividadCommandValidator : AbstractValidator<ModificarActividadCommand>
{
    public ModificarActividadCommandValidator()
    {
        RuleFor(x => x.ActividadId).NotEmpty();
        RuleFor(x => x.Tipo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class ModificarActividadCommandHandler : IRequestHandler<ModificarActividadCommand, Result>
{
    private readonly IEmpleadoRepository _repo;
    private readonly IUnitOfWork _uow;

    public ModificarActividadCommandHandler(IEmpleadoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(ModificarActividadCommand request, CancellationToken ct)
    {
        // Cargamos la actividad directamente (tracked) — es solo una actualización de propiedades,
        // no hay colección nueva, así que EF detecta los cambios sin problema.
        var actividad = await _repo.ObtenerActividadPorIdAsync(request.ActividadId, ct);
        if (actividad is null)
            return Result.NotFound($"No se encontró la actividad {request.ActividadId}.");

        var costo = Dinero.Crear(request.Costo, request.Moneda);
        actividad.Modificar(request.Tipo, request.Fecha, costo);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
