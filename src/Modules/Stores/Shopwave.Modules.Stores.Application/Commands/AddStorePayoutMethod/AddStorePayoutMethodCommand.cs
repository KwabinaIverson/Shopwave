using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Application.Commands.AddStorePayoutMethod;

public record AddStorePayoutMethodCommand(
    Guid StoreId,
    Guid CurrentUserId,
    PayoutMethodType Type,
    string Provider,
    string AccountName,
    string  AccountIdentifier
    ) : ICommand<Result<Guid>>;