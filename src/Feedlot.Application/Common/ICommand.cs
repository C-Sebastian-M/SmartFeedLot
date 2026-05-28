using MediatR;

namespace Feedlot.Application.Common;

public interface ICommandBase { }

public interface ICommand : ICommandBase, IRequest<Result> { }

public interface ICommand<T> : ICommandBase, IRequest<Result<T>> { }
