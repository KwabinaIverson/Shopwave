using System.Threading;


namespace Shopwave.Shared.Abstractions;

/// <summary>
/// Marker interface for queries that return a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
public interface IQuery<TResult>
{
    
}