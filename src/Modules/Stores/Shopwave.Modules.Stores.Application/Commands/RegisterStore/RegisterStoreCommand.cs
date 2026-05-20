using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;

namespace Shopwave.Modules.Stores.Application.Commands.RegisterStore;

public record RegisterStoreCommand(
    Guid OwnerId,
    string DisplayName,
    string Slug,
    string BusinessName,
    StoreAddressCommand StoreAddress
    ) : ICommand<Result<Guid>>;