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
            DayErrorCodes.InvalidDate or
            PlaceErrorCodes.InvalidSearchParameters or
            FileErrorCodes.FileInvalidType or
            FileErrorCodes.FileTooLarge
                => controller.BadRequest(result.Error),

            // Conflict errors
            AuthErrorCodes.EmailAlreadyExists or
            TripErrorCodes.TripLimitExceeded or
            DayErrorCodes.DayConflict or
            FileErrorCodes.FileLimitExceeded
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
            DayErrorCodes.DayNotFound or
            FileErrorCodes.FileNotFound
                => controller.NotFound(result.Error),

            // Forbidden errors
            TripErrorCodes.TripNotOwned or
            UserErrorCodes.UnauthorizedAccess or
            DayErrorCodes.DayForbidden 
                => controller.StatusCode(StatusCodes.Status403Forbidden, result.Error),

            // Internal Server Error
            AuthErrorCodes.InternalServerError or
            PlaceErrorCodes.PlaceSearchFailed or
            PlaceErrorCodes.GooglePlacesApiError or
            PlaceErrorCodes.OpenRouterApiError or
            PlaceErrorCodes.ExternalApiTimeout or
            FileErrorCodes.FileUploadFailed or
            FileErrorCodes.FileSaveFailed
                => controller.StatusCode(StatusCodes.Status500InternalServerError, result.Error),

            _ => controller.BadRequest(result.Error)
        };
    }
}
