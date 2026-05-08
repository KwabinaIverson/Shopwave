using Shopwave.Shared.Results;
using Shopwave.Shared.Abstractions;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;

namespace Shopwave.Modules.Identity.Application.Commands.LoginUser;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;


    public LoginUserCommandHandler(IUserRepository userRepository, IMediator mediator,
        IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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

        return Result.Success(
            new LoginUserResponse(
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    token
                )
            );
    }
}