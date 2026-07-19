using DemoERPApi.Models;
namespace DemoERPApi.Interfaces;

public interface IRefreshTokenService
{

    RefreshToken GenerateRefreshToken(
        int userId,
        string ipAddress
    );


    Task<bool> ValidateRefreshToken(
        string token
    );


    Task RevokeToken(
        string token
    );

}
