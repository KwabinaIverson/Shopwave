using System.Threading;

namespace Shopwave.Shared.Abstractions;

/// <summary>
/// Defines a handler for commands that do not return a result.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
public interface ICommandHandler<TCommand> where TCommand : ICommand 
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for commands that return a result.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the specified command asynchronously and returns a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}