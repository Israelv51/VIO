using System.Net.Mail;
using SQLite;
using VIO.App.Helpers;
using VIO.App.Interfaces;
using VIO.App.Models;

namespace VIO.App.Services;

public sealed class AuthService : IAuthService
{
    private readonly VioDatabase _database;
    private readonly ISessionService _sessionService;

    public AuthService(
        VioDatabase database,
        ISessionService sessionService)
    {
        _database = database;
        _sessionService = sessionService;
    }

    public async Task<AuthResult> RegisterAsync(
        string name,
        string email,
        string password)
    {
        name = name?.Trim() ?? string.Empty;

        email = email?
            .Trim()
            .ToLowerInvariant() ?? string.Empty;

        if (name.Length < 2)
        {
            return new AuthResult(
                false,
                "Ingresa tu nombre.");
        }

        if (!IsValidEmail(email))
        {
            return new AuthResult(
                false,
                "Ingresa un correo electrónico válido.");
        }

        string? passwordError =
            ValidatePassword(password);

        if (passwordError is not null)
        {
            return new AuthResult(
                false,
                passwordError);
        }

        User? existingUser =
            await _database.GetUserByEmailAsync(email);

        if (existingUser is not null)
        {
            return new AuthResult(
                false,
                "Ya existe una cuenta con ese correo.");
        }

        var passwordData =
            PasswordHasher.HashPassword(password);

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordData.Hash,
            PasswordSalt = passwordData.Salt,
            PasswordIterations = PasswordHasher.Iterations,
            PasswordAlgorithm = "PBKDF2-HMAC-SHA256",
            IsActive = true
        };

        try
        {
            await _database.AddUserAsync(user);

            _sessionService.Start(user.Id);

            return new AuthResult(
                true,
                "Cuenta creada correctamente.",
                user.Id);
        }
        catch (SQLiteException)
        {
            return new AuthResult(
                false,
                "No fue posible crear la cuenta.");
        }
    }

    public async Task<AuthResult> LoginAsync(
        string email,
        string password)
    {
        email = email?
            .Trim()
            .ToLowerInvariant() ?? string.Empty;

        if (!IsValidEmail(email))
        {
            return new AuthResult(
                false,
                "Ingresa un correo electrónico válido.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult(
                false,
                "Ingresa tu contraseña.");
        }

        User? user =
            await _database.GetUserByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return new AuthResult(
                false,
                "Correo o contraseña incorrectos.");
        }

        bool passwordIsValid =
            PasswordHasher.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt,
                user.PasswordIterations);

        if (!passwordIsValid)
        {
            return new AuthResult(
                false,
                "Correo o contraseña incorrectos.");
        }

        _sessionService.Start(user.Id);

        return new AuthResult(
            true,
            "Sesión iniciada.",
            user.Id);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var address = new MailAddress(email);

            return address.Address.Equals(
                email,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string? ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Ingresa una contraseña.";

        if (password.Length < 8)
            return "La contraseña debe tener al menos 8 caracteres.";

        if (!password.Any(char.IsUpper))
            return "La contraseña necesita una letra mayúscula.";

        if (!password.Any(char.IsLower))
            return "La contraseña necesita una letra minúscula.";

        if (!password.Any(char.IsDigit))
            return "La contraseña necesita al menos un número.";

        return null;
    }
}
