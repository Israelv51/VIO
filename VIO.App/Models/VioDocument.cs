using SQLite;

namespace VIO.App.Models;

[Table("VioDocuments")]
public class VioDocument
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string Category { get; set; } = "General";

    public string Description { get; set; } = string.Empty;

    public bool IsPrivate { get; set; } = true;

    public bool IsOfficialDocument { get; set; }

    public long SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public string TypeLabel =>
        Extension.TrimStart('.').ToUpperInvariant();

    [Ignore]
    public string SizeText =>
        SizeBytes < 1024 * 1024
            ? $"{SizeBytes / 1024.0:F1} KB"
            : $"{SizeBytes / 1024.0 / 1024.0:F1} MB";

    [Ignore]
    public string DateText =>
        CreatedAt.ToLocalTime().ToString("dd/MM/yyyy");

    [Ignore]
    public string PrivacyText =>
        IsPrivate
            ? "Privado"
            : "Compartible";

    [Ignore]
    public string PrivacyIcon =>
        IsPrivate
            ? "🔒"
            : "🌐";
}
