using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;

namespace Trivare.Infrastructure.Data;

/// <summary>
/// Database initializer for seeding initial data
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with initial required data
    /// </summary>
    /// <param name="context">The database context</param>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created and migrations are applied
        await context.Database.MigrateAsync();

        // Seed roles if they don't exist
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "User" 
                },
                new Role 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Admin" 
                }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }
    }
}
