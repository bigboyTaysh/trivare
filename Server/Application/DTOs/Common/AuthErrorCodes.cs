namespace Trivare.Application.DTOs.Common;

public static class AuthErrorCodes
{
    public const string EmailAlreadyExists = "EmailAlreadyExists";
    public const string InvalidCredentials = "InvalidCredentials";
    public const string InvalidRefreshToken = "InvalidRefreshToken";
    public const string TokenNotFound = "TokenNotFound";
    public const string TokenExpired = "TokenExpired";
    public const string CurrentPasswordMismatch = "CurrentPasswordMismatch";
    public const string SamePassword = "SamePassword";
    public const string InternalServerError = "InternalServerError";
}
