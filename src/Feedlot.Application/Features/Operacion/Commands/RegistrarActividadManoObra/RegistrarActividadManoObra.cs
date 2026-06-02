using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarActividadManoObra;

public sealed record RegistrarActividadManoObraCommand(
    Guid EmpleadoId, string Tipo, DateOnly Fecha, decimal Costo, string Moneda) : ICommand<Guid>;
public sealed class RegistrarActividadManoObraCommandValidator : AbstractValidator<RegistrarActividadManoObraCommand>
{
    public RegistrarActividadManoObraCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).NotEmpty();
        RuleFor(x => x.Tipo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class RegistrarActividadManoObraCommandHandler : IRequestHandler<RegistrarActividadManoObraCommand, Result<Guid>>
{
    private readonly IEmpleadoRepository _repo;
    public RegistrarActividadManoObraCommandHandler(IEmpleadoRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(RegistrarActividadManoObraCommand request, CancellationToken ct)
    {
        // AsNoTracking para que el nuevo ActividadManoObra no quede en el grafo tracked
        // y Add() lo marque limpio como EntityState.Added → INSERT
        var empleado = await _repo.ObtenerPorIdSinTrackingAsync(request.EmpleadoId, ct);
        if (empleado is null) return Result<Guid>.NotFound($"Empleado {request.EmpleadoId} no encontrado.");

        var costo = Dinero.Crear(request.Costo, request.Moneda);
        var actividad = empleado.RegistrarActividad(request.Tipo, request.Fecha, costo);
        _repo.AgregarActividad(actividad);
        return Result<Guid>.Success(actividad.Id);
    }
}
