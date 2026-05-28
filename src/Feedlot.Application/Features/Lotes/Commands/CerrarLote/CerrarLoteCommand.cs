using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Lotes.Commands.CerrarLote;

public sealed record CerrarLoteCommand(Guid LoteId) : ICommand;
