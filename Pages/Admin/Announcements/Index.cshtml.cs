using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.Announcements;
public class AnnouncementDto { public int Id; public string Title=""; public string Content=""; public string Type="Info"; public DateTime StartDate; public DateTime EndDate; }
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<AnnouncementDto> Announcements { get; set; } = [];
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var u = _auth.GetCurrentUser(HttpContext); if(u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        Announcements = [new(){Id=1,Title="Bảo trì hệ thống",Content="Hệ thống sẽ bảo trì lúc 2:00 AM ngày 10/06.",Type="Warning",StartDate=DateTime.UtcNow,EndDate=DateTime.UtcNow.AddDays(3)},new(){Id=2,Title="Ra mắt tính năng mới!",Content="Chào mừng tính năng đọc offline.",Type="Success",StartDate=DateTime.UtcNow.AddDays(-2),EndDate=DateTime.UtcNow.AddDays(5)}];
        return Page();
    }
    public async Task<IActionResult> OnPostAddAsync(string title, string? content, string type, int durationDays) { await _api.PostAsync<object>("/api/admin/announcements", new{title,content,type,endDate=DateTime.UtcNow.AddDays(durationDays)}, _auth.GetToken(HttpContext)); TempData["Success"]="Đã tạo!"; return RedirectToPage(); }
    public async Task<IActionResult> OnPostDeleteAsync(int id) { await _api.DeleteAsync<object>($"/api/admin/announcements/{id}", _auth.GetToken(HttpContext)); TempData["Success"]="Đã xóa!"; return RedirectToPage(); }
}
