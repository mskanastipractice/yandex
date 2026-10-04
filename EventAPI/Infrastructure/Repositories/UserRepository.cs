using Application.Contracts;
using Domain.Entities;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(AppDbContext context)  : IUserRepository
{
    public ValueTask<User?> FindAsync(Guid userId, CancellationToken cancellationToken) 
        => context.Users.FindAsync([userId], cancellationToken);

    public async Task<User?> FindByLoginAsync(string login, CancellationToken cancellationToken) 
        => await context.Users.FirstOrDefaultAsync(x => x.Login == login, cancellationToken: cancellationToken);

    public async Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken)
        => await context.Users.AnyAsync(x => x.Login == login, cancellationToken: cancellationToken);

    public Task AddAsync(User user)
    {
        context.Users.Add(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await context.SaveChangesAsync(cancellationToken);
}