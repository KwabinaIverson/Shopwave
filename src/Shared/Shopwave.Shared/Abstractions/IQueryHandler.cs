using System.Threading;

namespace Shopwave.Shared.Abstractions;

/// <summary>
/// Defines a handler for queries.
/// </summary>
/// <typeparam name="TQuery">The type of the query to handle.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query asynchronously and returns a result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}