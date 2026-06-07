using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Novels;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<NovelSummaryDto> Novels { get; set; } = [];
    public List<CategoryDto> Categories { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int CategoryId { get; set; }
    public string Sort { get; set; } = "updatedAt";

    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task OnGetAsync(string? keyword, string? status, int categoryId = 0, int page = 1, string sort = "updatedAt")
    {
        Keyword = keyword;
        Status = status;
        CategoryId = categoryId;
        Page = page;
        Sort = sort;

        var token = _auth.GetToken(HttpContext);
        var qs = $"/api/novels?page={page}&size=20&sort={sort}&order=desc"
            + (string.IsNullOrEmpty(keyword) ? "" : $"&keyword={Uri.EscapeDataString(keyword)}")
            + (string.IsNullOrEmpty(status)  ? "" : $"&status={status}")
            + (categoryId > 0                ? $"&categoryId={categoryId}" : "");

        var novelTask = _api.GetAsync<PagedData<NovelSummaryDto>>(qs, token);
        var catTask   = _api.GetAsync<List<CategoryDto>>("/api/categories");
        await Task.WhenAll(novelTask, catTask);

        var data = novelTask.Result?.Data;
        Novels        = data?.Items ?? [];
        TotalPages    = data?.TotalPages ?? 1;
        TotalElements = data?.TotalElements ?? Novels.Count;
        Categories    = catTask.Result?.Data ?? [];

        // Mock fallback
        if (!Novels.Any())
        {
            Novels = GetMockNovels();
            TotalElements = Novels.Count;
            Categories = GetMockCategories();
        }
    }

    private List<NovelSummaryDto> GetMockNovels() =>
    [
        new() { Id = 1, Title = "Long Vương Truyền Thuyết", Slug = "long-vuong-truyen-thuyet", Author = new() { Username = "tác_giả_1" }, Status = "Ongoing", ViewCount = 128432, RatingAverage = 4.5 },
        new() { Id = 2, Title = "Thiên Đạo Thư Viện", Slug = "thien-dao-thu-vien", Author = new() { Username = "tác_giả_2" }, Status = "Ongoing", ViewCount = 98120, RatingAverage = 4.8 },
        new() { Id = 3, Title = "Vô Hạn Phục Hồi", Slug = "vo-han-phuc-hoi", Author = new() { Username = "tác_giả_3" }, Status = "Ended", ViewCount = 85200, RatingAverage = 4.3 },
        new() { Id = 4, Title = "Mộng Tình Thiên Hạ", Slug = "mong-tinh-thien-ha", Author = new() { Username = "tác_giả_4" }, Status = "Ended", ViewCount = 76400, RatingAverage = 4.6 },
        new() { Id = 5, Title = "Chiến Thần Tái Lâm", Slug = "chien-than-tai-lam", Author = new() { Username = "tác_giả_5" }, Status = "Ongoing", ViewCount = 65800, RatingAverage = 4.2 },
        new() { Id = 6, Title = "Đô Thị Chi Vương", Slug = "do-thi-chi-vuong", Author = new() { Username = "tác_giả_6" }, Status = "Hiatus", ViewCount = 54300, RatingAverage = 4.1 },
    ];

    private List<CategoryDto> GetMockCategories() =>
    [
        new() { Id = 1, Name = "Tiên hiệp" }, new() { Id = 2, Name = "Kiếm hiệp" },
        new() { Id = 3, Name = "Lãng mạn" },  new() { Id = 4, Name = "Huyền huyễn" },
        new() { Id = 5, Name = "Đô thị" },    new() { Id = 6, Name = "Khoa huyễn" },
    ];
}
