using AutoMapper;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Entities;

namespace Feedlot.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para el bounded context de Producción.
/// Mapea Aggregates/Entities → DTOs. Nunca al revés desde aquí —
/// la creación de aggregates siempre pasa por los factory methods del dominio.
/// </summary>
public sealed class AnimalMappingProfile : Profile
{
    public AnimalMappingProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .ForMember(d => d.CodigoIdentificacion,
                o => o.MapFrom(s => s.CodigoIdentificacion.Valor))
            .ForMember(d => d.PesoIngresoKg,
                o => o.MapFrom(s => s.PesoIngreso.Kilogramos))
            .ForMember(d => d.PrecioCompra,
                o => o.MapFrom(s => s.PrecioCompra.Monto))
            .ForMember(d => d.Moneda,
                o => o.MapFrom(s => s.PrecioCompra.Moneda))
            .ForMember(d => d.EstadoProductivo,
                o => o.MapFrom(s => s.EstadoProductivo.ToString()))
            .ForMember(d => d.EstadoSanitario,
                o => o.MapFrom(s => s.EstadoSanitario.ToString()))
            .ForMember(d => d.PesoActualKg,
                o => o.MapFrom(s => s.PesoActual.Kilogramos))
            .ForMember(d => d.DiasEnEngorde,
                o => o.MapFrom(s => s.DiasEnEngorde))
            .ForMember(d => d.TotalPesajes,
                o => o.MapFrom(s => s.Pesajes.Count));

        CreateMap<Animal, AnimalResumenDto>()
            .ForMember(d => d.CodigoIdentificacion,
                o => o.MapFrom(s => s.CodigoIdentificacion.Valor))
            .ForMember(d => d.PesoActualKg,
                o => o.MapFrom(s => s.PesoActual.Kilogramos))
            .ForMember(d => d.DiasEnEngorde,
                o => o.MapFrom(s => s.DiasEnEngorde))
            .ForMember(d => d.EstadoProductivo,
                o => o.MapFrom(s => s.EstadoProductivo.ToString()))
            .ForMember(d => d.EstadoSanitario,
                o => o.MapFrom(s => s.EstadoSanitario.ToString()));

        CreateMap<Pesaje, PesajeDto>()
            .ForMember(d => d.PesoKg,
                o => o.MapFrom(s => s.Peso.Kilogramos));

        CreateMap<EventoSanitario, EventoSanitarioDto>()
            .ForMember(d => d.Severidad,
                o => o.MapFrom(s => s.Severidad.ToString()));
    }
}
