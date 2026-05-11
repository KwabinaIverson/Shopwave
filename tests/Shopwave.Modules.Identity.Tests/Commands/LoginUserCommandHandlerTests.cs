using Moq;
using Xunit;

using Shopwave.Shared.Abstractions;

using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Domain.Enums;
using Shopwave.Modules.Identity.Domain.Repositories;

using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Application.Commands.LoginUser;
using Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;

namespace Shopwave.Modules.Identity.Tests.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepos = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<IPasswordHasher> _mockHasher = new();
    private readonly Mock<ITokenService> _mockTokenService = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository = new();

    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(
            _mockRepos.Object,
            _mockMediator.Object,
            _mockHasher.Object,
            _mockTokenService.Object,
            _mockUnitOfWork.Object,
            _mockRefreshTokenRepository.Object
        );
    }

    private static LoginUserCommand CreateValidCommand(
        string email = "test@gmail.com",
        string password = "password123")
        => new(email, password);

    [Fact]
    public async Task Handle_WhenEmailIsEmpty_ReturnsFailure()
    {
        var command = CreateValidCommand(email: "");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(r =>
            r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockHasher.Verify(h =>
            h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _mockTokenService.Verify(t =>
            t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsEmpty_ReturnsFailure()
    {
        var command = CreateValidCommand(password: "");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);

        _mockTokenService.Verify(t =>
            t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsFailure()
    {
        var command = CreateValidCommand();

        _mockRepos.Setup(r =>
            r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);

        _mockTokenService.Verify(t =>
            t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsFailure()
    {
        var command = CreateValidCommand();

        var user = User.Create(
            "John",
            "Doe",
            command.Email,
            "hashed-password",
            "+233240000000",
            UserRole.Buyer
        );

        _mockRepos.Setup(r =>
            r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockHasher.Setup(h =>
            h.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);

        _mockTokenService.Verify(t =>
            t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsSuccess()
    {
        var command = CreateValidCommand();

        var user = User.Create(
            "John",
            "Doe",
            command.Email,
            "hashed-password",
            "+233240000000",
            UserRole.Buyer
        );

        var token = "jwt-token";

        _mockRepos.Setup(r =>
            r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockHasher.Setup(h =>
            h.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _mockTokenService.Setup(t =>
            t.GenerateToken(user.Id, user.Role.ToString()))
            .Returns(token);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<LoginUserResponse>(result.Value);
        Assert.Equal(token, result.Value.Token);

        _mockTokenService.Verify(t =>
            t.GenerateToken(user.Id, user.Role.ToString()),
            Times.Once);
    }
}