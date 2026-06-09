using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Publish;

public abstract class PublishPageModel : PageModel
{
    protected readonly IApiService Api;
    protected readonly IAuthService Auth;

    protected PublishPageModel(IApiService api, IAuthService auth)
    {
        Api = api;
        Auth = auth;
    }

    protected IActionResult? RequireAuthor()
    {
        var token = Auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!Auth.IsInRole(HttpContext, "User")) return RedirectToPage("/Index");

        var user = Auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"] = user.Avatar;
        }

        ViewData["ActiveNav"] = "publish";
        return null;
    }

    protected string? Token => Auth.GetToken(HttpContext);

    protected static List<CategoryDto> MockCategories() =>
    [
        new() { Id = 1, Name = "Fantasy", Slug = "fantasy", NovelCount = 42 },
        new() { Id = 2, Name = "Romance", Slug = "romance", NovelCount = 31 },
        new() { Id = 3, Name = "Action", Slug = "action", NovelCount = 28 },
        new() { Id = 4, Name = "Mystery", Slug = "mystery", NovelCount = 18 }
    ];

    protected static List<TagDto> MockTags() =>
    [
        new() { Id = 1, Name = "Adventure", Slug = "adventure" },
        new() { Id = 2, Name = "Slow burn", Slug = "slow-burn" },
        new() { Id = 3, Name = "Magic", Slug = "magic" },
        new() { Id = 4, Name = "Academy", Slug = "academy" },
        new() { Id = 5, Name = "Found family", Slug = "found-family" }
    ];

    protected static List<NovelSummaryDto> MockMyNovels() =>
    [
        new()
        {
            Id = 42, Title = "The Dragon War Chronicles", Slug = "the-dragon-war-chronicles",
            Status = "Draft", TotalChapters = 8, TotalVolumes = 2, ViewCount = 0, RatingAverage = 0,
            Category = new() { Id = 1, Name = "Fantasy" }, UpdatedAt = DateTime.UtcNow.AddHours(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        },
        new()
        {
            Id = 43, Title = "Moonlit Archive", Slug = "moonlit-archive",
            Status = "Pending", TotalChapters = 4, TotalVolumes = 1, ViewCount = 120, RatingAverage = 4.1,
            Category = new() { Id = 4, Name = "Mystery" }, UpdatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-32)
        },
        new()
        {
            Id = 44, Title = "Neon Blade School", Slug = "neon-blade-school",
            Status = "Ongoing", TotalChapters = 36, TotalVolumes = 3, ViewCount = 28400, RatingAverage = 4.6,
            Category = new() { Id = 3, Name = "Action" }, UpdatedAt = DateTime.UtcNow.AddDays(-3),
            CreatedAt = DateTime.UtcNow.AddDays(-120)
        }
    ];

    protected static NovelDetailDto MockNovel(int id)
    {
        var summary = MockMyNovels().FirstOrDefault(n => n.Id == id) ?? MockMyNovels()[0];
        return new()
        {
            Id = id,
            Title = summary.Title,
            Slug = summary.Slug,
            Status = summary.Status,
            ViewCount = summary.ViewCount,
            RatingAverage = summary.RatingAverage,
            RatingCount = summary.RatingAverage > 0 ? 96 : 0,
            TotalChapters = summary.TotalChapters,
            TotalVolumes = summary.TotalVolumes,
            Category = summary.Category,
            Tags = [MockTags()[0], MockTags()[2]],
            Description = "<p>A carefully paced serial novel with mystery, pressure, and character-driven stakes.</p>",
            UpdatedAt = summary.UpdatedAt,
            CreatedAt = summary.CreatedAt,
            Volumes =
            [
                new()
                {
                    Id = 101, NovelId = id, VolumeNumber = 1, Title = "Volume 1: Open Doors", ChapterCount = 3,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    Chapters =
                    [
                        new() { Id = 1001, VolumeId = 101, ChapterNumber = 1, Title = "A Letter at Dawn", Status = "Published", WordCount = 2400, CreatedAt = DateTime.UtcNow.AddDays(-25), UpdatedAt = DateTime.UtcNow.AddDays(-24) },
                        new() { Id = 1002, VolumeId = 101, ChapterNumber = 2, Title = "The Locked Reading Room", Status = "Pending", WordCount = 3100, CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow.AddDays(-8) },
                        new() { Id = 1003, VolumeId = 101, ChapterNumber = 3, Title = "A Name in the Margin", Status = "Draft", WordCount = 1800, CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow.AddHours(-6) }
                    ]
                },
                new()
                {
                    Id = 102, NovelId = id, VolumeNumber = 2, Title = "Volume 2: Deeper Shelves", ChapterCount = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    Chapters =
                    [
                        new() { Id = 1004, VolumeId = 102, ChapterNumber = 1, Title = "Ink That Would Not Dry", Status = "Draft", WordCount = 1200, CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow.AddHours(-2) }
                    ]
                }
            ]
        };
    }

    protected static ChapterDetailDto MockChapter(int id)
    {
        var novel = MockNovel(42);
        var chapter = novel.Volumes.SelectMany(v => v.Chapters).FirstOrDefault(c => c.Id == id) ?? novel.Volumes[0].Chapters[0];
        return new()
        {
            Id = id,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Status = chapter.Status,
            Content = "<p>The archive woke before the city did.</p><p>Between two rows of glass-backed shelves, Mara found a letter addressed in her own handwriting.</p>",
            Volume = novel.Volumes.First(v => v.Chapters.Any(c => c.Id == chapter.Id)),
            Novel = novel,
            CreatedAt = chapter.CreatedAt
        };
    }

    protected static NovelAnalyticsDto MockAnalytics(int novelId) => new()
    {
        NovelId = novelId,
        ViewCount = 28400,
        LikeCount = 1260,
        FavoritesCount = 880,
        RatingAverage = 4.6,
        RatingCount = 96,
        CommentCount = 312,
        RatingDistribution = new() { ["1"] = 2, ["2"] = 3, ["3"] = 12, ["4"] = 31, ["5"] = 48 },
        TopChapters =
        [
            new() { ChapterId = 1001, Title = "A Letter at Dawn", CommentCount = 89 },
            new() { ChapterId = 1002, Title = "The Locked Reading Room", CommentCount = 64 },
            new() { ChapterId = 1003, Title = "A Name in the Margin", CommentCount = 28 }
        ]
    };
}
