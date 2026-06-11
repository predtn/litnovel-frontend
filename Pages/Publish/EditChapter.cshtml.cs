using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Publish;
public class EditChapterModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public int NovelId { get; set; }
    public ChapterDetailDto? Chapter { get; set; }
    public EditChapterModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(int novelId, int chapterId)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        NovelId = novelId;
        var r = await _api.GetAsync<ChapterDetailDto>($"/api/chapters/{chapterId}", token);
        Chapter = r?.Data ?? new() { Id=chapterId, ChapterNumber=1, Title="Chương 1: Thức Tỉnh", Content="<p>Nội dung chương...</p>", Status="Draft" };
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int novelId, int chapterId, int chapterNumber, string title, string content)
    {
        var token = _auth.GetToken(HttpContext);
        var r = await _api.PutAsync<object>($"/api/chapters/{chapterId}", new { chapterNumber, title, content }, token);
        TempData[r?.Success==true?"Success":"Error"] = r?.Success==true?"Đã lưu!":"Lỗi lưu.";
        return RedirectToPage(new { novelId, chapterId });
    }
    public async Task<IActionResult> OnPostSubmitAsync(int novelId, int chapterId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/chapters/{chapterId}/submit", null, token);
        TempData["Success"] = "Đã gửi duyệt!";
        return RedirectToPage("/Publish/Manage", new { id = novelId });
    }
}
