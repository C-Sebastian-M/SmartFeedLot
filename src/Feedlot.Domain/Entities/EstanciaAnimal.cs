using Feedlot.Domain.Common;

namespace Feedlot.Domain.Entities;

public sealed class EstanciaAnimal : Entity<Guid>
{
    private EstanciaAnimal() { }

    internal EstanciaAnimal(Guid id, Guid potreroId, Guid animalId, DateOnly fechaEntrada)
        : base(id)
    {
        PotreroId = potreroId;
        AnimalId = animalId;
        FechaEntrada = fechaEntrada;
    }

    public Guid PotreroId { get; private set; }
    public Guid AnimalId { get; private set; }
    public DateOnly FechaEntrada { get; private set; }
    public DateOnly? Salida { get; private set; }

    public void RegistrarSalida(DateOnly fechaSalida)
    {
        Salida = fechaSalida;
    }
}
