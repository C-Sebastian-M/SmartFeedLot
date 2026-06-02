using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;

public sealed record CrearEmpleadoCommand(string Nombre, decimal PagoMensual, string Moneda) : ICommand<Guid>;
public sealed class CrearEmpleadoCommandValidator : AbstractValidator<CrearEmpleadoCommand>
{
    public CrearEmpleadoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PagoMensual).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class CrearEmpleadoCommandHandler : IRequestHandler<CrearEmpleadoCommand, Result<Guid>>
{
    private readonly IEmpleadoRepository _repo;
    public CrearEmpleadoCommandHandler(IEmpleadoRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearEmpleadoCommand request, CancellationToken ct)
    {
        var pago = Dinero.Crear(request.PagoMensual, request.Moneda);
        var empleado = Empleado.Crear(request.Nombre, pago);
        await _repo.AgregarAsync(empleado, ct);
        return Result<Guid>.Success(empleado.Id);
    }
}
