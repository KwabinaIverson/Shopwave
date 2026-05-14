using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Identity.Domain.Enums;

namespace Shopwave.Modules.Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    UserRole Role
) : ICommand<Result<Guid>>;