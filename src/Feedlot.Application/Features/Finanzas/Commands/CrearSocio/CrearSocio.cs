using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearSocio;

public sealed record CrearSocioCommand(
    string Nombre,
    decimal Participacion
) : ICommand<Guid>;
public sealed class CrearSocioCommandValidator
    : AbstractValidator<CrearSocioCommand>
{
    public CrearSocioCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del socio es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Participacion)
            .InclusiveBetween(0, 100).WithMessage("La participación debe ser un porcentaje entre 0 y 100.");
    }
}

public sealed class CrearSocioCommandHandler
    : IRequestHandler<CrearSocioCommand, Result<Guid>>
{
    private readonly ISocioRepository _socioRepo;

    public CrearSocioCommandHandler(
        ISocioRepository socioRepo)
    {
        _socioRepo = socioRepo;
    }

    public async Task<Result<Guid>> Handle(
        CrearSocioCommand request,
        CancellationToken ct)
    {
        var socio = Socio.Crear(request.Nombre, request.Participacion);

        await _socioRepo.AgregarAsync(socio, ct);

        return Result<Guid>.Success(socio.Id);
    }
}
