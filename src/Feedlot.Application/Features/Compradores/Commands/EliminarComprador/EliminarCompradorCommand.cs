using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Compradores.Commands.EliminarComprador;

public sealed record EliminarCompradorCommand(Guid Id) : ICommand;
