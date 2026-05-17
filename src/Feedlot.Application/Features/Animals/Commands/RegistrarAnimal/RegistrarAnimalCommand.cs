using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarAnimal;

/// <summary>
/// Command para registrar un nuevo animal en el sistema.
/// 
/// En CQRS, un Command representa una intención de cambiar el estado del sistema.
/// Es inmutable (record) y contiene solo los datos necesarios para ejecutar la operación.
/// MediatR lo enruta automáticamente a su Handler correspondiente.
/// </summary>
public sealed record RegistrarAnimalCommand(
    string CodigoIdentificacion,
    string NumeroArete,
    string Sexo,
    string Raza,
    DateOnly FechaNacimiento,
    decimal PesoIngresoKg,
    decimal PrecioCompra,
    string Moneda,
    DateOnly FechaIngreso,
    Guid? LoteInicialId
) : IRequest<Result<Guid>>;
