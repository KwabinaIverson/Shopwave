using System.Threading;

namespace Shopwave.Shared.Abstractions;

/// <summary>
/// Provides a mediator for handling commands, queries, and domain events in a decoupled manner.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a command asynchronously without expecting a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to send.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    
    /// <summary>
    /// Sends a command asynchronously and returns a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to send.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;
    
    /// <summary>
    /// Executes a query asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query instance.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;

    /// <summary>
    /// Publishes a domain event asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event instance.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Publish<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}