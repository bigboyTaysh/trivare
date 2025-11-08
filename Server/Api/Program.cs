using Microsoft.EntityFrameworkCore;
using Trivare.Application;
using Trivare.Infrastructure;
using Trivare.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Trivare.Infrastructure.Settings;
using Trivare.Api.Interceptors;
using Trivare.Api.Middleware;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using DotNetEnv;

// Load environment variables from .env file in development
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == "Development" || environment == "Testing")
{
    // Load .env file from the root directory (parent of Server directory)
    var rootPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");
    var envFilePath = Path.Combine(rootPath, ".env");
    Env.Load(envFilePath);
}

var builder = WebApplication.CreateBuilder(args);

// Set URLs based on environment
if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}   

builder.Services.AddHttpContextAccessor();

// Register the RLS interceptor
builder.Services.AddScoped<RlsSessionContextInterceptor>();

// Add services
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    // Get connection string from environment variables
    var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
        ? Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
        : Environment.GetEnvironmentVariable("CONNECTION_STRING");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string not configured. Please set CONNECTION_STRING or TEST_CONNECTION_STRING environment variable.");
    }

    options.UseSqlServer(connectionString)
           .AddInterceptors(serviceProvider.GetRequiredService<RlsSessionContextInterceptor>());
});

// Register layer services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customize automatic model validation response to match ErrorResponse format
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var errorResponse = new Trivare.Application.DTOs.Common.ErrorResponse
            {
                Error = "ValidationError",
                Message = "One or more validation errors occurred",
                Errors = errors
            };

            return new BadRequestObjectResult(errorResponse);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Trivare API",
        Version = "v1",
        Description = "Trip planning API - MVP version",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Trivare Team"
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var allowedOriginsStr = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
var allowedOrigins = allowedOriginsStr?.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(o => o.Trim())
        .Where(o => !string.IsNullOrEmpty(o))
        .ToArray();

if(allowedOrigins != null)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });
}

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = JwtRegisteredClaimNames.Sub,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"))
        ),
        ClockSkew = TimeSpan.Zero // Remove default 5-minute grace period
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//Helper method to determine if SQL error is transient and worth retrying
static bool IsTransientSqlError(SqlException ex)
{
    // Common transient SQL Server error numbers that indicate temporary connectivity issues
    var transientErrorNumbers = new[]
    {
        -2,     // Timeout expired
        2,      // Connection timeout
        53,     // Server is not found or not accessible
        64,     // Named Pipes Provider: Could not open a connection to SQL Server
        233,    // Connection refused - usually due to server not ready
        258,    // TCP Provider: Timeout error
        4060,   // Cannot open database requested by the login
        10053,  // Connection aborted
        10054,  // Connection reset
        10060,  // Connection timeout
        10061,  // Connection refused
        11001,  // Host not found
        18452,  // Login failed for user - can happen during database startup
        18456   // Login failed for user
    };

    return transientErrorNumbers.Contains(ex.Number);
}

// Seed database with initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Retry seeding up to 10 times with exponential backoff for database availability
    const int maxRetries = 10;
    var retryDelay = TimeSpan.FromSeconds(2);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Attempting to seed database (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
            await DbInitializer.SeedAsync(context);
            logger.LogInformation("Database seeding completed successfully");
            break;
        }
        catch (SqlException sqlEx) when (IsTransientSqlError(sqlEx))
        {
            if (attempt == maxRetries)
            {
                logger.LogError(sqlEx, "Failed to seed database after {MaxRetries} attempts due to transient SQL errors. This may be expected if the database is not yet ready. Application will continue without seeding.", maxRetries);
                // Don't fail startup if seeding fails - the app should still be able to start
                break;
            }

            logger.LogWarning(sqlEx, "Database seeding failed on attempt {Attempt}/{MaxRetries} due to transient SQL error. Retrying in {Delay} seconds...", attempt, maxRetries, retryDelay.TotalSeconds);
            await Task.Delay(retryDelay);
            retryDelay = TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 1.5, 30)); // Exponential backoff, max 30 seconds
        }
        catch (Exception ex)
        {
            // For non-SQL exceptions, don't retry
            logger.LogError(ex, "Failed to seed database due to non-transient error. Application will continue without seeding.");
            break;
        }
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

// CORS must come before HttpsRedirection and Authorization
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();