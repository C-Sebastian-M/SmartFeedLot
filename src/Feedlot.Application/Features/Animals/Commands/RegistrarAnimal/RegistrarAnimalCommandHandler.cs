using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.Services;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarAnimal;

/// <summary>
/// Handler para RegistrarAnimalCommand.
/// 
/// Responsabilidades del Handler:
/// 1. Traducir el Command a objetos de dominio (Value Objects).
/// 2. Invocar el factory method del Aggregate Root.
/// 3. Persistir usando el repositorio.
/// 4. Si se especificó un lote inicial, coordinar el ingreso vía Domain Service.
/// 5. Retornar el ID del animal creado.
/// 
/// El Handler NO valida reglas de negocio — eso es responsabilidad del Dominio.
/// El Handler NO persiste directamente — el UnitOfWorkBehavior lo hace al final del pipeline.
/// </summary>
public sealed class RegistrarAnimalCommandHandler
    : IRequestHandler<RegistrarAnimalCommand, Result<Guid>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly AnimalLoteService _animalLoteService;

    public RegistrarAnimalCommandHandler(
        IAnimalRepository animalRepository,
        AnimalLoteService animalLoteService)
    {
        _animalRepository = animalRepository;
        _animalLoteService = animalLoteService;
    }

    public async Task<Result<Guid>> Handle(
        RegistrarAnimalCommand request,
        CancellationToken ct)
    {
        var codigoStr = await _animalRepository.ObtenerSiguienteCodigoAsync(ct);
        var codigo = CodigoIdentificacion.Crear(codigoStr);

        var arete = await _animalRepository.ObtenerSiguienteAreteAsync(ct);

        var pesoIngreso = Peso.Crear(request.PesoIngresoKg);
        var precioCompra = Dinero.Crear(
            request.PrecioCompraPorKg * request.PesoIngresoKg, request.Moneda);
        var sexo = Enum.Parse<Sexo>(request.Sexo, ignoreCase: true);

        var animal = Animal.Registrar(
            codigo,
            request.Nombre,
            arete,
            sexo,
            request.Raza,
            request.FechaNacimiento,
            pesoIngreso,
            precioCompra,
            request.FechaIngreso);

        await _animalRepository.AgregarAsync(animal, ct);

        if (request.LoteInicialId.HasValue)
        {
            await _animalLoteService.IngresoInicialAsync(
                animal.Id,
                request.LoteInicialId.Value,
                request.FechaIngreso,
                ct);
        }

        return Result<Guid>.Success(animal.Id);
    }
}
