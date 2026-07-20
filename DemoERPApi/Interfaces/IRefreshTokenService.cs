using DemoERPApi.Models;

namespace DemoERPApi.Interfaces;


/// Interface for refresh token management (generation, validation, revocation).

public interface IRefreshTokenService
{
    
    /// Generates a new refresh token for a user.
    
    RefreshToken GenerateRefreshToken(int userId, string ipAddress);

    
    /// Validates a refresh token.
    
    Task<bool> ValidateRefreshToken(string token);

    
    /// Revokes a refresh token.
    
    Task RevokeToken(string token);
}