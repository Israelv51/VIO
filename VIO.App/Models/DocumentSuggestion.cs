namespace VIO.App.Models;

public class DocumentSuggestion
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Group { get; set; } = "Base";

    public bool IsOfficialDocument { get; set; }

    public bool IsAvailable { get; set; }

    public int? MatchedDocumentId { get; set; }

    public string[] MatchTokens { get; set; } = [];

    public string StatusText =>
        IsAvailable
            ? "Listo"
            : "Agregar";

    public string StatusIcon =>
        IsAvailable
            ? "✓"
            : "+";
}
