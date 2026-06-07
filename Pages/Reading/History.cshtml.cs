using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Reading;
public class HistoryModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<ReadingProgressDto> History { get; set; } = [];
    public HistoryModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var r = await _api.GetAsync<List<ReadingProgressDto>>("/api/reading-progress", token);
        History = r?.Data ?? GetMock();
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    private List<ReadingProgressDto> GetMock() =>
    [
        new() { Novel = new() { Title = "Long Vương Truyền Thuyết", Slug = "long-vuong" }, LastChapter = new() { Id = 51, Title = "Chương 51: Đại Chiến" }, ProgressPercentage = 65, LastReadAt = DateTime.UtcNow.AddHours(-2) },
        new() { Novel = new() { Title = "Thiên Đạo Thư Viện", Slug = "thien-dao" }, LastChapter = new() { Id = 12, Title = "Chương 12" }, ProgressPercentage = 22, LastReadAt = DateTime.UtcNow.AddDays(-1) },
    ];
}
