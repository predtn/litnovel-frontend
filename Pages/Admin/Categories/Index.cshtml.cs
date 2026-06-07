using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.Categories;
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<CategoryDto> Categories { get; set; } = [];
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var r = await _api.GetAsync<List<CategoryDto>>("/api/categories");
        Categories = r?.Data ?? [new(){Id=1,Name="Tiên hiệp",NovelCount=120},new(){Id=2,Name="Huyền huyễn",NovelCount=85},new(){Id=3,Name="Đô thị",NovelCount=60},new(){Id=4,Name="Kiếm hiệp",NovelCount=45}];
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
    public async Task<IActionResult> OnPostAddAsync(string name)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>("/api/admin/categories", new { name }, token);
        TempData["Success"] = "Đã thêm thể loại!"; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostEditAsync(int id, string name)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PutAsync<object>($"/api/admin/categories/{id}", new { name }, token);
        TempData["Success"] = "Đã cập nhật!"; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.DeleteAsync<object>($"/api/admin/categories/{id}", token);
        TempData["Success"] = "Đã xóa!"; return RedirectToPage();
    }
}
