using AutoMapper;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Entities;

namespace Feedlot.Application.Mappings;

public sealed class LoteMappingProfile : Profile
{
    public LoteMappingProfile()
    {
        CreateMap<Lote, LoteDto>()
            .ForMember(d => d.CapacidadMaxima,
                o => o.MapFrom(s => s.Capacidad.Maxima))
            .ForMember(d => d.AnimalesActuales,
                o => o.MapFrom(s => s.Capacidad.Actual))
            .ForMember(d => d.PorcentajeOcupacion,
                o => o.MapFrom(s => s.Capacidad.PorcentajeOcupacion))
            .ForMember(d => d.Estado,
                o => o.MapFrom(s => s.Estado.ToString()))
            .ForMember(d => d.Animales,
                o => o.MapFrom(s => s.AnimalesLote));

        CreateMap<Lote, LoteResumenDto>()
            .ForMember(d => d.CapacidadMaxima,
                o => o.MapFrom(s => s.Capacidad.Maxima))
            .ForMember(d => d.AnimalesActuales,
                o => o.MapFrom(s => s.Capacidad.Actual))
            .ForMember(d => d.PorcentajeOcupacion,
                o => o.MapFrom(s => s.Capacidad.PorcentajeOcupacion))
            .ForMember(d => d.Estado,
                o => o.MapFrom(s => s.Estado.ToString()));

        CreateMap<AnimalLote, AnimalLoteDto>()
            .ForMember(d => d.CodigoAnimal,
                o => o.Ignore()) // Se hidrata en el Handler con datos del repositorio
            .ForMember(d => d.MotivoIngreso,
                o => o.MapFrom(s => s.MotivoIngreso.ToString()))
            .ForMember(d => d.DiasEnLote,
                o => o.MapFrom(s => s.DiasEnLote));

        CreateMap<Racion, RacionDto>()
            .ForMember(d => d.CostoKg,
                o => o.MapFrom(s => s.CostoKg.Monto));

        CreateMap<ConsumoAlimenticio, ConsumoAlimenticioDto>()
            .ForMember(d => d.CantidadKg,
                o => o.MapFrom(s => s.CantidadKg.Valor))
            .ForMember(d => d.CostoTotal,
                o => o.MapFrom(s => s.CostoTotal.Monto))
            .ForMember(d => d.Moneda,
                o => o.MapFrom(s => s.CostoTotal.Moneda))
            .ForMember(d => d.NombreRacion,
                o => o.Ignore()); // Se hidrata en el Handler
    }
}
