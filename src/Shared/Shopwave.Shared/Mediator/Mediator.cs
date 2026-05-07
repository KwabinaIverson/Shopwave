using Microsoft.Extensions.DependencyInjection;
using Shopwave.Shared.Abstractions;

namespace Shopwave.Shared.Mediator;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // 🔹 command without result
    public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        await handler.Handle(command, cancellationToken);
    }

    // 🔹 command with result
    public async Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        return await handler.Handle(command, cancellationToken);
    }

    // 🔹 query
    public async Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        return await handler.Handle(query, cancellationToken);
    }

    // 🔹 domain events (fan-out)
    public async Task Publish<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        var tasks = handlers.Select(handler => handler.Handle(domainEvent, cancellationToken));

        await Task.WhenAll(tasks);
    }
}