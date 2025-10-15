using Microsoft.AspNetCore.Mvc;
using Trivare.Application.DTOs.Common;

namespace Trivare.Api.Controllers.Utils;

public static class ControllerHelper
{
    public static ActionResult HandleResult<T>(this ControllerBase controller, Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return controller.StatusCode(successStatusCode, result.Value);
        }

        return result.Error?.Error switch
        {
            AuthErrorCodes.EmailAlreadyExists => controller.Conflict(result.Error),
            AuthErrorCodes.InvalidCredentials => controller.Unauthorized(result.Error),
            AuthErrorCodes.InvalidRefreshToken => controller.Unauthorized(result.Error),
            AuthErrorCodes.TokenNotFound => controller.NotFound(result.Error),
            AuthErrorCodes.TokenExpired => controller.BadRequest(result.Error),
            AuthErrorCodes.CurrentPasswordMismatch => controller.BadRequest(result.Error),
            AuthErrorCodes.SamePassword => controller.BadRequest(result.Error),
            TripErrorCodes.TripLimitExceeded => controller.Conflict(result.Error),
            _ => controller.BadRequest(result.Error)
        };
    }
}
