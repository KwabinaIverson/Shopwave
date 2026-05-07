using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;

namespace Shopwave.Modules.Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
) : ICommand<Result<Guid>>
;