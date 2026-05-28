using AutoMapper;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Entities;

namespace Feedlot.Application.Mappings;

public sealed class ProveedorCompraMappingProfile : Profile
{
    public ProveedorCompraMappingProfile()
    {
        CreateMap<Proveedor, ProveedorDto>();

        CreateMap<Compra, CompraDto>()
            .ForMember(d => d.NombreProveedor, o => o.Ignore());
    }
}
