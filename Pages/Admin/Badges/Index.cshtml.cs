using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.Badges;
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<BadgeDto> Badges { get; set; } = [];
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var r = await _api.GetAsync<List<BadgeDto>>("/api/admin/badges", token);
        Badges = r?.Data ?? [new(){Key="first_novel",Name="Tác giả đầu tiên",Icon="✍️",Description="Tạo tiểu thuyết đầu tiên",EarnedCount=234},new(){Key="reader_100",Name="Độc giả chăm chỉ",Icon="📚",Description="Đọc 100 chương",EarnedCount=1205},new(){Key="liked_1000",Name="Nội dung chất lượng",Icon="⭐",Description="Nhận 1000 lượt thích",EarnedCount=12}];
        var u = _auth.GetCurrentUser(HttpContext); if(u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
    public async Task<IActionResult> OnPostAddAsync(string icon, string name, string? description) { await _api.PostAsync<object>("/api/admin/badges", new{icon,name,description}, _auth.GetToken(HttpContext)); TempData["Success"]="Đã thêm!"; return RedirectToPage(); }
}
