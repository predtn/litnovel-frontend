namespace litnovel_frontend.Services;

// ─── Generic API response wrapper ───
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class PagedData<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
}

// ─── DTOs ───
public class UserSummaryDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string? Avatar { get; set; }
    public string Role { get; set; } = "User";
    public string Status { get; set; } = "Offline";
}

public class UserDetailDto : UserSummaryDto
{
    public string Email { get; set; } = "";
    public string? Bio { get; set; }
    public int Reputation { get; set; }
    public List<BadgeDto> Badges { get; set; } = [];
    public UserStatsDto Stats { get; set; } = new();
    public List<UserWarningDto> Warnings { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? RegisteredAt { get; set; }

    public DateTime JoinedDate =>
        CreatedAt != default ? CreatedAt :
        JoinedAt ?? JoinDate ?? RegisteredAt ?? default;
}

public class UserWarningDto
{
    public string Reason { get; set; } = "";
    public string Severity { get; set; } = "";
    public UserSummaryDto? IssuedBy { get; set; }
    public DateTime IssuedAt { get; set; }
}

public class UserStatsDto
{
    public int NovelsCreated { get; set; }
    public int ChaptersPublished { get; set; }
    public int FavoritesCount { get; set; }
    public int CommentsCount { get; set; }
}

public class BadgeDto
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    public int EarnedCount { get; set; }
    public DateTime EarnedAt { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int NovelCount { get; set; }
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int NovelCount { get; set; }
}

public class NovelSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? CoverImage { get; set; }
    public UserSummaryDto? Author { get; set; }
    public CategoryDto? Category { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public string Status { get; set; } = "Draft";
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int TotalChapters { get; set; }
    public int TotalVolumes { get; set; }
    public double RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    // For reading history
    public DateTime? FavoritedAt { get; set; }
}

public class NovelDetailDto : NovelSummaryDto
{
    public string? Description { get; set; }
    public bool? IsFavorited { get; set; }
    public bool? IsLiked { get; set; }
    public byte? UserRating { get; set; }
    public List<VolumeWithChaptersDto> Volumes { get; set; } = [];
}

public class VolumeDto
{
    public int Id { get; set; }
    public int NovelId { get; set; }
    public int VolumeNumber { get; set; }
    public string Title { get; set; } = "";
    public int ChapterCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VolumeWithChaptersDto : VolumeDto
{
    public List<ChapterNavDto> Chapters { get; set; } = [];
}

public class ChapterNavDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public int VolumeId { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public int WordCount { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ChapterDetailDto
{
    public int Id { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Content { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public DateTime? ReleaseDate { get; set; }
    public VolumeDto? Volume { get; set; }
    public NovelSummaryDto? Novel { get; set; }
    public ChapterNavDto? PrevChapter { get; set; }
    public ChapterNavDto? NextChapter { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommentDto
{
    public int Id { get; set; }
    public UserSummaryDto? User { get; set; }
    public string Content { get; set; } = "";
    public int LikeCount { get; set; }
    public bool IsLiked { get; set; }
    public int DislikeCount { get; set; }
    public int? ParentCommentId { get; set; }
    public List<CommentDto> Replies { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class ReviewSummaryDto
{
    public double Average { get; set; }
    public int Total { get; set; }
    public Dictionary<string, int> Distribution { get; set; } = [];
}

public class ReviewDto
{
    public int Id { get; set; }
    public UserSummaryDto? User { get; set; }
    public byte Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewPageDto
{
    public ReviewSummaryDto? Summary { get; set; }
    public List<ReviewDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
}

public class ReadingProgressDto
{
    public NovelSummaryDto? Novel { get; set; }
    public ChapterNavDto? LastChapter { get; set; }
    public byte ProgressPercentage { get; set; }
    public DateTime LastReadAt { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string NotificationType { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationListDto
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
}

public class ReportDto
{
    public int Id { get; set; }
    public string ReportType { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ActionTaken { get; set; }
    public string? ResolutionNotes { get; set; }
    public UserSummaryDto? Reporter { get; set; }
    public UserSummaryDto? ProcessedBy { get; set; }
    public NovelSummaryDto? TargetNovel { get; set; }
    public ChapterNavDto? TargetChapter { get; set; }
    public UserSummaryDto? TargetUser { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StatisticsDto
{
    public StatUserDto Users { get; set; } = new();
    public StatNovelDto Novels { get; set; } = new();
    public StatChapterDto Chapters { get; set; } = new();
    public StatReportDto Reports { get; set; } = new();
    public StatEngagementDto Engagement { get; set; } = new();
}
public class StatUserDto
{
    public int Total { get; set; }
    public int NewThisWeek { get; set; }
    public int Banned { get; set; }
}

public class StatNovelDto
{
    public int Total { get; set; }
    public int Ongoing { get; set; }
    public int Pending { get; set; }
    public int NewThisMonth { get; set; }
}

public class StatChapterDto
{
    public int Total { get; set; }
    public int PublishedThisWeek { get; set; }
}

public class StatReportDto
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int ResolvedThisMonth { get; set; }
}

public class StatEngagementDto
{
    public int TotalComments { get; set; }
    public int TotalRatings { get; set; }
    public int TotalFavorites { get; set; }
}

public class StaffDashboardDto
{
    public int PendingNovels { get; set; }
    public int PendingChapters { get; set; }
    public int OpenReports { get; set; }
    public int ActiveWarnings { get; set; }
    public List<ModerationActivityItem> RecentActivity { get; set; } = [];
}

public class ModerationActivityItem
{
    public string Action { get; set; } = "";
    public string StaffUsername { get; set; } = "";
    public string Target { get; set; } = "";
    public DateTime PerformedAt { get; set; }
}

public class NovelUpsertRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? CoverImage { get; set; }
    public int? CategoryId { get; set; }
    public List<int> TagIds { get; set; } = [];
    public string? Status { get; set; }
}

public class VolumeUpsertRequest
{
    public int VolumeNumber { get; set; }
    public string Title { get; set; } = "";
}

public class ChapterUpsertRequest
{
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime? ReleaseDate { get; set; }
    public string Status { get; set; } = "Draft";
}

public class NovelAnalyticsDto
{
    public int NovelId { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int FavoritesCount { get; set; }
    public double RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public int CommentCount { get; set; }
    public Dictionary<string, int> RatingDistribution { get; set; } = [];
    public List<TopChapterDto> TopChapters { get; set; } = [];
}

public class TopChapterDto
{
    public int ChapterId { get; set; }
    public string Title { get; set; } = "";
    public int CommentCount { get; set; }
}

public class ModerationItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Type { get; set; } = "Novel";
    public string Status { get; set; } = "Pending";
    public string? ReviewerNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
}

// Extension methods for string
public static class StringExtensions
{
    public static string Truncate(this string s, int max) => s.Length <= max ? s : s[..max] + "…";
}

// ─── Staff Moderation DTOs ───
public class PendingNovelDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? CoverImage { get; set; }
    public string? Description { get; set; }
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = "";
    public string? CategoryName { get; set; }
    public List<string> Tags { get; set; } = [];
    public int TotalChapters { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class PendingChapterDto
{
    public int Id { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public int NovelId { get; set; }
    public string NovelTitle { get; set; } = "";
    public string NovelSlug { get; set; } = "";
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = "";
    public DateTime SubmittedAt { get; set; }
}

public class NovelReviewDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public string? CoverImage { get; set; }
    public string Status { get; set; } = "";
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = "";
    public string? AuthorAvatar { get; set; }
    public string? CategoryName { get; set; }
    public List<string> Tags { get; set; } = [];
    public int TotalChapters { get; set; }
    public int TotalVolumes { get; set; }
    public int ViewCount { get; set; }
    public List<ReviewVolumeDto> Volumes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReviewVolumeDto
{
    public int Id { get; set; }
    public int VolumeNumber { get; set; }
    public string Title { get; set; } = "";
    public List<ReviewChapterDto> Chapters { get; set; } = [];
}

public class ReviewChapterDto
{
    public int Id { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "";
}

public class ChapterReviewDetailDto
{
    public int Id { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Status { get; set; } = "";
    public string Content { get; set; } = "";
    public int NovelId { get; set; }
    public string NovelTitle { get; set; } = "";
    public string NovelSlug { get; set; } = "";
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = "";
    public int VolumeId { get; set; }
    public string VolumeTitle { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StaffReportDto
{
    public int Id { get; set; }
    public string Kind { get; set; } = "";
    public string ReportType { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ActionTaken { get; set; }
    public string? ResolutionNotes { get; set; }
    public ReportActorInfoDto? Reporter { get; set; }
    public ReportActorInfoDto? ProcessedBy { get; set; }
    public ReportTargetNovelInfoDto? TargetNovel { get; set; }
    public ReportActorInfoDto? TargetUser { get; set; }
    public ReportTargetChapterInfoDto? TargetChapter { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReportActorInfoDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string? Avatar { get; set; }
}

public class ReportTargetNovelInfoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
}

public class ReportTargetChapterInfoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int ChapterNumber { get; set; }
}

public class ModerationHistoryDto
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string StaffUsername { get; set; } = "";
    public string Action { get; set; } = "";       // e.g. "ApproveNovel", "RejectChapter"
    public string TargetType { get; set; } = "";   // "Novel" | "Chapter" | "Report" | "User"
    public int TargetId { get; set; }
    public string? TargetTitle { get; set; }
    public string? Notes { get; set; }
    public DateTime PerformedAt { get; set; }
}

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
    public string DisplayEntity
    {
        get
        {
            var type = FirstNonEmpty(EntityType, ResourceType, TableName) ?? "Entity";
            var key = EntityId != 0 ? $"#{EntityId}" : "-";
            return string.IsNullOrWhiteSpace(EntityKey) ? $"{type} {key}" : $"{type} {key} - {EntityKey}";
        }
    }
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

public class TaxonomyListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int NovelCount { get; set; }
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
}

public class ModerationSettingsDto
{
    public int ReviewSLAHours { get; set; } = 48;
    public List<string> AutoFlagKeywords { get; set; } = [];
}
