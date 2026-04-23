using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Domain.Enums;
using Shopwave.Modules.Identity.Domain.Entities;

namespace Shopwave.Modules.Identity.Application.Commands.RegisterUser;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IMediator mediator,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result.Failure<Guid>("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result.Failure<Guid>("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure<Guid>("Email is required.");
        
        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<Guid>("Password is required.");
        
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Result.Failure<Guid>("Phone number is required.");
        
        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            return Result.Failure<Guid>("Email is already in use.");

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            request.PhoneNumber,
            UserRole.Buyer
            );

        await _userRepository.AddAsync(user);
        
        foreach (var domainEvent in user.DomainEvents)
            await _mediator.Publish(domainEvent, ct);

        user.ClearDomainEvents();

        return Result.Success(user.Id);
    }
}