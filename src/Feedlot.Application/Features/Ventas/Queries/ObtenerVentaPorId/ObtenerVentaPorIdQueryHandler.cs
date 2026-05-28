using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Queries.ObtenerVentaPorId;

public sealed class ObtenerVentaPorIdQueryHandler
    : IRequestHandler<ObtenerVentaPorIdQuery, Result<VentaDto>>
{
    private readonly IVentaRepository _ventaRepository;
    private readonly ICompradorRepository _compradorRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IMapper _mapper;

    public ObtenerVentaPorIdQueryHandler(
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

    public async Task<Result<VentaDto>> Handle(
        ObtenerVentaPorIdQuery request, CancellationToken ct)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(request.Id, ct);
        if (venta is null)
            return Result<VentaDto>.NotFound($"Venta {request.Id} no encontrada.");

        var dto = _mapper.Map<VentaDto>(venta);
        dto.MontoTotal = venta.MontoTotal;

        var comprador = await _compradorRepository.ObtenerPorIdAsync(venta.CompradorId, ct);
        dto.NombreComprador = comprador?.Nombre ?? "Desconocido";

        var animalIds = venta.Items.Select(i => i.AnimalId).ToList();
        var animales = await _animalRepository.ObtenerCodigosPorIdsAsync(animalIds, ct);

        foreach (var item in dto.Items)
        {
            if (animales.TryGetValue(item.AnimalId, out var info))
            {
                item.CodigoAnimal = info.Codigo;
                item.NombreAnimal = info.Nombre;
            }
        }

        return Result<VentaDto>.Success(dto);
    }
}
