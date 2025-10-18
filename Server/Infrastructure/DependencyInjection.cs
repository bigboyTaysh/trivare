using Microsoft.Extensions.DependencyInjection;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Repositories;
using Trivare.Infrastructure.Services;

namespace Trivare.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure layer repositories and services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<ITransportRepository, TransportRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAccommodationRepository, AccommodationRepository>();
        services.AddScoped<IDayRepository, DayRepository>();
        services.AddScoped<IPlaceRepository, PlaceRepository>();
        services.AddScoped<IDayAttractionRepository, DayAttractionRepository>();

        // Register services
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
