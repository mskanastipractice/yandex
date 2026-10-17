using Domain.Entities;

namespace Application.Contracts;

public interface IUserRepository
{
    ValueTask<User?> FindAsync(Guid userId, CancellationToken cancellationToken);
    
    public Task<User?> FindByLoginAsync(string login, CancellationToken cancellationToken);

    public Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken);

    public Task AddAsync(User user);

    public Task SaveChangesAsync(CancellationToken cancellationToken);
}