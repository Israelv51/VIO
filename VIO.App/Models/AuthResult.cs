namespace VIO.App.Models;

public sealed record AuthResult(
    bool Success,
    string Message,
    int? UserId = null);
