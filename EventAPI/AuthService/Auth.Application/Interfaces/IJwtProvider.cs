using Auth.Domain.User;

namespace Auth.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}