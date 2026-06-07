using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Staff;
public class PendingNovelsModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public PendingNovelsModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");
        var r = await _api.GetAsync<PagedData<NovelSummaryDto>>("/api/staff/novels/pending?page=1&size=50", token);
        Novels = r?.Data?.Items ?? [new(){Id=10,Title="Kiếm Đạo Vô Song",Author=new(){Username="author_a"},UpdatedAt=DateTime.UtcNow.AddHours(-2),Category=new(){Name="Kiếm hiệp"}},new(){Id=11,Title="Thần Hoàng Tái Thế",Author=new(){Username="author_b"},UpdatedAt=DateTime.UtcNow.AddHours(-5)}];
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
}
