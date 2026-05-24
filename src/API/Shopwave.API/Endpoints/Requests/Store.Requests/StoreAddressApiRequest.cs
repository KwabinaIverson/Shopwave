namespace Shopwave.API.Endpoints.Requests.Store.Requests;

public record StoreAddressApiRequest(
    string StreetAddress1,
    string? StreetAddress2,
    string City,
    string StateProvinceRegion,
    string Country,
    string? PostalZipCode
    );