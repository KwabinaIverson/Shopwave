using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken.Responses;

namespace Shopwave.Modules.Identity.Application.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
    ) : ICommand<Result<RefreshTokenResponse>>;