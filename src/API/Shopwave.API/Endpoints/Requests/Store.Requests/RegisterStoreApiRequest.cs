namespace Shopwave.API.Endpoints.Requests.Store.Requests;

public record RegisterStoreApiRequest(
    string DisplayName,
    string Slug,
    string BusinessName,
    StoreAddressApiRequest StoreAddress
    );