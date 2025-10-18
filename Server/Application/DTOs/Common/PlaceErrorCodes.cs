namespace Trivare.Application.DTOs.Common;

public static class PlaceErrorCodes
{
    public const string PlaceSearchFailed = "PlaceSearchFailed";
    public const string InvalidSearchParameters = "InvalidSearchParameters";
    public const string GooglePlacesApiError = "GooglePlacesApiError";
    public const string OpenRouterApiError = "OpenRouterApiError";
    public const string ExternalApiTimeout = "ExternalApiTimeout";

    // Add place to day error codes
    public const string DayNotFound = "DayNotFound";
    public const string DayNotOwned = "DayNotOwned";
    public const string PlaceNotFound = "PlaceNotFound";
    public const string InvalidPlaceData = "InvalidPlaceData";
    public const string PlaceAlreadyAdded = "PlaceAlreadyAdded";
    public const string InvalidOrder = "InvalidOrder";
    public const string InternalServerError = "InternalServerError";

    // Update place on day error codes
    public const string DayAttractionNotFound = "DayAttractionNotFound";
    public const string NoFieldsToUpdate = "NoFieldsToUpdate";
}
