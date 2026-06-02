using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Compradores.Commands.CrearComprador;

public sealed record CrearCompradorCommand(
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand<Guid>;
public sealed class CrearCompradorCommandValidator : AbstractValidator<CrearCompradorCommand>
{
    public CrearCompradorCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del comprador es requerido.")
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

public sealed class CrearCompradorCommandHandler
    : IRequestHandler<CrearCompradorCommand, Result<Guid>>
{
    private readonly ICompradorRepository _compradorRepository;

    public CrearCompradorCommandHandler(ICompradorRepository compradorRepository)
    {
        _compradorRepository = compradorRepository;
    }

    public async Task<Result<Guid>> Handle(CrearCompradorCommand request, CancellationToken ct)
    {
        var existe = await _compradorRepository.ExisteConNombreAsync(request.Nombre, null, ct);
        if (existe)
            return Result<Guid>.Conflict($"Ya existe un comprador con el nombre '{request.Nombre}'.");

        var comprador = Comprador.Crear(request.Nombre, request.Contacto, request.Telefono, request.Email);
        await _compradorRepository.AgregarAsync(comprador, ct);
        return Result<Guid>.Success(comprador.Id);
    }
}
