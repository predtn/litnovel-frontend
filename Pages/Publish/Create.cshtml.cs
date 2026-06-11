using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Publish;
public class CreateModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<CategoryDto> Categories { get; set; } = [];
    public CreateModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var r = await _api.GetAsync<List<CategoryDto>>("/api/categories");
        Categories = r?.Data ?? [new(){Id=1,Name="Tiên hiệp"},new(){Id=2,Name="Huyền huyễn"},new(){Id=3,Name="Đô thị"}];
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string title, string? description, int? categoryId, string? coverImage, string? tags)
    {
        if (string.IsNullOrEmpty(title)) { TempData["Error"] = "Tiêu đề không được trống."; return Page(); }
        var token = _auth.GetToken(HttpContext);
        var tagList = tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? [];
        var r = await _api.PostAsync<NovelDetailDto>("/api/novels", new { title, description, categoryId, coverImage, tags = tagList }, token);
        if (r?.Success == true && r.Data?.Id > 0) return RedirectToPage("/Publish/Manage", new { id = r.Data.Id });
        TempData["Error"] = r?.Message ?? "Có lỗi khi tạo tiểu thuyết.";
        return Page();
    }
}
