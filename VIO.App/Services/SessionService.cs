using Microsoft.Maui.Storage;
using VIO.App.Interfaces;
using VIO.App.Models;

namespace VIO.App.Services;

public sealed class SessionService : ISessionService
{
    private const string CurrentUserKey = "vio_current_user_id";

    private readonly VioDatabase _database;

    public SessionService(VioDatabase database)
    {
        _database = database;
    }

    public int? CurrentUserId
    {
        get
        {
            int userId = Preferences.Default.Get(
                CurrentUserKey,
                0);

            return userId > 0
                ? userId
                : null;
        }
    }

    public bool IsAuthenticated =>
        CurrentUserId.HasValue;

    public void Start(int userId)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        Preferences.Default.Set(
            CurrentUserKey,
            userId);
    }

    public void End()
    {
        Preferences.Default.Remove(CurrentUserKey);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        if (CurrentUserId is not int userId)
            return null;

        return await _database.GetUserByIdAsync(userId);
    }
}
