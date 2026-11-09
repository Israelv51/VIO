using VIO.App.Models;

namespace VIO.App.Services;

public partial class VioDatabase
{
    public async Task<int> UpdateDocumentAsync(
        VioDocument document)
    {
        await InitAsync();

        return await _database!
            .UpdateAsync(document);
    }
}
