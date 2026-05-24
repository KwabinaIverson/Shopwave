using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.API.Endpoints.Requests.Store.Requests;

public record AddStorePayoutMethodApiRequest(
    PayoutMethodType Type,
    string Provider,
    string AccountName,
    string AccountIdentifier
    );