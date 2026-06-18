namespace Shopwave.Modules.Stores.Application.Queries.GetStoreProfile.Responses;

public record GetStoreAddressResponse(
    Guid AddressId,
    string StreetAddress1,
    string? StreetAddress2,
    string City,
    string StateProvinceRegion,
    string Country,
    string? PostalZipCode
    );