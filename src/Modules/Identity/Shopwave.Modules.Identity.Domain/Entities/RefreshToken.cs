using System.Text;
using Shopwave.Shared.Domain;
using System.Security.Cryptography;

namespace Shopwave.Modules.Identity.Domain.Entities;

/// <summary>
/// Represents a refresh token belonging to a user.
/// Refresh tokens are part of the User aggregate
/// and should only be created/manipulated through the User aggregate root.
/// </summary>
public class RefreshToken : Entity
{
    private Guid _userId;
    private string _token = default!;
    private DateTime _expiresAt;
    private bool _isRevoked;
    private string? _replacedByToken;

    /// <summary>
    /// Gets the user id that owns this refresh token.
    /// </summary>
    public Guid UserId
    {
        get => _userId;
        private set
        {
            if (value == Guid.Empty)
                throw new ArgumentException(
                    "UserId cannot be empty.",
                    nameof(value));

            _userId = value;
        }
    }

    /// <summary>
    /// Gets the refresh token value.
    /// </summary>
    public string Token
    {
        get => _token;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Token cannot be empty.",
                    nameof(value));

            _token = value;
        }
    }

    /// <summary>
    /// Gets when the refresh token expires.
    /// </summary>
    public DateTime ExpiresAt
    {
        get => _expiresAt;
        private set
        {
            if (value <= DateTime.UtcNow)
                throw new ArgumentException(
                    "Expiration date must be in the future.",
                    nameof(value));

            _expiresAt = value;
        }
    }

    /// <summary>
    /// Gets whether the refresh token has been revoked.
    /// </summary>
    public bool IsRevoked
    {
        get => _isRevoked;
        private set => _isRevoked = value;
    }

    /// <summary>
    /// Gets the replacement token during token rotation.
    /// </summary>
    public string? ReplacedByToken
    {
        get => _replacedByToken;
        private set => _replacedByToken = value;
    }

    /// <summary>
    /// Navigation property to the owning user.
    /// </summary>
    public User User { get; private set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// This parameterless constructor is used by the ORM for deserialization.
    /// </summary>
    private RefreshToken()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who owns this refresh token.</param>
    /// <param name="token">The refresh token value.</param>
    /// <param name="expiresAt">The expiration date and time of the refresh token in UTC.</param>
    /// <exception cref="ArgumentException">Thrown when userId is empty, token is null/empty, or expiresAt is in the past.</exception>
    private RefreshToken(
        Guid userId,
        string token,
        DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }
    
    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Creates a new refresh token with the specified parameters.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who owns this refresh token.</param>
    /// <param name="token">The refresh token value to be stored.</param>
    /// <param name="expiresAt">The expiration date and time of the refresh token in UTC. Must be in the future.</param>
    /// <returns>A new instance of <see cref="RefreshToken"/> with the specified values.</returns>
    /// <exception cref="ArgumentException">Thrown when userId is empty, token is null/empty, or expiresAt is in the past.</exception>
    public static RefreshToken Create(Guid userId, string rawToken, DateTime expiresAt)
    {
        return new RefreshToken(userId, HashToken(rawToken), expiresAt);
    }

    /// <summary>
    /// Revokes the refresh token, optionally recording the replacement token during token rotation.
    /// Once revoked, the token can no longer be used for authentication.
    /// </summary>
    /// <param name="replacedByToken">Optional token value that replaced this token during token rotation.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to revoke an already revoked token.</exception>
    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException(
                "Refresh token is already revoked.");

        IsRevoked = true;
        ReplacedByToken = replacedByToken;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the refresh token has expired.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the current UTC time is greater than or equal to the token's expiration time; otherwise, <c>false</c>.
    /// </returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Determines whether the refresh token is currently active and can be used for authentication.
    /// A token is considered active if it is neither revoked nor expired.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the token is active (not revoked and not expired); otherwise, <c>false</c>.
    /// </returns>
    public bool IsActive()
    {
        return !IsRevoked && !IsExpired();
    }
}