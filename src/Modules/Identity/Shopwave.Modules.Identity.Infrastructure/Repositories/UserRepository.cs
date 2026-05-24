using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Persistence;
using Shopwave.Modules.Identity.Application.Abstractions;

namespace Shopwave.Modules.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
	private readonly IIdentityUnitOfWork _unitOfWork;

    public UserRepository(IdentityDbContext context,  IIdentityUnitOfWork unitOfWork)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
		_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
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
            .AnyAsync(u => u.Email == email, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(User user, CancellationToken ct = default)
    {
        user.Delete();

        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}