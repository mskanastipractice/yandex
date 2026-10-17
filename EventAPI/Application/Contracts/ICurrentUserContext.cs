namespace Application.Contracts;

public interface ICurrentUserContext
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}