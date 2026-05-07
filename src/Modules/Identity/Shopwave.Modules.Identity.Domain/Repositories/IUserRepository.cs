using Shopwave.Modules.Identity.Domain.Entities;

namespace Shopwave.Modules.Identity.Domain.Repositories;

/// <summary>
/// Defines a repository interface for managing User entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the repository asynchronously.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the user if found, or null.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken  cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the user if found, or null.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the repository asynchronously.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user from the repository asynchronously.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the specified email address asynchronously.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with true if exists, false otherwise.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}