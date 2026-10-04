using Domain.Entities;

namespace Application.Contracts;

public interface IJwtProvider
{
    string GenerateToken(User user);
}