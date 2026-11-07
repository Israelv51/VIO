using SQLite;
using VIO.App.Constants;
using VIO.App.Models;

namespace VIO.App.Services;

public class VioDatabase
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(
            DatabaseConstants.DatabasePath,
            DatabaseConstants.Flags);

        await _database.CreateTableAsync<User>();
    }

    public async Task<List<User>> GetUsersAsync()
    {
        await InitAsync();

        return await _database!
            .Table<User>()
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();

        return await _database!
            .Table<User>()
            .Where(user => user.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();

        var normalizedEmail = email
            .Trim()
            .ToLowerInvariant();

        return await _database!
            .Table<User>()
            .Where(user => user.Email == normalizedEmail)
            .FirstOrDefaultAsync();
    }

    public async Task<int> AddUserAsync(User user)
    {
        await InitAsync();

        user.Email = user.Email
            .Trim()
            .ToLowerInvariant();

        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        return await _database!
            .InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await InitAsync();

        user.UpdatedAt = DateTime.UtcNow;

        return await _database!
            .UpdateAsync(user);
    }
}
