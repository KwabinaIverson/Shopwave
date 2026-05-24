namespace Shopwave.API.Endpoints.Requests.Store.Requests;

public record RegisterStoreApiRequest(
    Guid OwnerId,
    string DisplayName,
    string Slug,
    string BusinessName,
    StoreAddressApiRequest StoreAddress
    );