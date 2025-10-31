using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Repositories;
using Trivare.Infrastructure.Services;
using Trivare.Infrastructure.Settings;

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
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddScoped<IFileRepository, FileRepository>();

        // Register services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGooglePlacesService, GooglePlacesService>();

        // Configure HttpClient factory
        services.AddHttpClient();

        // Configure OpenRouter HTTP Client with timeout
        services.AddHttpClient("OpenRouterClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(120); // Set reasonable timeout for LLM requests
        });

        // Register OpenRouter service
        services.AddScoped<IOpenRouterService, OpenRouterService>();

        // Configure settings
        services.Configure<GooglePlacesSettings>(configuration.GetSection("GooglePlaces"));
        services.Configure<OpenRouterSettings>(configuration.GetSection("OpenRouter"));

        // Configure Cloudflare R2 (S3-compatible storage)
        services.Configure<CloudflareR2Settings>(configuration.GetSection("CloudflareR2"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var r2Section = configuration.GetSection("CloudflareR2");
            var accountId = r2Section["AccountId"];
            var accessKeyId = r2Section["AccessKeyId"];
            var secretAccessKey = r2Section["SecretAccessKey"];

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                // Force Signature Version 4 for Cloudflare R2 compatibility
                SignatureVersion = "4",
                // Set region to auto for R2
                AuthenticationRegion = "auto"
            };

            var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
            return new AmazonS3Client(credentials, config);
        });

        services.AddScoped<IFileStorageService, CloudflareR2FileStorageService>();

        return services;
    }
}
