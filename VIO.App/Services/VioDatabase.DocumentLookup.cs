using VIO.App.Models;

namespace VIO.App.Services;

public partial class VioDatabase
{
    public async Task<VioDocument?> GetDocumentByIdAsync(
        int userId,
        int documentId)
    {
        await InitAsync();

        return await _database!
            .Table<VioDocument>()
            .Where(x =>
                x.Id == documentId &&
                x.UserId == userId)
            .FirstOrDefaultAsync();
    }
}
