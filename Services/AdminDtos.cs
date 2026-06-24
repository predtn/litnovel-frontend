namespace litnovel_frontend.Services;

public class AdminReportDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "novel";
    public string ReportType { get; set; } = "Spam";
    public string Status { get; set; } = "Pending";
    public string ReporterName { get; set; } = "";
    public string TargetTitle { get; set; } = "";
    public string? TargetSubtitle { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLogDto
{
    public int Id { get; set; }
    public UserSummaryDto? Actor { get; set; }
    public int? ActorId { get; set; }
    public string? ActorUsername { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = "";
    public string? Event { get; set; }
    public string? Operation { get; set; }
    public string EntityType { get; set; } = "";
    public string? TableName { get; set; }
    public string? ResourceType { get; set; }
    public int EntityId { get; set; }
    public string? EntityKey { get; set; }
    public string IpAddress { get; set; } = "";
    public string? Ip { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? Timestamp { get; set; }
    public DateTime? PerformedAt { get; set; }

    public string DisplayActor => Actor?.Username ?? ActorUsername ?? Username ?? (ActorId.HasValue ? $"User #{ActorId}" : "System");
    public string DisplayAction => FirstNonEmpty(Action, Event, Operation) ?? "Unknown";
    public string DisplayEntity => $"{FirstNonEmpty(EntityType, ResourceType, TableName) ?? "Entity"} #{(EntityId != 0 ? EntityId.ToString() : EntityKey ?? "-")}";
    public string DisplayIp => FirstNonEmpty(IpAddress, Ip) ?? "-";
    public DateTime DisplayCreatedAt => CreatedAt != default ? CreatedAt : Timestamp ?? PerformedAt ?? default;

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}

public class SentNotificationDto
{
    public int Id { get; set; }
    public string NotificationType { get; set; } = "SystemAlert";
    public string Message { get; set; } = "";
    public string Target { get; set; } = "All users";
    public int ReadCount { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
}

public class BackupDto
{
    public string Id { get; set; } = "";
    public long SizeBytes { get; set; }
    public string SizeFormatted { get; set; } = "";
    public string Status { get; set; } = "Completed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? DownloadUrl { get; set; }
}

public class ChartPointDto
{
    public string Date { get; set; } = "";
    public int Value { get; set; }
}

public class StatisticsChartDto
{
    public string Metric { get; set; } = "";
    public List<ChartPointDto> Points { get; set; } = [];
}

public class AdminSettingsDto
{
    public GeneralSettingsDto General { get; set; } = new();
    public ContentSettingsDto Content { get; set; } = new();
    public ModerationSettingsDto Moderation { get; set; } = new();
}

public class GeneralSettingsDto
{
    public string SiteName { get; set; } = "LitNovel";
    public string Tagline { get; set; } = "Read, Write, Discover";
    public bool MaintenanceMode { get; set; }
}

public class ContentSettingsDto
{
    public int MaxNovelDescriptionLength { get; set; } = 5000;
    public int MaxChapterLength { get; set; } = 50000;
    public int MaxTagsPerNovel { get; set; } = 10;
}

public class ModerationSettingsDto
{
    public int ReviewSLAHours { get; set; } = 48;
    public List<string> AutoFlagKeywords { get; set; } = [];
}
