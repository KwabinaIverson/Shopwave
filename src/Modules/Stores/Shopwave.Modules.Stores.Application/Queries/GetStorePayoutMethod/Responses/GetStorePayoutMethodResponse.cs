namespace Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod.Responses;

public record GetStorePayoutMethodResponse(
    Guid PayoutId,
    string Provider,
    string? AccountName,
    string AccountIdentifier,
    bool IsVerified
    );