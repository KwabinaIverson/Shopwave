namespace Shopwave.Modules.Identity.Application.Abstractions;

public interface ITokenService
{
    string GenerateToken(Guid userId, string role);
    string GenerateRefreshToken();
}