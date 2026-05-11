using Shopwave.Shared.Results;
using Shopwave.Shared.Abstractions;

using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken.Responses;

using RefreshTokenEntity = Shopwave.Modules.Identity.Domain.Entities.RefreshToken;

namespace Shopwave.Modules.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Handles refresh token commands to generate new access tokens and refresh tokens.
/// This handler implements the token rotation pattern for enhanced security.
/// </summary>
public class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
    /// </summary>
    /// <param name="tokenService">Service for generating JWT and refresh tokens.</param>
    /// <param name="refreshTokenRepository">Repository for refresh token data access.</param>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="unitOfWork">Unit of work for transaction management.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public RefreshTokenCommandHandler(ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));

        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Handles the refresh token command asynchronously.
    /// Validates the provided refresh token, generates new tokens, and implements token rotation.
    /// </summary>
    /// <param name="request">The refresh token command containing the refresh token to validate.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation with a result containing either the new tokens or an error message.</returns>
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Failure<RefreshTokenResponse>("Refresh token is required.");
        
        var token = await _refreshTokenRepository
                .FindByTokenStringAsync(request.RefreshToken, ct);

        if (token is null)
            return Result.Failure<RefreshTokenResponse>("Invalid refresh token.");
        
        if (!token.IsActive())
            return Result.Failure<RefreshTokenResponse>("Refresh token is not active.");
        
        var user = await _userRepository.GetByIdAsync(token.UserId, ct);

        if (user is null)
            return Result.Failure<RefreshTokenResponse>("User not found.");
        
        var accessToken = _tokenService.GenerateToken(user.Id, user.Role.ToString());

        var rawNewRefreshToken = _tokenService.GenerateRefreshToken();

        var hashedNewRefreshToken = RefreshTokenEntity.HashToken(rawNewRefreshToken);
        
        token.Revoke(hashedNewRefreshToken);
        await _refreshTokenRepository.UpdateAsync(token, ct);
        
        var newRefreshToken = RefreshTokenEntity.Create(user.Id, rawNewRefreshToken, DateTime.UtcNow.AddDays(7));
        
        await _refreshTokenRepository
            .SaveAsync(newRefreshToken, ct);
        
        await _unitOfWork
            .SaveChangesAsync(ct);
        
        return Result.Success(new RefreshTokenResponse(
                accessToken,
                rawNewRefreshToken
            ));
    }
}