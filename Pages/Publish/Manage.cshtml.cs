using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Publish;
public class ManageModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public NovelDetailDto? Novel { get; set; }
    public ManageModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var r = await _api.GetAsync<NovelDetailDto>($"/api/novels/{id}", token);
        Novel = r?.Data ?? GetMock(id);
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostSubmitAsync(int novelId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/novels/{novelId}/submit", null, token);
        TempData["Success"] = "Đã gửi yêu cầu duyệt!"; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostSubmitChapterAsync(int chapterId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/chapters/{chapterId}/submit", null, token);
        TempData["Success"] = "Đã gửi chương để duyệt!"; return RedirectToPage();
    }
    private NovelDetailDto GetMock(int id) => new()
    {
        Id = id, Title = "Long Vương Truyền Thuyết", Slug = "long-vuong", Status = "Draft",
        ViewCount = 0, LikeCount = 0, RatingAverage = 0, RatingCount = 0, TotalChapters = 3, TotalVolumes = 1,
        Volumes = [new() { Id=1, VolumeNumber=1, Title="Tập 1: Tái Sinh", Chapters=[
            new(){Id=1,ChapterNumber=1,Title="Chương 1: Thức Tỉnh",Status="Draft",CreatedAt=DateTime.UtcNow.AddDays(-3)},
            new(){Id=2,ChapterNumber=2,Title="Chương 2: Thế Giới Mới",Status="Draft",CreatedAt=DateTime.UtcNow.AddDays(-2)},
        ]}]
    };
}
