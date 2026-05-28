using AutoMapper;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Entities;

namespace Feedlot.Application.Mappings;

public sealed class VentaMappingProfile : Profile
{
    public VentaMappingProfile()
    {
        CreateMap<Comprador, CompradorDto>();

        CreateMap<VentaItem, VentaItemDto>()
            .ForMember(d => d.CodigoAnimal, o => o.Ignore())
            .ForMember(d => d.NombreAnimal, o => o.Ignore());

        CreateMap<Venta, VentaDto>()
            .ForMember(d => d.NombreComprador, o => o.Ignore())
            .ForMember(d => d.TotalAnimales, o => o.MapFrom(s => s.Items.Count));
    }
}
