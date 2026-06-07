using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<CategoryDto> Categories { get; set; } = [];
    public List<NovelSummaryDto> TrendingNovels { get; set; } = [];
    public List<NovelSummaryDto> NewNovels { get; set; } = [];
    public List<NovelSummaryDto> TopRatedNovels { get; set; } = [];

    public IndexModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);

        // Load categories, trending, new, top-rated in parallel
        var catTask      = _api.GetAsync<List<CategoryDto>>("/api/categories");
        var trendingTask = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/novels?sort=viewCount&order=desc&status=Ongoing&page=1&size=8");
        var newTask      = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/novels?sort=updatedAt&order=desc&status=Ongoing&page=1&size=8");
        var topTask      = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/novels?sort=ratingAverage&order=desc&page=1&size=8");

        await Task.WhenAll(catTask, trendingTask, newTask, topTask);

        Categories      = catTask.Result?.Data ?? GetMockCategories();
        TrendingNovels  = trendingTask.Result?.Data?.Items ?? GetMockNovels();
        NewNovels       = newTask.Result?.Data?.Items ?? GetMockNovels();
        TopRatedNovels  = topTask.Result?.Data?.Items ?? GetMockNovels();

        // Set user info for layout
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"]  = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"]= user.Avatar;
        }
    }

    // Mock data for when API is unavailable
    private List<CategoryDto> GetMockCategories() =>
    [
        new() { Id = 1, Name = "Tiên hiệp", Slug = "tien-hiep" },
        new() { Id = 2, Name = "Kiếm hiệp", Slug = "kiem-hiep" },
        new() { Id = 3, Name = "Lãng mạn", Slug = "lang-man" },
        new() { Id = 4, Name = "Huyền huyễn", Slug = "huyen-huyen" },
        new() { Id = 5, Name = "Đô thị", Slug = "do-thi" },
        new() { Id = 6, Name = "Khoa huyễn", Slug = "khoa-huyen" },
    ];

    private List<NovelSummaryDto> GetMockNovels() =>
    [
        new() { Id = 1, Title = "Long Vương Truyền Thuyết", Slug = "long-vuong-truyen-thuyet", Author = new() { Username = "tác_giả_1" }, Status = "Ongoing", ViewCount = 128432, RatingAverage = 4.5, TotalChapters = 324, UpdatedAt = DateTime.UtcNow.AddHours(-2), Category = new() { Name = "Tiên hiệp" } },
        new() { Id = 2, Title = "Thiên Đạo Thư Viện", Slug = "thien-dao-thu-vien", Author = new() { Username = "tác_giả_2" }, Status = "Ongoing", ViewCount = 98120, RatingAverage = 4.8, TotalChapters = 512, UpdatedAt = DateTime.UtcNow.AddHours(-5), Category = new() { Name = "Huyền huyễn" } },
        new() { Id = 3, Title = "Vô Hạn Phục Hồi", Slug = "vo-han-phuc-hoi", Author = new() { Username = "tác_giả_3" }, Status = "Ongoing", ViewCount = 85200, RatingAverage = 4.3, TotalChapters = 198, UpdatedAt = DateTime.UtcNow.AddDays(-1), Category = new() { Name = "Khoa huyễn" } },
        new() { Id = 4, Title = "Mộng Tình Thiên Hạ", Slug = "mong-tinh-thien-ha", Author = new() { Username = "tác_giả_4" }, Status = "Ended", ViewCount = 76400, RatingAverage = 4.6, TotalChapters = 256, UpdatedAt = DateTime.UtcNow.AddDays(-3), Category = new() { Name = "Lãng mạn" } },
        new() { Id = 5, Title = "Chiến Thần Tái Lâm", Slug = "chien-than-tai-lam", Author = new() { Username = "tác_giả_5" }, Status = "Ongoing", ViewCount = 65800, RatingAverage = 4.2, TotalChapters = 147, UpdatedAt = DateTime.UtcNow.AddHours(-8), Category = new() { Name = "Kiếm hiệp" } },
        new() { Id = 6, Title = "Đô Thị Chi Vương", Slug = "do-thi-chi-vuong", Author = new() { Username = "tác_giả_6" }, Status = "Ongoing", ViewCount = 54300, RatingAverage = 4.1, TotalChapters = 89, UpdatedAt = DateTime.UtcNow.AddHours(-12), Category = new() { Name = "Đô thị" } },
        new() { Id = 7, Title = "Hắc Long Ký", Slug = "hac-long-ky", Author = new() { Username = "tác_giả_7" }, Status = "Hiatus", ViewCount = 42100, RatingAverage = 4.7, TotalChapters = 320, UpdatedAt = DateTime.UtcNow.AddDays(-7), Category = new() { Name = "Tiên hiệp" } },
        new() { Id = 8, Title = "Vũ Hành Thiên Địa", Slug = "vu-hanh-thien-dia", Author = new() { Username = "tác_giả_8" }, Status = "Ongoing", ViewCount = 38900, RatingAverage = 4.4, TotalChapters = 156, UpdatedAt = DateTime.UtcNow.AddDays(-2), Category = new() { Name = "Huyền huyễn" } },
    ];
}
