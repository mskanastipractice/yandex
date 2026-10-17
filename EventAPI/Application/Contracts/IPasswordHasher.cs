namespace Application.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string inputPassword, string storedHash);
}