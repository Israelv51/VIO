using VIO.App.Models;

namespace VIO.App.Services;

public partial class VioDatabase
{
    public async Task<List<Certification>>
        GetCertificationsAsync(
            int userId,
            string search = "")
    {
        await InitAsync();

        var items =
            await _database!
                .Table<Certification>()
                .Where(x => x.UserId == userId)
                .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term =
                search.Trim();

            items =
                items.Where(x =>
                        x.Name.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase) ||
                        x.Issuer.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }

        return items
            .OrderByDescending(x => x.IssueDate)
            .ToList();
    }

    public async Task<int>
        SaveCertificationAsync(
            Certification item)
    {
        await InitAsync();

        if (item.Id == 0)
        {
            item.CreatedAt =
                DateTime.UtcNow;

            return await _database!
                .InsertAsync(item);
        }

        return await _database!
            .UpdateAsync(item);
    }

    public async Task<int>
        DeleteCertificationAsync(
            Certification item)
    {
        await InitAsync();

        return await _database!
            .DeleteAsync(item);
    }
}
