using SQLite;

namespace VIO.App.Models;

[Table("Certifications")]
public class Certification
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; } = DateTime.Today;

    public DateTime ExpirationDate { get; set; } = DateTime.Today;

    public bool DoesNotExpire { get; set; }

    public string CredentialId { get; set; } = string.Empty;

    public string CredentialUrl { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public int? RelatedDocumentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public string StatusText =>
        DoesNotExpire || ExpirationDate.Date >= DateTime.Today
            ? "Vigente"
            : "Vencida";

    [Ignore]
    public string IssueDateText =>
        $"Emitida: {IssueDate:dd/MM/yyyy}";

    [Ignore]
    public string ExpirationText =>
        DoesNotExpire
            ? "Sin vencimiento"
            : $"Vence: {ExpirationDate:dd/MM/yyyy}";

    [Ignore]
    public string RelatedDocumentName { get; set; } =
        "Sin evidencia vinculada";

    [Ignore]
    public bool HasRelatedDocument =>
        RelatedDocumentId.HasValue;
}
