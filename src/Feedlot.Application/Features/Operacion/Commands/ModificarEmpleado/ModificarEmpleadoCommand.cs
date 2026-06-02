using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.ModificarEmpleado;

public sealed record ModificarEmpleadoCommand(
    Guid EmpleadoId,
    string Nombre,
    decimal PagoMensual,
    string Moneda
) : ICommand;

public sealed class ModificarEmpleadoCommandValidator : AbstractValidator<ModificarEmpleadoCommand>
{
    public ModificarEmpleadoCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PagoMensual).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class ModificarEmpleadoCommandHandler : IRequestHandler<ModificarEmpleadoCommand, Result>
{
    private readonly IEmpleadoRepository _repo;

    public ModificarEmpleadoCommandHandler(IEmpleadoRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(ModificarEmpleadoCommand request, CancellationToken ct)
    {
        var empleado = await _repo.ObtenerPorIdAsync(request.EmpleadoId, ct);
        if (empleado is null)
            return Result.NotFound($"No se encontró el empleado {request.EmpleadoId}.");

        var pagoMensual = Dinero.Crear(request.PagoMensual, request.Moneda);
        empleado.Modificar(request.Nombre, pagoMensual);
        _repo.Actualizar(empleado);
        return Result.Success();
    }
}
