using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.CerrarLote;

/// <summary>
/// Cierra un lote activo. Solo se puede cerrar si no tiene animales activos.
/// </summary>
public sealed record CerrarLoteCommand(Guid LoteId) : IRequest<Result>;
