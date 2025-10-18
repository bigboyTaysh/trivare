using Microsoft.Extensions.DependencyInjection;
using Trivare.Application.Interfaces;
using Trivare.Application.Services;

namespace Trivare.Application;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<ITransportService, TransportService>();
        services.AddScoped<IAccommodationService, AccommodationService>();
        services.AddScoped<IDayService, DayService>();
        services.AddScoped<IPlacesService, PlacesService>();
        services.AddScoped<IFileService, FileService>();
        services.AddSingleton<IPasswordHashingService, PasswordHashingService>();

        return services;
    }
}
