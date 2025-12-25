using Amazon.DynamoDBv2.DataModel;

namespace UrlShortener.Models
{
    [DynamoDBTable("Users")]
    public class User
    {
        [DynamoDBHashKey]
        public string UserId { get; set; } = null!;

        [DynamoDBProperty("Username")]
        public string Username { get; set; } = null!;

        [DynamoDBProperty]
        public string PasswordHash { get; set; } = null!;

        //[DynamoDBGlobalSecondaryIndexHashKey("EmailIndex")]
        [DynamoDBProperty]
        public string Email { get; set; } = null!;

        [DynamoDBProperty("Role")]
        public string Role { get; set; } = "User";

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty("IsActive")]
        public bool IsActive { get; set; } = true;
    }

}
