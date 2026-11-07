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
        await _database.CreateTableAsync<ProfessionalProfile>();
        await _database.CreateTableAsync<Education>();
        await _database.CreateTableAsync<WorkExperience>();
        await _database.CreateTableAsync<Skill>();
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
            await GetProfessionalProfileAsync(
                profile.UserId);

        profile.UpdatedAt = DateTime.UtcNow;

        if (existing is null)
        {
            return await _database!
                .InsertAsync(profile);
        }

        profile.Id = existing.Id;

        return await _database!
            .UpdateAsync(profile);
    }

    public async Task<List<Education>> GetEducationAsync(
        int userId)
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

    public async Task<int> AddEducationAsync(
        Education education)
    {
        await InitAsync();

        education.CreatedAt =
            DateTime.UtcNow;

        return await _database!
            .InsertAsync(education);
    }

    public async Task<int> DeleteEducationAsync(
        Education education)
    {
        await InitAsync();

        return await _database!
            .DeleteAsync(education);
    }

    public async Task<List<WorkExperience>> GetExperiencesAsync(
        int userId)
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

    public async Task<int> AddExperienceAsync(
        WorkExperience experience)
    {
        await InitAsync();

        experience.CreatedAt =
            DateTime.UtcNow;

        return await _database!
            .InsertAsync(experience);
    }

    public async Task<int> DeleteExperienceAsync(
        WorkExperience experience)
    {
        await InitAsync();

        return await _database!
            .DeleteAsync(experience);
    }

    public async Task<List<Skill>> GetSkillsAsync(
        int userId)
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

    public async Task<int> AddSkillAsync(
        Skill skill)
    {
        await InitAsync();

        skill.CreatedAt =
            DateTime.UtcNow;

        return await _database!
            .InsertAsync(skill);
    }

    public async Task<int> DeleteSkillAsync(
        Skill skill)
    {
        await InitAsync();

        return await _database!
            .DeleteAsync(skill);
    }
}
