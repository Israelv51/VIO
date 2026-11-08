using SQLite;
using VIO.App.Constants;
using VIO.App.Models;

namespace VIO.App.Services;

public partial class VioDatabase
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
        await _database.CreateTableAsync<ProfessionalProfile>();
        await _database.CreateTableAsync<Education>();
        await _database.CreateTableAsync<WorkExperience>();
        await _database.CreateTableAsync<Skill>();
        await _database.CreateTableAsync<VioDocument>();
        await _database.CreateTableAsync<Certification>();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();

        return await _database!
            .Table<User>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return await _database!
            .Table<User>()
            .Where(x => x.Email == normalizedEmail)
            .FirstOrDefaultAsync();
    }

    public async Task<int> AddUserAsync(User user)
    {
        await InitAsync();

        user.Email =
            user.Email.Trim().ToLowerInvariant();

        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        return await _database!.InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await InitAsync();

        user.UpdatedAt = DateTime.UtcNow;

        return await _database!.UpdateAsync(user);
    }

    public async Task<ProfessionalProfile?> GetProfessionalProfileAsync(
        int userId)
    {
        await InitAsync();

        return await _database!
            .Table<ProfessionalProfile>()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveProfessionalProfileAsync(
        ProfessionalProfile profile)
    {
        await InitAsync();

        var existing =
            await GetProfessionalProfileAsync(profile.UserId);

        profile.UpdatedAt = DateTime.UtcNow;

        if (existing is null)
            return await _database!.InsertAsync(profile);

        profile.Id = existing.Id;

        return await _database!.UpdateAsync(profile);
    }

    public async Task<List<Education>> GetEducationAsync(int userId)
    {
        await InitAsync();

        var items = await _database!
            .Table<Education>()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return items
            .OrderByDescending(x => x.StartDate)
            .ToList();
    }

    public async Task<int> SaveEducationAsync(Education item)
    {
        await InitAsync();

        if (item.Id == 0)
        {
            item.CreatedAt = DateTime.UtcNow;
            return await _database!.InsertAsync(item);
        }

        return await _database!.UpdateAsync(item);
    }

    public async Task<int> DeleteEducationAsync(Education item)
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }

    public async Task<List<WorkExperience>> GetExperiencesAsync(int userId)
    {
        await InitAsync();

        var items = await _database!
            .Table<WorkExperience>()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return items
            .OrderByDescending(x => x.StartDate)
            .ToList();
    }

    public async Task<int> SaveExperienceAsync(WorkExperience item)
    {
        await InitAsync();

        if (item.Id == 0)
        {
            item.CreatedAt = DateTime.UtcNow;
            return await _database!.InsertAsync(item);
        }

        return await _database!.UpdateAsync(item);
    }

    public async Task<int> DeleteExperienceAsync(WorkExperience item)
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }

    public async Task<List<Skill>> GetSkillsAsync(int userId)
    {
        await InitAsync();

        var items = await _database!
            .Table<Skill>()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return items
            .OrderBy(x => x.Name)
            .ToList();
    }

    public async Task<int> SaveSkillAsync(Skill item)
    {
        await InitAsync();

        if (item.Id == 0)
        {
            item.CreatedAt = DateTime.UtcNow;
            return await _database!.InsertAsync(item);
        }

        return await _database!.UpdateAsync(item);
    }

    public async Task<int> DeleteSkillAsync(Skill item)
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }
}


