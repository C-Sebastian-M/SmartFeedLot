using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Queries.ObtenerVentas;

public sealed class ObtenerVentasQueryHandler
    : IRequestHandler<ObtenerVentasQuery, Result<IReadOnlyList<VentaDto>>>
{
    private readonly IVentaRepository _ventaRepository;
    private readonly ICompradorRepository _compradorRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IMapper _mapper;

    public ObtenerVentasQueryHandler(
        IVentaRepository ventaRepository,
        ICompradorRepository compradorRepository,
        IAnimalRepository animalRepository,
        IMapper mapper)
    {
        _ventaRepository = ventaRepository;
        _compradorRepository = compradorRepository;
        _animalRepository = animalRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<VentaDto>>> Handle(
        ObtenerVentasQuery request, CancellationToken ct)
    {
        var ventas = await _ventaRepository.ObtenerTodosAsync(ct);
        var compradorIds = ventas.Select(v => v.CompradorId).Distinct().ToList();
        var animalIds = ventas.SelectMany(v => v.Items.Select(i => i.AnimalId)).Distinct().ToList();

        var compradores = new Dictionary<Guid, string>();
        foreach (var cid in compradorIds)
        {
            var c = await _compradorRepository.ObtenerPorIdAsync(cid, ct);
            if (c is not null) compradores[cid] = c.Nombre;
        }

        var animales = await _animalRepository.ObtenerCodigosPorIdsAsync(animalIds, ct);

        var dtos = _mapper.Map<List<VentaDto>>(ventas);
        foreach (var dto in dtos)
        {
            if (compradores.TryGetValue(dto.CompradorId, out var nombre))
                dto.NombreComprador = nombre;
            dto.MontoTotal = ventas.First(v => v.Id == dto.Id).MontoTotal;

            foreach (var item in dto.Items)
            {
                if (animales.TryGetValue(item.AnimalId, out var info))
                {
                    item.CodigoAnimal = info.Codigo;
                    item.NombreAnimal = info.Nombre;
                }
            }
        }

        return Result<IReadOnlyList<VentaDto>>.Success(dtos);
    }
}
