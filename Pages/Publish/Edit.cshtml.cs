using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Publish;
public class EditModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public NovelDetailDto? Novel { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];
    public EditModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var t1 = _api.GetAsync<NovelDetailDto>($"/api/novels/{id}", token);
        var t2 = _api.GetAsync<List<CategoryDto>>("/api/categories");
        await Task.WhenAll(t1, t2);
        Novel = t1.Result?.Data ?? new() { Id=id, Title="Long Vương Truyền Thuyết", Status="Draft", Category=new(){Id=1,Name="Tiên hiệp"} };
        Categories = t2.Result?.Data ?? [new(){Id=1,Name="Tiên hiệp"},new(){Id=2,Name="Huyền huyễn"}];
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id, string title, string? description, int? categoryId, string? coverImage, string? status)
    {
        var token = _auth.GetToken(HttpContext);
        var r = await _api.PutAsync<object>($"/api/novels/{id}", new { title, description, categoryId, coverImage, status }, token);
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã cập nhật!" : (r?.Message ?? "Lỗi cập nhật.");
        return RedirectToPage(new { id });
    }
}
