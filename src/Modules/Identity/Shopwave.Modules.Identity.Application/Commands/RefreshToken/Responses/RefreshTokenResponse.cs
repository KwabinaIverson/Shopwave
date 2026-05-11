namespace Shopwave.Modules.Identity.Application.Commands.RefreshToken.Responses;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
    );