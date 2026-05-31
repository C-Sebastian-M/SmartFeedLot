using AutoMapper;
using Feedlot.Application.Features.Mercado.Queries.ObtenerPreciosMercado;
using Feedlot.Domain.Entities;

namespace Feedlot.Application.Mappings;

public sealed class MercadoMappingProfile : Profile
{
    public MercadoMappingProfile()
    {
        CreateMap<PrecioMercado, PrecioMercadoDto>();
    }
}
