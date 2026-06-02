using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Commands.ModificarProveedor;

public sealed record ModificarProveedorCommand(
    Guid Id,
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand;
public sealed class ModificarProveedorCommandValidator : AbstractValidator<ModificarProveedorCommand>
{
    public ModificarProveedorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del proveedor es requerido.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del proveedor es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.Contacto)
            .MaximumLength(200).WithMessage("El contacto no puede superar 200 caracteres.")
            .When(x => x.Contacto is not null);

        RuleFor(x => x.Telefono)
            .MaximumLength(50).WithMessage("El teléfono no puede superar 50 caracteres.")
            .When(x => x.Telefono is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(200).WithMessage("El email no puede superar 200 caracteres.")
            .When(x => x.Email is not null);
    }
}

public sealed class ModificarProveedorCommandHandler
    : IRequestHandler<ModificarProveedorCommand, Result>
{
    private readonly IProveedorRepository _proveedorRepository;

    public ModificarProveedorCommandHandler(IProveedorRepository proveedorRepository)
    {
        _proveedorRepository = proveedorRepository;
    }

    public async Task<Result> Handle(ModificarProveedorCommand request, CancellationToken ct)
    {
        var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.Id, ct);
        if (proveedor is null)
            return Result.NotFound($"Proveedor {request.Id} no encontrado.");

        proveedor.Modificar(request.Nombre, request.Contacto, request.Telefono, request.Email);
        _proveedorRepository.Actualizar(proveedor);
        return Result.Success();
    }
}
