namespace Shopwave.Modules.Stores.Application.Queries.GetStoreProfile.Responses;
public record GetStoreResponse(
    Guid StoreId,
    string DisplayName,
    string Slug,
    string BusinessName,
    GetStoreAddressResponse Address
);