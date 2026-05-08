using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;

namespace Shopwave.Modules.Identity.Application.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
    ) : ICommand<Result<LoginUserResponse>>;