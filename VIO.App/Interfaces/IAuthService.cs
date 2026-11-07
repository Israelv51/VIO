using VIO.App.Models;

namespace VIO.App.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(
        string name,
        string email,
        string password);

    Task<AuthResult> LoginAsync(
        string email,
        string password);
}
