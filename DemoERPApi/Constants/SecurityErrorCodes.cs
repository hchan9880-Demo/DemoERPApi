namespace DemoERPApi.Constants;


/// Security-related error codes for authentication and authorization.

public static class SecurityErrorCodes
{
    public const string InvalidToken = "AUTH_INVALID_TOKEN";
    public const string ExpiredToken = "AUTH_EXPIRED_TOKEN";
    public const string RefreshTokenInvalid = "AUTH_REFRESH_TOKEN_INVALID";
    public const string RefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
    public const string RefreshTokenRevoked = "AUTH_REFRESH_TOKEN_REVOKED";
    public const string UserDisabled = "AUTH_USER_DISABLED";
    public const string InsufficientPermission = "AUTH_INSUFFICIENT_PERMISSION";
}