using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Publish;
public class CreateChapterModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public int NovelId { get; set; }
    public int VolumeId { get; set; }
    public string VolumeName { get; set; } = "";
    public int NextChapterNum { get; set; } = 1;
    public CreateChapterModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(int novelId, int volumeId)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        NovelId = novelId; VolumeId = volumeId; VolumeName = "Tập 1: Tái Sinh"; NextChapterNum = 1;
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int novelId, int volumeId, int chapterNumber, string title, string content, DateTime? releaseDate)
        => await Save(novelId, volumeId, chapterNumber, title, content, releaseDate, "Pending");
    public async Task<IActionResult> OnPostSaveDraftAsync(int novelId, int volumeId, int chapterNumber, string title, string content, DateTime? releaseDate)
        => await Save(novelId, volumeId, chapterNumber, title, content, releaseDate, "Draft");
    private async Task<IActionResult> Save(int novelId, int volumeId, int chapterNumber, string title, string content, DateTime? releaseDate, string status)
    {
        var token = _auth.GetToken(HttpContext);
        var r = await _api.PostAsync<ChapterDetailDto>($"/api/novels/{novelId}/volumes/{volumeId}/chapters", new { chapterNumber, title, content, releaseDate, status }, token);
        if (r?.Success == true) return RedirectToPage("/Publish/Manage", new { id = novelId });
        TempData["Error"] = r?.Message ?? "Lỗi tạo chương.";
        return Page();
    }
}
