using VIO.App.Models;

namespace VIO.App.Interfaces;

public interface ISessionService
{
    int? CurrentUserId { get; }

    bool IsAuthenticated { get; }

    void Start(int userId);

    void End();

    Task<User?> GetCurrentUserAsync();
}
