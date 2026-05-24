using Moq;
using Xunit;
using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Application.Commands.RegisterUser;
using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Domain.Enums;
using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Identity.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepos = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<IPasswordHasher> _mockHasher = new();
    private readonly Mock<IIdentityUnitOfWork> _mockUow = new();

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _mockRepos.Object,
            _mockMediator.Object,
            _mockHasher.Object,
            _mockUow.Object
        );
    }

    private static RegisterUserCommand CreateValidCommand(string? firstName = "Phebe", string? lastName = "Adjetey", string email = "test@gmail.com", 
        UserRole role = UserRole.Buyer) => new(
        firstName!,
        lastName!,
        email,
        "password123",
        "+233123456789",
        role
    );

    // ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsFailure()
    {
        _mockRepos
            .Setup(r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            CreateValidCommand(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(
            r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockHasher.Verify(
            h => h.HashPassword(It.IsAny<string>()),
            Times.Never);

        _mockUow.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccessWithGuid()
    {
        _mockRepos
            .Setup(r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockHasher
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        var result = await _handler.Handle(
            CreateValidCommand(role: UserRole.Buyer),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        _mockRepos.Verify(
            r => r.AddAsync(
                It.Is<User>(u =>
                    u.Role == UserRole.Buyer &&
                    u.Email == "test@gmail.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockHasher.Verify(
            h => h.HashPassword(It.IsAny<string>()),
            Times.Once);

        _mockUow.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenFirstNameIsEmpty_ReturnsFailure()
    {
        var result = await _handler.Handle(
            CreateValidCommand(firstName: ""),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(
            r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mockHasher.Verify(
            h => h.HashPassword(It.IsAny<string>()),
            Times.Never);

        _mockUow.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenLastNameIsEmpty_ReturnsFailure()
    {
        var result = await _handler.Handle(
            CreateValidCommand(lastName: ""),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(
            r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mockHasher.Verify(
            h => h.HashPassword(It.IsAny<string>()),
            Times.Never);

        _mockUow.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─────────────────────────────────────────────

    [Theory]
    [InlineData(UserRole.Buyer)]
    [InlineData(UserRole.Seller)]
    public async Task Handle_WhenRoleProvided_AssignsCorrectRole(UserRole role)
    {
        _mockRepos
            .Setup(r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockHasher
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        await _handler.Handle(
            CreateValidCommand(role: role),
            CancellationToken.None);

        _mockRepos.Verify(
            r => r.AddAsync(
                It.Is<Domain.Entities.User>(u =>
                    u.Role == role),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}