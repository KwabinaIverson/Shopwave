using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Persistence;

namespace Shopwave.Modules.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
	{
    	var normalizedEmail = email.ToLowerInvariant();
    	return await _context.Users
        	.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
	}

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
		var normalizedEmail = email.ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        user.Delete();

        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}