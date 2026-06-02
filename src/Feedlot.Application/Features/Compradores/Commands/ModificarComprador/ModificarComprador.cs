using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Compradores.Commands.ModificarComprador;

public sealed record ModificarCompradorCommand(
    Guid Id,
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand;
public sealed class ModificarCompradorCommandValidator : AbstractValidator<ModificarCompradorCommand>
{
    public ModificarCompradorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El ID del comprador es requerido.");

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

public sealed class ModificarCompradorCommandHandler
    : IRequestHandler<ModificarCompradorCommand, Result>
{
    private readonly ICompradorRepository _compradorRepository;

    public ModificarCompradorCommandHandler(ICompradorRepository compradorRepository)
    {
        _compradorRepository = compradorRepository;
    }

    public async Task<Result> Handle(ModificarCompradorCommand request, CancellationToken ct)
    {
        var comprador = await _compradorRepository.ObtenerPorIdAsync(request.Id, ct);
        if (comprador is null)
            return Result.NotFound($"Comprador {request.Id} no encontrado.");

        var existe = await _compradorRepository.ExisteConNombreAsync(request.Nombre, request.Id, ct);
        if (existe)
            return Result.Conflict($"Ya existe otro comprador con el nombre '{request.Nombre}'.");

        comprador.Modificar(request.Nombre, request.Contacto, request.Telefono, request.Email);
        _compradorRepository.Actualizar(comprador);
        return Result.Success();
    }
}
