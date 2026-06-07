using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Staff;
public class NovelReviewModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public NovelDetailDto? Novel { get; set; }
    public NovelReviewModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");
        var r = await _api.GetAsync<NovelDetailDto>($"/api/novels/{id}", token);
        Novel = r?.Data ?? new(){Id=id,Title="Kiếm Đạo Vô Song",Author=new(){Username="author_a"},Description="<p>Một thế giới...</p>",TotalChapters=5,Volumes=[new(){Title="Tập 1",Chapters=[new(){Id=1,ChapterNumber=1,Title="Chương 1: Khởi Đầu",Status="Pending",CreatedAt=DateTime.UtcNow}]}]};
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
    public async Task<IActionResult> OnPostApproveAsync(int novelId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/staff/novels/{novelId}/approve", null, token);
        TempData["Success"] = "Đã duyệt tiểu thuyết!";
        return RedirectToPage("/Staff/PendingNovels");
    }
    public async Task<IActionResult> OnPostRejectAsync(int novelId, string reason)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/staff/novels/{novelId}/reject", new { reason }, token);
        TempData["Success"] = "Đã từ chối tiểu thuyết.";
        return RedirectToPage("/Staff/PendingNovels");
    }
}
