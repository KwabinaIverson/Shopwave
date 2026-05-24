using Moq;
using Xunit;

using Shopwave.Shared.Abstractions;

using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Domain.Enums;
using Shopwave.Modules.Identity.Domain.Repositories;

using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken.Responses;

namespace Shopwave.Modules.Identity.Tests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<ITokenService> _mockTokenService = new();
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<IIdentityUnitOfWork> _mockUnitOfWork = new();

    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _mockTokenService.Object,
            _mockRefreshTokenRepository.Object,
            _mockUserRepository.Object,
            _mockUnitOfWork.Object
        );
    }

    private static RefreshTokenCommand CreateCommand(
        string token = "refresh-token")
        => new(token);

    [Fact]
    public async Task Handle_WhenRefreshTokenIsEmpty_ReturnsFailure()
    {
        var command = CreateCommand("");

        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockRefreshTokenRepository.Verify(r =>
            r.FindByTokenStringAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenTokenDoesNotExist_ReturnsFailure()
    {
        var command = CreateCommand();
        var hashedToken = RefreshToken.HashToken(
            command.RefreshToken
        );

        _mockRefreshTokenRepository
            .Setup(r => r.FindByTokenStringAsync(
                hashedToken,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockUserRepository.Verify(u =>
            u.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenTokenIsInactive_ReturnsFailure()
    {
        var command = CreateCommand();
        var hashedToken = RefreshToken.HashToken(
            command.RefreshToken
        );

        var token = RefreshToken.Create(
            Guid.NewGuid(),
            command.RefreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        token.Revoke("replacement-token");

        _mockRefreshTokenRepository
            .Setup(r => r.FindByTokenStringAsync(
                hashedToken,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(token);

        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockUserRepository.Verify(u =>
            u.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var command = CreateCommand();

        var hashedToken = RefreshToken.HashToken(
            command.RefreshToken
        );

        var token = RefreshToken.Create(
            userId,
            command.RefreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        _mockRefreshTokenRepository
            .Setup(r => r.FindByTokenStringAsync(
                hashedToken,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(token);

        _mockUserRepository
            .Setup(u => u.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockTokenService.Verify(t =>
            t.GenerateToken(
                It.IsAny<Guid>(),
                It.IsAny<string>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsValid_RotatesAndReturnsSuccess()
    {
        var command = CreateCommand(
            "old-refresh-token"
        );

        var hashedToken = RefreshToken.HashToken(
            command.RefreshToken
        );

        var user = User.Create(
            "John",
            "Doe",
            "john@example.com",
            "hashed-password",
            "+233240000000",
            UserRole.Buyer
        );

        var oldToken = RefreshToken.Create(
            user.Id,
            command.RefreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        var accessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";

        _mockRefreshTokenRepository
            .Setup(r => r.FindByTokenStringAsync(
                hashedToken,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(oldToken);

        _mockUserRepository
            .Setup(u => u.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(user);

        _mockTokenService
            .Setup(t => t.GenerateToken(
                user.Id,
                user.Role.ToString()
            ))
            .Returns(accessToken);

        _mockTokenService
            .Setup(t => t.GenerateRefreshToken())
            .Returns(newRefreshToken);

        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<RefreshTokenResponse>(
            result.Value
        );

        Assert.Equal(
            accessToken,
            result.Value.AccessToken
        );

        Assert.Equal(
            newRefreshToken,
            result.Value.RefreshToken
        );

        _mockRefreshTokenRepository.Verify(r =>
            r.UpdateAsync(
                oldToken,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _mockRefreshTokenRepository.Verify(r =>
            r.SaveAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _mockUnitOfWork.Verify(u =>
            u.SaveChangesAsync(
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}