using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Novels;

public class DetailModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public NovelDetailDto? Novel { get; set; }
    public List<ReviewDto> Reviews { get; set; } = [];

    public DetailModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task OnGetAsync(string slug)
    {
        var token = _auth.GetToken(HttpContext);
        var novelTask   = _api.GetAsync<NovelDetailDto>($"/api/novels/{slug}", token);
        var reviewTask  = _api.GetAsync<ReviewPageDto>($"/api/novels/{slug}/reviews?page=1&size=10", token);
        await Task.WhenAll(novelTask, reviewTask);

        Novel   = novelTask.Result?.Data ?? GetMockNovel(slug);
        Reviews = reviewTask.Result?.Data?.Items ?? GetMockReviews();

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserAvatar"] = user.Avatar; }
    }

    public async Task<IActionResult> OnPostReviewAsync(int id, [FromForm] byte rating, [FromForm] string? reviewText)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>($"/api/novels/{id}/reviews", new { rating, review = reviewText }, token);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReportAsync(int targetNovelId, string reportType, string description)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>("/api/reports/novels", new { targetNovelId, reportType, description }, token);
        TempData["Success"] = "Báo cáo đã được gửi.";
        return RedirectToPage();
    }

    private NovelDetailDto GetMockNovel(string slug) => new()
    {
        Id = 1, Title = "Long Vương Truyền Thuyết", Slug = slug,
        Description = "<p>Một câu chuyện về long vương hồi sinh sau ngàn năm, bước vào thế giới tu tiên hiện đại...</p>",
        Author = new() { Id = 5, Username = "tác_giả_1" },
        Category = new() { Id = 1, Name = "Tiên hiệp" },
        Tags = [new() { Id = 1, Name = "Long tộc" }, new() { Id = 2, Name = "Mạnh mẽ" }],
        Status = "Ongoing", ViewCount = 128432, LikeCount = 3200, RatingAverage = 4.5, RatingCount = 1248,
        TotalChapters = 324, TotalVolumes = 3,
        Volumes = [
            new() { Id = 1, VolumeNumber = 1, Title = "Tập 1: Tái Sinh", Chapters = [
                new() { Id = 1, ChapterNumber = 1, Title = "Chương 1: Thức Tỉnh", Status = "Published", CreatedAt = DateTime.UtcNow.AddDays(-60) },
                new() { Id = 2, ChapterNumber = 2, Title = "Chương 2: Thế Giới Mới", Status = "Published", CreatedAt = DateTime.UtcNow.AddDays(-58) },
                new() { Id = 3, ChapterNumber = 3, Title = "Chương 3: Bước Đầu Tu Tiên", Status = "Published", CreatedAt = DateTime.UtcNow.AddDays(-56) },
            ]},
            new() { Id = 2, VolumeNumber = 2, Title = "Tập 2: Trỗi Dậy", Chapters = [
                new() { Id = 50, ChapterNumber = 50, Title = "Chương 50: Đột Phá", Status = "Published", CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new() { Id = 51, ChapterNumber = 51, Title = "Chương 51: Đại Chiến", Status = "Published", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            ]}
        ]
    };

    private List<ReviewDto> GetMockReviews() =>
    [
        new() { Id = 1, User = new() { Username = "doc_gia_1" }, Rating = 5, Review = "Truyện cực hay, lối viết hấp dẫn, tình tiết hợp lý!", CreatedAt = DateTime.UtcNow.AddDays(-5) },
        new() { Id = 2, User = new() { Username = "doc_gia_2" }, Rating = 4, Review = "Thú vị, nhưng đôi chỗ hơi dài dòng.", CreatedAt = DateTime.UtcNow.AddDays(-10) },
        new() { Id = 3, User = new() { Username = "doc_gia_3" }, Rating = 5, Review = "Một trong những truyện hay nhất tôi từng đọc!", CreatedAt = DateTime.UtcNow.AddDays(-15) },
    ];
}
