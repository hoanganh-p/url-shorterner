using UrlShortener.Models;

namespace UrlShortener.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(string userId);
        Task<bool> CreateUserAsync(User user);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
