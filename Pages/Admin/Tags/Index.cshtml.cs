using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.Tags;
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<TagDto> Tags { get; set; } = [];
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var r = await _api.GetAsync<List<TagDto>>("/api/tags");
        Tags = r?.Data ?? [new(){Id=1,Name="Long tộc",NovelCount=45},new(){Id=2,Name="Hậu cung",NovelCount=30},new(){Id=3,Name="Phế vật tái sinh",NovelCount=60},new(){Id=4,Name="Mạnh mẽ",NovelCount=120}];
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
    public async Task<IActionResult> OnPostAddAsync(string name) { await _api.PostAsync<object>("/api/admin/tags", new{name}, _auth.GetToken(HttpContext)); TempData["Success"]="Đã thêm!"; return RedirectToPage(); }
    public async Task<IActionResult> OnPostDeleteAsync(int id) { await _api.DeleteAsync<object>($"/api/admin/tags/{id}", _auth.GetToken(HttpContext)); TempData["Success"]="Đã xóa!"; return RedirectToPage(); }
}
