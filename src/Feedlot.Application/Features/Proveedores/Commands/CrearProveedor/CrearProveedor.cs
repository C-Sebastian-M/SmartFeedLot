using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Commands.CrearProveedor;

public sealed record CrearProveedorCommand(
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand<Guid>;
public sealed class CrearProveedorCommandValidator : AbstractValidator<CrearProveedorCommand>
{
    public CrearProveedorCommandValidator()
    {
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

public sealed class CrearProveedorCommandHandler
    : IRequestHandler<CrearProveedorCommand, Result<Guid>>
{
    private readonly IProveedorRepository _proveedorRepository;

    public CrearProveedorCommandHandler(IProveedorRepository proveedorRepository)
    {
        _proveedorRepository = proveedorRepository;
    }

    public async Task<Result<Guid>> Handle(CrearProveedorCommand request, CancellationToken ct)
    {
        var proveedor = Proveedor.Crear(request.Nombre, request.Contacto, request.Telefono, request.Email);
        await _proveedorRepository.AgregarAsync(proveedor, ct);
        return Result<Guid>.Success(proveedor.Id);
    }
}
