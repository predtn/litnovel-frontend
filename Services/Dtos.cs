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
    public string Status { get; set; } = "Online";
}

public class UserDetailDto : UserSummaryDto
{
    public string Email { get; set; } = "";
    public string? Bio { get; set; }
    public int Reputation { get; set; }
    public List<BadgeDto> Badges { get; set; } = [];
    public UserStatsDto Stats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
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
    public int TotalVolumes { get; set; }
    public bool? IsFavorited { get; set; }
    public bool? IsLiked { get; set; }
    public byte? UserRating { get; set; }
    public List<VolumeWithChaptersDto> Volumes { get; set; } = [];
}

public class VolumeDto
{
    public int Id { get; set; }
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
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
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
public class StatUserDto { public int Total; public int NewThisWeek; public int Banned; }
public class StatNovelDto { public int Total; public int Ongoing; public int Pending; public int NewThisMonth; }
public class StatChapterDto { public int Total; public int PublishedThisWeek; }
public class StatReportDto { public int Total; public int Open; public int ResolvedThisMonth; }
public class StatEngagementDto { public int TotalComments; public int TotalRatings; public int TotalFavorites; }

public class StaffDashboardDto
{
    public int PendingNovels { get; set; }
    public int PendingChapters { get; set; }
    public int OpenReports { get; set; }
}

// Extension methods for string
public static class StringExtensions
{
    public static string Truncate(this string s, int max) => s.Length <= max ? s : s[..max] + "…";
}

