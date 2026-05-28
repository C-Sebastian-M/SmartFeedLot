using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Lotes.Commands.ActivarLote;

public sealed record ActivarLoteCommand(Guid LoteId) : ICommand;
