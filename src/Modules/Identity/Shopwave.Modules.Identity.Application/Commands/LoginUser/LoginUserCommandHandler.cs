using Shopwave.Shared.Results;
using Shopwave.Shared.Abstractions;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;

using RefreshTokenEntity = Shopwave.Modules.Identity.Domain.Entities.RefreshToken;

namespace Shopwave.Modules.Identity.Application.Commands.LoginUser;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenRepository _refreshTokenRepository;


    public LoginUserCommandHandler(IUserRepository userRepository, IMediator mediator,
        IPasswordHasher passwordHasher, ITokenService tokenService, IUnitOfWork unitOfWork, IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
    }
    
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure<LoginUserResponse>("Email is required.");
        
        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<LoginUserResponse>("Password is required.");
        
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
            return Result.Failure<LoginUserResponse>("Invalid email.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result.Failure<LoginUserResponse>("Invalid password.");

        string token = _tokenService.GenerateToken(user.Id, user.Role.ToString());
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<LoginUserResponse>("Failed to generate token.");
        
        var rawToken = _tokenService.GenerateRefreshToken();
        if (string.IsNullOrWhiteSpace(rawToken))
            return Result.Failure<LoginUserResponse>("Failed to generate refresh token.");
        
        var refreshToken = RefreshTokenEntity.Create(user.Id, rawToken, DateTime.UtcNow.AddDays(7));
        if (string.IsNullOrWhiteSpace(refreshToken.Token))
            return Result.Failure<LoginUserResponse>("Failed to generate refresh token.");
        
        await _refreshTokenRepository.SaveAsync(refreshToken, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(
            new LoginUserResponse(
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    token,
                    rawToken
                )
            );
    }
}