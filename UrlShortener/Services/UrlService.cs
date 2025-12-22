using System;
using System.Security.Cryptography;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using UrlShortener.Models;

namespace UrlShortener.Services;

public class UrlService : IUrlService
{
    private readonly IDynamoDBContext _db;
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private static readonly char[] Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private const int ShortCodeLength = 7;
    private readonly ILogger<UrlService> _logger;

    public UrlService(IAmazonDynamoDB dynamoDb, IConfiguration configuration, ILogger<UrlService> logger)
    {
        _dynamoDb = dynamoDb;
        _db = new DynamoDBContext(dynamoDb);
        _tableName = "UrlMappings";
        _logger = logger;
    }

    private static string GenerateShortCode()
    {
        Span<byte> buffer = stackalloc byte[ShortCodeLength];
        RandomNumberGenerator.Fill(buffer);
        var sb = new StringBuilder(ShortCodeLength);
        for (int i = 0; i < ShortCodeLength; i++)
        {
            var idx = buffer[i] % Base62Chars.Length;
            sb.Append(Base62Chars[idx]);
        }

        return sb.ToString();
    }

    public async Task<UrlMapping> CreateAsync(string originalUrl)
    {
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uriResult))
            throw new ArgumentException("Invalid URL format", nameof(originalUrl));

        originalUrl = originalUrl.Trim();

            var now = DateTime.UtcNow;
            var code = GenerateShortCode();

        var mapping = new UrlMapping
        {
            ShortCode = code,
            OriginalUrl = originalUrl,
            CreatedAt = now
        };

        try
        {
            await _db.SaveAsync(mapping);
            return mapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving URL mapping");
            throw;
        }
    }

    public Task<UrlMapping?> GetAsync(string shortCode) =>
        _db.LoadAsync<UrlMapping>(shortCode);
}