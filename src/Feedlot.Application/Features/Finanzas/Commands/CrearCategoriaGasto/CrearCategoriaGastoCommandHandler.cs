using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearCategoriaGasto;

public sealed class CrearCategoriaGastoCommandHandler
    : IRequestHandler<CrearCategoriaGastoCommand, Result<Guid>>
{
    private readonly ICategoriaGastoRepository _categoriaRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CrearCategoriaGastoCommandHandler(
        ICategoriaGastoRepository categoriaRepo,
        IUnitOfWork unitOfWork)
    {
        _categoriaRepo = categoriaRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CrearCategoriaGastoCommand request,
        CancellationToken ct)
    {
        var existe = await _categoriaRepo.ObtenerPorNombreAsync(request.Nombre, ct);
        if (existe is not null)
            return Result<Guid>.Failure(
                $"Ya existe una categoría de gasto con el nombre '{request.Nombre}'.");

        if (!Enum.TryParse<TipoCategoriaGasto>(request.Tipo, ignoreCase: true, out var tipo))
            return Result<Guid>.Failure("Tipo de categoría de gasto inválido.");

        var categoria = CategoriaGasto.Crear(request.Nombre, tipo);

        await _categoriaRepo.AgregarAsync(categoria, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(categoria.Id);
    }
}
