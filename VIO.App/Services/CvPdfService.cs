using Microsoft.Maui.Storage;
using SkiaSharp;
using VIO.App.Interfaces;
using VIO.App.Models;

namespace VIO.App.Services;

public class CvPdfService
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float Margin = 42f;

    public CvPdfService(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    public async Task<VioDocument> GenerateAsync()
    {
        var user =
            await _session.GetCurrentUserAsync()
            ?? throw new InvalidOperationException(
                "No hay una sesión activa.");

        int userId = user.Id;

        var profile =
            await _database.GetProfessionalProfileAsync(
                userId);

        var education =
            await _database.GetEducationAsync(
                userId);

        var experiences =
            await _database.GetExperiencesAsync(
                userId);

        var skills =
            await _database.GetSkillsAsync(
                userId);

        var certifications =
            await _database.GetCertificationsAsync(
                userId);

        string directory =
            Path.Combine(
                FileSystem.Current.AppDataDirectory,
                "VioDocuments",
                userId.ToString());

        Directory.CreateDirectory(directory);

        string storedFileName =
            $"{Guid.NewGuid():N}.pdf";

        string path =
            Path.Combine(
                directory,
                storedFileName);

        using var document =
            SKDocument.CreatePdf(path);

        SKCanvas? canvas = null;
        bool pageOpen = false;
        float y = 0;

        using var textPaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(
                    0x1D,
                    0x1D,
                    0x1F)
            };

        using var secondaryPaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(
                    0x6E,
                    0x6E,
                    0x73)
            };

        using var bluePaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(
                    0x0A,
                    0x84,
                    0xFF)
            };

        using var whitePaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White
            };

        using var linePaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(
                    0xE5,
                    0xE5,
                    0xEA),
                StrokeWidth = 1
            };

        using var nameFont =
            new SKFont(
                SKTypeface.Default,
                27);

        using var titleFont =
            new SKFont(
                SKTypeface.Default,
                15);

        using var sectionFont =
            new SKFont(
                SKTypeface.Default,
                15);

        using var bodyFont =
            new SKFont(
                SKTypeface.Default,
                11.5f);

        using var smallFont =
            new SKFont(
                SKTypeface.Default,
                9.5f);

        void EndCurrentPage()
        {
            if (!pageOpen)
                return;

            document.EndPage();
            pageOpen = false;
        }

        void NewPage(bool firstPage = false)
        {
            EndCurrentPage();

            canvas =
                document.BeginPage(
                    PageWidth,
                    PageHeight);

            pageOpen = true;

            canvas.Clear(SKColors.White);

            if (firstPage)
            {
                canvas.DrawRect(
                    0,
                    0,
                    PageWidth,
                    126,
                    bluePaint);

                canvas.DrawText(
                    user.Name,
                    Margin,
                    55,
                    SKTextAlign.Left,
                    nameFont,
                    whitePaint);

                string professionalTitle =
                    string.IsNullOrWhiteSpace(
                        profile?.ProfessionalTitle)
                        ? "Perfil profesional"
                        : profile.ProfessionalTitle;

                canvas.DrawText(
                    professionalTitle,
                    Margin,
                    82,
                    SKTextAlign.Left,
                    titleFont,
                    whitePaint);

                var contactParts =
                    new List<string>();

                if (!string.IsNullOrWhiteSpace(user.Email))
                    contactParts.Add(user.Email);

                if (!string.IsNullOrWhiteSpace(profile?.Phone))
                    contactParts.Add(profile.Phone);

                if (!string.IsNullOrWhiteSpace(profile?.City))
                    contactParts.Add(profile.City);

                canvas.DrawText(
                    string.Join(
                        "  ·  ",
                        contactParts),
                    Margin,
                    105,
                    SKTextAlign.Left,
                    smallFont,
                    whitePaint);

                y = 158;
            }
            else
            {
                canvas.DrawText(
                    "VIO · Curriculum Vitae",
                    Margin,
                    32,
                    SKTextAlign.Left,
                    smallFont,
                    secondaryPaint);

                canvas.DrawLine(
                    Margin,
                    43,
                    PageWidth - Margin,
                    43,
                    linePaint);

                y = 67;
            }
        }

        void EnsureSpace(float needed)
        {
            if (y + needed >
                PageHeight - 55)
            {
                NewPage();
            }
        }

        void Section(string title)
        {
            EnsureSpace(45);

            canvas!.DrawText(
                title.ToUpperInvariant(),
                Margin,
                y,
                SKTextAlign.Left,
                sectionFont,
                bluePaint);

            y += 9;

            canvas.DrawLine(
                Margin,
                y,
                PageWidth - Margin,
                y,
                linePaint);

            y += 22;
        }

        void Wrapped(
            string? text,
            SKFont font,
            SKPaint paint,
            float indentation = 0,
            float lineHeight = 16)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            float maxWidth =
                PageWidth -
                Margin * 2 -
                indentation;

            string[] words =
                text.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            string line =
                string.Empty;

            foreach (string word in words)
            {
                string test =
                    string.IsNullOrEmpty(line)
                        ? word
                        : $"{line} {word}";

                float width =
                    font.MeasureText(
                        test,
                        paint);

                if (width > maxWidth &&
                    !string.IsNullOrEmpty(line))
                {
                    EnsureSpace(lineHeight + 2);

                    canvas!.DrawText(
                        line,
                        Margin + indentation,
                        y,
                        SKTextAlign.Left,
                        font,
                        paint);

                    y += lineHeight;

                    line = word;
                }
                else
                {
                    line = test;
                }
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                EnsureSpace(lineHeight + 2);

                canvas!.DrawText(
                    line,
                    Margin + indentation,
                    y,
                    SKTextAlign.Left,
                    font,
                    paint);

                y += lineHeight;
            }
        }

        void ItemHeader(
            string main,
            string secondary)
        {
            EnsureSpace(42);

            canvas!.DrawText(
                main,
                Margin,
                y,
                SKTextAlign.Left,
                bodyFont,
                textPaint);

            y += 16;

            if (!string.IsNullOrWhiteSpace(secondary))
            {
                canvas.DrawText(
                    secondary,
                    Margin,
                    y,
                    SKTextAlign.Left,
                    smallFont,
                    secondaryPaint);

                y += 17;
            }
        }

        NewPage(
            firstPage: true);

        if (!string.IsNullOrWhiteSpace(
                profile?.Summary))
        {
            Section("Perfil profesional");

            Wrapped(
                profile.Summary,
                bodyFont,
                textPaint);

            y += 8;
        }

        if (!string.IsNullOrWhiteSpace(
                profile?.GitHub) ||
            !string.IsNullOrWhiteSpace(
                profile?.LinkedIn))
        {
            Section("Enlaces");

            if (!string.IsNullOrWhiteSpace(
                    profile.GitHub))
            {
                Wrapped(
                    $"GitHub: {profile.GitHub}",
                    bodyFont,
                    textPaint);
            }

            if (!string.IsNullOrWhiteSpace(
                    profile.LinkedIn))
            {
                Wrapped(
                    $"LinkedIn: {profile.LinkedIn}",
                    bodyFont,
                    textPaint);
            }

            y += 8;
        }

        if (education.Count > 0)
        {
            Section("Formación académica");

            foreach (var item in education)
            {
                string dates =
                    item.IsCurrent
                        ? $"{item.StartDate:yyyy} - Actual"
                        : $"{item.StartDate:yyyy} - {item.EndDate:yyyy}";

                ItemHeader(
                    item.Degree,
                    $"{item.Institution} · {dates}");

                Wrapped(
                    item.Description,
                    smallFont,
                    secondaryPaint);

                y += 10;
            }
        }

        if (experiences.Count > 0)
        {
            Section("Experiencia");

            foreach (var item in experiences)
            {
                string dates =
                    item.IsCurrent
                        ? $"{item.StartDate:MMM yyyy} - Actual"
                        : $"{item.StartDate:MMM yyyy} - {item.EndDate:MMM yyyy}";

                ItemHeader(
                    item.Position,
                    $"{item.Company} · {dates}");

                Wrapped(
                    item.Description,
                    smallFont,
                    secondaryPaint);

                y += 10;
            }
        }

        if (skills.Count > 0)
        {
            Section("Habilidades");

            foreach (var skill in skills)
            {
                Wrapped(
                    $"• {skill.Name} — {skill.Level}",
                    bodyFont,
                    textPaint);
            }

            y += 8;
        }

        if (certifications.Count > 0)
        {
            Section("Certificaciones");

            foreach (var cert in certifications)
            {
                string detail =
                    cert.DoesNotExpire
                        ? $"{cert.Issuer} · {cert.IssueDate:yyyy} · Sin vencimiento"
                        : $"{cert.Issuer} · {cert.IssueDate:yyyy}";

                ItemHeader(
                    cert.Name,
                    detail);

                if (!string.IsNullOrWhiteSpace(
                        cert.CredentialId))
                {
                    Wrapped(
                        $"Credencial: {cert.CredentialId}",
                        smallFont,
                        secondaryPaint);
                }

                y += 7;
            }
        }

        EnsureSpace(45);

        y += 10;

        canvas!.DrawLine(
            Margin,
            y,
            PageWidth - Margin,
            y,
            linePaint);

        y += 18;

        canvas.DrawText(
            $"Generado con VIO · {DateTime.Now:dd/MM/yyyy}",
            Margin,
            y,
            SKTextAlign.Left,
            smallFont,
            secondaryPaint);

        EndCurrentPage();

        document.Close();

        var info =
            new FileInfo(path);

        return new VioDocument
        {
            UserId =
                userId,

            DisplayName =
                $"CV - {user.Name}",

            OriginalFileName =
                $"CV - {user.Name}.pdf",

            StoredFileName =
                storedFileName,

            Extension =
                ".pdf",

            Category =
                "CV",

            SizeBytes =
                info.Length,

            CreatedAt =
                DateTime.UtcNow
        };
    }
}
