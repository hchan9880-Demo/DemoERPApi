using System.Security.Cryptography;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;
namespace DemoERPApi.Services;

public class RefreshTokenService : IRefreshTokenService
{
    public RefreshToken GenerateRefreshToken(int userId, string ip)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            CreatedByIP = ip,
            IsUsed = false
        };
    }

    public Task<bool> ValidateRefreshToken(string token)
    {
        return Task.FromResult(true);
    }

    public Task RevokeToken(string token)
    {
        return Task.CompletedTask;
    }

}
