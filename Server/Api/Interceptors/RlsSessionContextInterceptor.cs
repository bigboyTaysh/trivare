using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Data.Common;
using System.Security.Claims;

namespace Trivare.Api.Interceptors;

/// <summary>
/// Interceptor that sets the SESSION_CONTEXT for Row-Level Security (RLS) on SQL Server connections.
/// This ensures that the current user's ID is available in the database session for RLS predicates.
/// </summary>
public class RlsSessionContextInterceptor : DbConnectionInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RlsSessionContextInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Called when a connection is opened. Sets the UserId in SESSION_CONTEXT if the user is authenticated.
    /// </summary>
    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);

        if (connection.State == System.Data.ConnectionState.Open)
        {
            await SetSessionContextAsync(connection, cancellationToken);
        }
    }

    /// <summary>
    /// Called when a connection is opened (synchronous version).
    /// </summary>
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        base.ConnectionOpened(connection, eventData);

        if (connection.State == System.Data.ConnectionState.Open)
        {
            SetSessionContext(connection);
        }
    }

    private async Task SetSessionContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            using var command = connection.CreateCommand();
            command.CommandText = "EXEC sp_set_session_context 'UserId', @UserId, @read_only = 1";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@UserId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private void SetSessionContext(DbConnection connection)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            using var command = connection.CreateCommand();
            command.CommandText = "EXEC sp_set_session_context 'UserId', @UserId, @read_only = 1";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@UserId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);
            command.ExecuteNonQuery();
        }
    }

    private string? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim;
        }
        return null;
    }
}