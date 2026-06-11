using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Publish;

public class PublishStats { public int Total; public int Ongoing; public int Pending; public int TotalViews; }

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public PublishStats Stats { get; set; } = new();
    public string? StatusFilter { get; set; }

    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync(string? status)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        StatusFilter = status;

        var qs = $"/api/novels/my?page=1&size=50" + (string.IsNullOrEmpty(status) ? "" : $"&status={status}");
        var result = await _api.GetAsync<PagedData<NovelSummaryDto>>(qs, token);
        Novels = result?.Data?.Items ?? GetMockNovels();

        Stats = new() {
            Total = Novels.Count,
            Ongoing = Novels.Count(n => n.Status == "Ongoing"),
            Pending = Novels.Count(n => n.Status == "Pending"),
            TotalViews = Novels.Sum(n => n.ViewCount)
        };

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; ViewData["UserAvatar"] = user.Avatar; }
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int novelId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/novels/{novelId}/submit", null, token);
        TempData["Success"] = "Đã gửi yêu cầu duyệt!";
        return RedirectToPage();
    }

    private List<NovelSummaryDto> GetMockNovels() =>
    [
        new() { Id = 1, Title = "Long Vương Truyền Thuyết", Status = "Ongoing",  TotalChapters = 52, ViewCount = 12400, RatingAverage = 4.5, UpdatedAt = DateTime.UtcNow.AddHours(-2),  Category = new() { Name = "Tiên hiệp" } },
        new() { Id = 2, Title = "Thiên Địa Bí Nguyên",       Status = "Draft",    TotalChapters = 3,  ViewCount = 0,     RatingAverage = 0,   UpdatedAt = DateTime.UtcNow.AddDays(-1),  Category = new() { Name = "Huyền huyễn" } },
        new() { Id = 3, Title = "Vô Danh Anh Hùng",          Status = "Pending",  TotalChapters = 10, ViewCount = 0,     RatingAverage = 0,   UpdatedAt = DateTime.UtcNow.AddDays(-2) },
    ];
}
