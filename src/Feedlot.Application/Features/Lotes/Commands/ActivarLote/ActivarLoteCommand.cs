using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.ActivarLote;

/// <summary>
/// Activa un lote que está en estado EnPreparacion.
/// Una vez activo, puede recibir animales.
/// </summary>
public sealed record ActivarLoteCommand(Guid LoteId) : IRequest<Result>;
