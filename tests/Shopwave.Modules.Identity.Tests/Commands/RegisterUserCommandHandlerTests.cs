using Moq;
using Xunit;
using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Application.Commands.RegisterUser;
using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Identity.Tests.Commands;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandHandler"/> class.
/// Tests cover various scenarios including successful registration, validation failures, and duplicate email detection.
/// </summary>
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepos = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<IPasswordHasher> _mockHasher = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();

    private readonly RegisterUserCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandlerTests"/> class.
    /// Sets up the test fixtures with mock dependencies and initializes the handler.
    /// </summary>
    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _mockRepos.Object,
            _mockMediator.Object,
            _mockHasher.Object,
            _mockUow.Object
        );
    }

    /// <summary>
    /// Creates a valid <see cref="RegisterUserCommand"/> with default or specified values.
    /// </summary>
    /// <param name="firstName">The first name for the command. Defaults to "Phebe".</param>
    /// <param name="lastName">The last name for the command. Defaults to "Adjetey".</param>
    /// <param name="email">The email for the command. Defaults to "test@gmail.com".</param>
    /// <returns>A <see cref="RegisterUserCommand"/> instance with the specified values.</returns>
    private static RegisterUserCommand CreateValidCommand(
        string? firstName = "Phebe",
        string? lastName = "Adjetey",
        string email = "test@gmail.com"
    ) => new(
        firstName!,
        lastName!,
        email,
        "password123",
        "+233123456789"
    );

    // ─────────────────────────────────────────────

    /// <summary>
    /// Tests that the handler returns a failure result when the email already exists in the repository.
    /// </summary>
    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsFailure()
    {
        _mockRepos
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─────────────────────────────────────────────

    /// <summary>
    /// Tests that the handler returns a success result with a valid user ID when all inputs are valid and the email is unique.
    /// Verifies that the user repository and password hasher are called appropriately.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccessWithGuid()
    {
        _mockRepos
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockHasher
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        _mockRepos.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─────────────────────────────────────────────

    /// <summary>
    /// Tests that the handler returns a failure result when the first name is empty or whitespace.
    /// Verifies that no database operations are performed when validation fails.
    /// </summary>
    [Fact]
    public async Task Handle_WhenFirstNameIsEmpty_ReturnsFailure()
    {
        var result = await _handler.Handle(
            CreateValidCommand(firstName: ""),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─────────────────────────────────────────────

    /// <summary>
    /// Tests that the handler returns a failure result when the last name is empty or whitespace.
    /// Verifies that no database operations are performed when validation fails.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLastNameIsEmpty_ReturnsFailure()
    {
        var result = await _handler.Handle(
            CreateValidCommand(lastName: ""),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);

        _mockRepos.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}