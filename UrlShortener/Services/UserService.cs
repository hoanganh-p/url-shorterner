using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using UrlShortener.Models;
using UrlShortener.Services.Interfaces;

namespace UrlShortener.Services
{
    public class UserService : IUserService
    {
        private readonly DynamoDBContext _context;

        public UserService(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var search = _context.ScanAsync<User>(new[]
            {
                new ScanCondition("Username", ScanOperator.Equal, username)
            });

            var users = await search.GetNextSetAsync();
            return users.FirstOrDefault();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.LoadAsync<User>(userId);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                // Check if username already exists
                var existingUser = await GetUserByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    return false;
                }

                user.UserId = Guid.NewGuid().ToString();
                user.CreatedAt = DateTime.UtcNow;
                user.PasswordHash = HashPassword(user.PasswordHash); // Hash password before saving

                await _context.SaveAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);

            if (user == null || !user.IsActive)
            {
                return false;
            }

            return VerifyPassword(password, user.PasswordHash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
