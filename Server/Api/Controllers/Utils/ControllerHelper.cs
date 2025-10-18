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
            // BadRequest errors
            AuthErrorCodes.TokenExpired or
            AuthErrorCodes.CurrentPasswordMismatch or
            AuthErrorCodes.SamePassword or
            TripErrorCodes.TripInvalidDateRange or
            TripErrorCodes.InvalidTripData or
            AccommodationErrorCodes.AccommodationInvalidDateRange or
            DayErrorCodes.InvalidDate 
                => controller.BadRequest(result.Error),

            // Conflict errors
            AuthErrorCodes.EmailAlreadyExists or
            TripErrorCodes.TripLimitExceeded or
            DayErrorCodes.DayConflict 
                => controller.Conflict(result.Error),

            // Unauthorized errors
            AuthErrorCodes.InvalidCredentials or
            AuthErrorCodes.InvalidRefreshToken 
                => controller.Unauthorized(result.Error),

            // NotFound errors
            AuthErrorCodes.TokenNotFound or
            TripErrorCodes.TripNotFound or
            AccommodationErrorCodes.AccommodationNotFound or
            UserErrorCodes.UserNotFound or
            DayErrorCodes.DayNotFound 
                => controller.NotFound(result.Error),

            // Forbidden errors
            TripErrorCodes.TripNotOwned or
            UserErrorCodes.UnauthorizedAccess or
            DayErrorCodes.DayForbidden 
                => controller.StatusCode(StatusCodes.Status403Forbidden, result.Error),

            // Internal Server Error
            AuthErrorCodes.InternalServerError 
                => controller.StatusCode(StatusCodes.Status500InternalServerError, result.Error),

            _ => controller.BadRequest(result.Error)
        };
    }
}
