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

    private readonly SKColor _blue =
        new(10, 132, 255);

    private readonly SKColor _deepBlue =
        new(10, 65, 180);

    private readonly SKColor _navy =
        new(23, 35, 61);

    private readonly SKColor _text =
        new(29, 29, 31);

    private readonly SKColor _secondary =
        new(110, 110, 115);

    private readonly SKColor _border =
        new(229, 229, 234);

    private readonly SKColor _softBlue =
        new(234, 244, 255);

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

        int userId =
            user.Id;

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

        Directory.CreateDirectory(
            directory);

        string storedFileName =
            $"{Guid.NewGuid():N}.pdf";

        string path =
            Path.Combine(
                directory,
                storedFileName);

        using var document =
            SKDocument.CreatePdf(path);

        using var regularTypeface =
            SKTypeface.FromFamilyName(
                null,
                SKFontStyle.Normal);

        using var boldTypeface =
            SKTypeface.FromFamilyName(
                null,
                SKFontStyle.Bold);

        using var blackPaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = _text
            };

        using var grayPaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = _secondary
            };

        using var bluePaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = _blue
            };

        using var navyPaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = _navy
            };

        using var whitePaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White
            };

        using var borderPaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = _border,
                StrokeWidth = 1
            };

        using var softBluePaint =
            new SKPaint
            {
                IsAntialias = true,
                Color = _softBlue
            };

        using var nameFont =
            new SKFont(
                boldTypeface,
                26);

        using var titleFont =
            new SKFont(
                regularTypeface,
                13.5f);

        using var sectionFont =
            new SKFont(
                boldTypeface,
                14);

        using var bodyFont =
            new SKFont(
                regularTypeface,
                10.5f);

        using var bodyBoldFont =
            new SKFont(
                boldTypeface,
                10.7f);

        using var smallFont =
            new SKFont(
                regularTypeface,
                8.8f);

        using var smallBoldFont =
            new SKFont(
                boldTypeface,
                8.8f);

        using var chipFont =
            new SKFont(
                boldTypeface,
                8.2f);

        SKCanvas? canvas =
            null;

        bool pageOpen =
            false;

        float mainX =
            42;

        float mainWidth =
            337;

        float mainY =
            207;

        const float sidebarX =
            397;

        const float sidebarWidth =
            156;

        void DrawFooter()
        {
            if (canvas is null)
                return;

            canvas.DrawLine(
                42,
                810,
                553,
                810,
                borderPaint);

            canvas.DrawText(
                $"Generado con VIO · {DateTime.Now:dd/MM/yyyy}",
                42,
                826,
                SKTextAlign.Left,
                smallFont,
                grayPaint);

            canvas.DrawText(
                "Verified Identity & Opportunities",
                553,
                826,
                SKTextAlign.Right,
                smallFont,
                grayPaint);
        }

        void ClosePage()
        {
            if (!pageOpen)
                return;

            DrawFooter();

            document.EndPage();

            pageOpen =
                false;
        }

        void DrawHeader()
        {
            using var headerPaint =
                new SKPaint
                {
                    IsAntialias = true
                };

            using var gradient =
                SKShader.CreateLinearGradient(
                    new SKPoint(
                        0,
                        0),
                    new SKPoint(
                        PageWidth,
                        180),
                    new[]
                    {
                        _deepBlue,
                        _blue
                    },
                    null,
                    SKShaderTileMode.Clamp);

            headerPaint.Shader =
                gradient;

            canvas!.DrawRect(
                0,
                0,
                PageWidth,
                180,
                headerPaint);

            using var decorativePaint =
                new SKPaint
                {
                    IsAntialias = true,
                    Color =
                        new SKColor(
                            255,
                            255,
                            255,
                            25)
                };

            canvas.DrawCircle(
                545,
                40,
                120,
                decorativePaint);

            canvas.DrawCircle(
                500,
                150,
                105,
                decorativePaint);

            var photoRect =
                new SKRect(
                    42,
                    38,
                    137,
                    145);

            if (!string.IsNullOrWhiteSpace(
                    user.ProfilePhotoPath) &&
                File.Exists(
                    user.ProfilePhotoPath))
            {
                using var stream =
                    File.OpenRead(
                        user.ProfilePhotoPath);

                using var bitmap =
                    SKBitmap.Decode(
                        stream);

                if (bitmap is not null)
                {
                    float sourceRatio =
                        bitmap.Width /
                        (float)bitmap.Height;

                    float targetRatio =
                        photoRect.Width /
                        photoRect.Height;

                    SKRect sourceRect;

                    if (sourceRatio >
                        targetRatio)
                    {
                        float desiredWidth =
                            bitmap.Height *
                            targetRatio;

                        float left =
                            (bitmap.Width -
                             desiredWidth) / 2f;

                        sourceRect =
                            new SKRect(
                                left,
                                0,
                                left + desiredWidth,
                                bitmap.Height);
                    }
                    else
                    {
                        float desiredHeight =
                            bitmap.Width /
                            targetRatio;

                        float top =
                            (bitmap.Height -
                             desiredHeight) / 2f;

                        sourceRect =
                            new SKRect(
                                0,
                                top,
                                bitmap.Width,
                                top + desiredHeight);
                    }

                    int save =
                        canvas.Save();

                    using var roundRect =
                        new SKRoundRect(
                            photoRect,
                            14,
                            14);

                    canvas.ClipRoundRect(
                        roundRect,
                        SKClipOperation.Intersect,
                        true);

                    var sampling =
                        new SKSamplingOptions(
                            SKFilterMode.Linear);

                    canvas.DrawBitmap(
                        bitmap,
                        sourceRect,
                        photoRect,
                        sampling);

                    canvas.RestoreToCount(
                        save);

                    using var photoBorder =
                        new SKPaint
                        {
                            IsAntialias = true,
                            Style =
                                SKPaintStyle.Stroke,
                            StrokeWidth = 3,
                            Color =
                                new SKColor(
                                    255,
                                    255,
                                    255,
                                    220)
                        };

                    canvas.DrawRoundRect(
                        photoRect,
                        14,
                        14,
                        photoBorder);
                }
            }
            else
            {
                using var placeholder =
                    new SKPaint
                    {
                        IsAntialias = true,
                        Color =
                            new SKColor(
                                255,
                                255,
                                255,
                                35)
                    };

                canvas.DrawRoundRect(
                    photoRect,
                    14,
                    14,
                    placeholder);

                using var initialsFont =
                    new SKFont(
                        boldTypeface,
                        24);

                canvas.DrawText(
                    GetInitials(
                        user.Name),
                    photoRect.MidX,
                    photoRect.MidY + 8,
                    SKTextAlign.Center,
                    initialsFont,
                    whitePaint);
            }

            float textX =
                158;

            canvas.DrawText(
                user.Name,
                textX,
                69,
                SKTextAlign.Left,
                nameFont,
                whitePaint);

            string professionalTitle =
                string.IsNullOrWhiteSpace(
                    profile?.ProfessionalTitle)
                    ? "Profesional VIO"
                    : profile.ProfessionalTitle;

            canvas.DrawText(
                professionalTitle,
                textX,
                95,
                SKTextAlign.Left,
                titleFont,
                whitePaint);

            var contact =
                new List<string>();

            if (!string.IsNullOrWhiteSpace(
                    user.Email))
            {
                contact.Add(
                    user.Email);
            }

            if (!string.IsNullOrWhiteSpace(
                    profile?.Phone))
            {
                contact.Add(
                    profile.Phone);
            }

            if (!string.IsNullOrWhiteSpace(
                    profile?.City))
            {
                contact.Add(
                    profile.City);
            }

            canvas.DrawText(
                string.Join(
                    "  ·  ",
                    contact),
                textX,
                121,
                SKTextAlign.Left,
                smallFont,
                whitePaint);
        }

        void StartFirstPage()
        {
            ClosePage();

            canvas =
                document.BeginPage(
                    PageWidth,
                    PageHeight);

            pageOpen =
                true;

            canvas.Clear(
                SKColors.White);

            DrawHeader();

            mainX =
                42;

            mainWidth =
                337;

            mainY =
                212;
        }

        void StartContinuationPage()
        {
            ClosePage();

            canvas =
                document.BeginPage(
                    PageWidth,
                    PageHeight);

            pageOpen =
                true;

            canvas.Clear(
                SKColors.White);

            using var topPaint =
                new SKPaint
                {
                    IsAntialias = true,
                    Color = _navy
                };

            canvas.DrawRoundRect(
                new SKRect(
                    32,
                    28,
                    563,
                    68),
                14,
                14,
                topPaint);

            canvas.DrawText(
                user.Name,
                50,
                53,
                SKTextAlign.Left,
                smallBoldFont,
                whitePaint);

            canvas.DrawText(
                "Curriculum Vitae",
                545,
                53,
                SKTextAlign.Right,
                smallFont,
                whitePaint);

            mainX =
                42;

            mainWidth =
                511;

            mainY =
                96;
        }

        void EnsureMainSpace(
            float needed)
        {
            if (mainY + needed >
                790)
            {
                StartContinuationPage();
            }
        }

        void DrawSection(
            string title)
        {
            EnsureMainSpace(
                38);

            canvas!.DrawRoundRect(
                new SKRect(
                    mainX,
                    mainY - 14,
                    mainX + 27,
                    mainY + 10),
                7,
                7,
                softBluePaint);

            canvas.DrawCircle(
                mainX + 13.5f,
                mainY - 2,
                4,
                bluePaint);

            canvas.DrawText(
                title,
                mainX + 36,
                mainY + 4,
                SKTextAlign.Left,
                sectionFont,
                navyPaint);

            mainY +=
                17;

            canvas.DrawLine(
                mainX,
                mainY,
                mainX + mainWidth,
                mainY,
                borderPaint);

            mainY +=
                17;
        }

        void DrawWrappedMain(
            string? value,
            SKFont font,
            SKPaint paint,
            float lineHeight = 14)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return;
            }

            var lines =
                WrapText(
                    value,
                    font,
                    paint,
                    mainWidth);

            foreach (string line in lines)
            {
                EnsureMainSpace(
                    lineHeight + 2);

                canvas!.DrawText(
                    line,
                    mainX,
                    mainY,
                    SKTextAlign.Left,
                    font,
                    paint);

                mainY +=
                    lineHeight;
            }
        }

        void DrawSidebarTitle(
            string title,
            ref float y)
        {
            canvas!.DrawText(
                title,
                sidebarX,
                y,
                SKTextAlign.Left,
                bodyBoldFont,
                navyPaint);

            y +=
                10;

            canvas.DrawLine(
                sidebarX,
                y,
                sidebarX + sidebarWidth,
                y,
                borderPaint);

            y +=
                17;
        }

        StartFirstPage();

        // =========================
        // PERFIL
        // =========================

        if (!string.IsNullOrWhiteSpace(
                profile?.Summary))
        {
            DrawSection(
                "Perfil profesional");

            DrawWrappedMain(
                profile.Summary,
                bodyFont,
                blackPaint,
                14);

            mainY +=
                11;
        }

        // =========================
        // EXPERIENCIA
        // =========================

        if (experiences.Count > 0)
        {
            DrawSection(
                "Experiencia");

            foreach (var item in experiences)
            {
                EnsureMainSpace(
                    55);

                canvas!.DrawCircle(
                    mainX + 4,
                    mainY - 4,
                    3,
                    bluePaint);

                canvas.DrawText(
                    item.Position,
                    mainX + 14,
                    mainY,
                    SKTextAlign.Left,
                    bodyBoldFont,
                    blackPaint);

                mainY +=
                    15;

                string dates =
                    item.IsCurrent
                        ? $"{item.Company} · {item.StartDate:MMM yyyy} – Actual"
                        : $"{item.Company} · {item.StartDate:MMM yyyy} – {item.EndDate:MMM yyyy}";

                canvas.DrawText(
                    dates,
                    mainX + 14,
                    mainY,
                    SKTextAlign.Left,
                    smallFont,
                    grayPaint);

                mainY +=
                    15;

                if (!string.IsNullOrWhiteSpace(
                        item.Description))
                {
                    var lines =
                        WrapText(
                            item.Description,
                            smallFont,
                            grayPaint,
                            mainWidth - 14);

                    foreach (string line in lines)
                    {
                        EnsureMainSpace(
                            12);

                        canvas.DrawText(
                            line,
                            mainX + 14,
                            mainY,
                            SKTextAlign.Left,
                            smallFont,
                            grayPaint);

                        mainY +=
                            12;
                    }
                }

                mainY +=
                    11;
            }
        }

        // =========================
        // FORMACIÓN
        // =========================

        if (education.Count > 0)
        {
            DrawSection(
                "Formación académica");

            foreach (var item in education)
            {
                EnsureMainSpace(
                    45);

                canvas!.DrawText(
                    item.Degree,
                    mainX,
                    mainY,
                    SKTextAlign.Left,
                    bodyBoldFont,
                    blackPaint);

                mainY +=
                    15;

                string dates =
                    item.IsCurrent
                        ? $"{item.Institution} · {item.StartDate:yyyy} – Actual"
                        : $"{item.Institution} · {item.StartDate:yyyy} – {item.EndDate:yyyy}";

                canvas.DrawText(
                    dates,
                    mainX,
                    mainY,
                    SKTextAlign.Left,
                    smallFont,
                    grayPaint);

                mainY +=
                    15;

                DrawWrappedMain(
                    item.Description,
                    smallFont,
                    grayPaint,
                    12);

                mainY +=
                    9;
            }
        }

        // =========================
        // SIDEBAR
        // =========================

        float sideY =
            214;

        DrawSidebarTitle(
            "Contacto",
            ref sideY);

        DrawSidebarLine(
            user.Email,
            ref sideY);

        DrawSidebarLine(
            profile?.Phone,
            ref sideY);

        DrawSidebarLine(
            profile?.City,
            ref sideY);

        sideY +=
            11;

        if (!string.IsNullOrWhiteSpace(
                profile?.GitHub) ||
            !string.IsNullOrWhiteSpace(
                profile?.LinkedIn))
        {
            DrawSidebarTitle(
                "Enlaces",
                ref sideY);

            if (!string.IsNullOrWhiteSpace(
                    profile?.GitHub))
            {
                DrawSidebarLabel(
                    "GitHub",
                    profile.GitHub,
                    ref sideY);
            }

            if (!string.IsNullOrWhiteSpace(
                    profile?.LinkedIn))
            {
                DrawSidebarLabel(
                    "LinkedIn",
                    profile.LinkedIn,
                    ref sideY);
            }

            sideY +=
                9;
        }

        if (skills.Count > 0)
        {
            DrawSidebarTitle(
                "Habilidades",
                ref sideY);

            float chipX =
                sidebarX;

            float chipY =
                sideY;

            foreach (var skill in
                     skills.Take(12))
            {
                string chipText =
                    skill.Name;

                float textWidth =
                    chipFont.MeasureText(
                        chipText,
                        bluePaint);

                float chipWidth =
                    Math.Min(
                        sidebarWidth,
                        textWidth + 18);

                if (chipX +
                    chipWidth >
                    sidebarX +
                    sidebarWidth)
                {
                    chipX =
                        sidebarX;

                    chipY +=
                        27;
                }

                canvas!.DrawRoundRect(
                    new SKRect(
                        chipX,
                        chipY - 14,
                        chipX + chipWidth,
                        chipY + 7),
                    10,
                    10,
                    softBluePaint);

                canvas.DrawText(
                    chipText,
                    chipX + 9,
                    chipY,
                    SKTextAlign.Left,
                    chipFont,
                    bluePaint);

                chipX +=
                    chipWidth + 6;
            }

            sideY =
                chipY + 27;
        }

        if (certifications.Count > 0 &&
            sideY < 720)
        {
            DrawSidebarTitle(
                "Certificaciones",
                ref sideY);

            foreach (var cert in
                     certifications.Take(5))
            {
                var nameLines =
                    WrapText(
                        cert.Name,
                        smallBoldFont,
                        blackPaint,
                        sidebarWidth);

                foreach (string line in nameLines)
                {
                    canvas!.DrawText(
                        line,
                        sidebarX,
                        sideY,
                        SKTextAlign.Left,
                        smallBoldFont,
                        blackPaint);

                    sideY +=
                        11;
                }

                canvas!.DrawText(
                    $"{cert.Issuer} · {cert.IssueDate:yyyy}",
                    sidebarX,
                    sideY,
                    SKTextAlign.Left,
                    smallFont,
                    grayPaint);

                sideY +=
                    18;
            }
        }

        ClosePage();

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

        // =========================
        // HELPERS LOCALES
        // =========================

        void DrawSidebarLine(
            string? value,
            ref float y)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return;
            }

            foreach (string line in
                     WrapText(
                         value,
                         smallFont,
                         grayPaint,
                         sidebarWidth))
            {
                canvas!.DrawText(
                    line,
                    sidebarX,
                    y,
                    SKTextAlign.Left,
                    smallFont,
                    grayPaint);

                y +=
                    12;
            }

            y +=
                2;
        }

        void DrawSidebarLabel(
            string label,
            string value,
            ref float y)
        {
            canvas!.DrawText(
                label,
                sidebarX,
                y,
                SKTextAlign.Left,
                smallBoldFont,
                blackPaint);

            y +=
                11;

            foreach (string line in
                     WrapText(
                         value,
                         smallFont,
                         grayPaint,
                         sidebarWidth))
            {
                canvas.DrawText(
                    line,
                    sidebarX,
                    y,
                    SKTextAlign.Left,
                    smallFont,
                    grayPaint);

                y +=
                    11;
            }

            y +=
                5;
        }
    }

    private static List<string> WrapText(
        string? text,
        SKFont font,
        SKPaint paint,
        float maxWidth)
    {
        var lines =
            new List<string>();

        if (string.IsNullOrWhiteSpace(
                text))
        {
            return lines;
        }

        var words =
            text.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        string current =
            string.Empty;

        foreach (string word in words)
        {
            string candidate =
                string.IsNullOrEmpty(current)
                    ? word
                    : $"{current} {word}";

            float width =
                font.MeasureText(
                    candidate,
                    paint);

            if (width > maxWidth &&
                !string.IsNullOrEmpty(current))
            {
                lines.Add(current);

                current =
                    word;
            }
            else
            {
                current =
                    candidate;
            }
        }

        if (!string.IsNullOrWhiteSpace(
                current))
        {
            lines.Add(current);
        }

        return lines;
    }

    private static void DrawWrapped(
        string text,
        float x,
        float width,
        float y,
        SKFont font,
        SKPaint paint,
        float lineHeight,
        int maxLines)
    {
        // Se dibuja desde el encabezado mediante DrawText directo
        // cuando el contenido cabe en una línea.
    }

    private static string GetInitials(
        string name)
    {
        var parts =
            name.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return "VIO";

        if (parts.Length == 1)
        {
            return parts[0]
                .Substring(
                    0,
                    Math.Min(
                        2,
                        parts[0].Length))
                .ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[1][0]}"
            .ToUpperInvariant();
    }
}



