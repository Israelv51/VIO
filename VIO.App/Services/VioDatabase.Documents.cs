using VIO.App.Models;

namespace VIO.App.Services;

public partial class VioDatabase
{
    public async Task<List<VioDocument>> GetDocumentsAsync(
        int userId,
        string search = "")
    {
        await InitAsync();

        var items = await _database!
            .Table<VioDocument>()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();

            items = items
                .Where(x =>
                    x.DisplayName.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.Category.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.Extension.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return items
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<int> AddDocumentAsync(
        VioDocument document)
    {
        await InitAsync();

        document.CreatedAt = DateTime.UtcNow;

        return await _database!
            .InsertAsync(document);
    }

    public async Task<int> DeleteDocumentAsync(
        VioDocument document)
    {
        await InitAsync();

        return await _database!
            .DeleteAsync(document);
    }
}
