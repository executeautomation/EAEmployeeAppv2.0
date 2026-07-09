using System.Globalization;
using EAEmployee.Net8.Data;
using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Services;

public interface IAuditService
{
    Task LogAsync(string entityType, string action, int? entityId, string? summary = null);
    Task LogWithValuesAsync(string entityType, string action, int? entityId, object? oldValues, object? newValues, string? summary = null);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor httpContext, ILogger<AuditService> logger)
    {
        _db = db;
        _httpContext = httpContext;
        _logger = logger;
    }

    public async Task LogAsync(string entityType, string action, int? entityId, string? summary = null)
    {
        await LogInternalAsync(entityType, action, entityId, null, null, summary);
    }

    public async Task LogWithValuesAsync(string entityType, string action, int? entityId, object? oldValues, object? newValues, string? summary = null)
    {
        var oldJson = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues, new System.Text.Json.JsonSerializerOptions { WriteIndented = false }) : null;
        var newJson = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues, new System.Text.Json.JsonSerializerOptions { WriteIndented = false }) : null;
        await LogInternalAsync(entityType, action, entityId, oldJson, newJson, summary);
    }

    private async Task LogInternalAsync(string entityType, string action, int? entityId, string? oldValues, string? newValues, string? summary)
    {
        try
        {
            var userName = _httpContext.HttpContext?.User.Identity?.Name;
            var ip = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString();

            // Truncate long values to fit the column
            if (oldValues != null && oldValues.Length > 4096) oldValues = oldValues[..4093] + "...";
            if (newValues != null && newValues.Length > 4096) newValues = newValues[..4093] + "...";

            var entry = new AuditLog
            {
                EntityType = entityType,
                Action = action,
                Timestamp = DateTime.UtcNow,
                UserName = userName,
                IpAddress = ip,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Summary = summary ?? $"{action} {entityType} (ID: {entityId})"
            };

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Never let audit failures break the main operation
            _logger.LogWarning(ex, "Failed to write audit log for {EntityType} {Action}", entityType, action);
        }
    }
}
