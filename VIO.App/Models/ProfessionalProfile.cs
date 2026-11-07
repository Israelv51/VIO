using SQLite;

namespace VIO.App.Models;

[Table("ProfessionalProfiles")]
public class ProfessionalProfile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    public string ProfessionalTitle { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string LinkedIn { get; set; } = string.Empty;

    public string GitHub { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
