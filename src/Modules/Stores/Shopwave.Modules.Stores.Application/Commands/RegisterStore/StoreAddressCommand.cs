using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;

namespace Shopwave.Modules.Stores.Application.Commands.RegisterStore;

public record StoreAddressCommand(
    string StreetAddress1,
    string? StreetAddress2,
    string City,
    string StateProvinceRegion,
    string Country,
    string? PostalZipCode
    ) : ICommand<Result<Guid>>;