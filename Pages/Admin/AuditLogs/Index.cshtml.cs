using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.AuditLogs;
public class AuditLogEntry { public int Id; public string Username=""; public string Action=""; public string EntityType=""; public int EntityId; public string IpAddress=""; public DateTime CreatedAt; }
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<AuditLogEntry> Logs { get; set; } = [];
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var u = _auth.GetCurrentUser(HttpContext); if(u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        Logs = [
            new(){Id=1,Username="admin_user",Action="BanUser",EntityType="User",EntityId=45,IpAddress="192.168.1.1",CreatedAt=DateTime.UtcNow.AddMinutes(-5)},
            new(){Id=2,Username="staff_user",Action="ApproveNovel",EntityType="Novel",EntityId=120,IpAddress="192.168.1.2",CreatedAt=DateTime.UtcNow.AddMinutes(-15)},
            new(){Id=3,Username="admin_user",Action="DeleteComment",EntityType="Comment",EntityId=890,IpAddress="192.168.1.1",CreatedAt=DateTime.UtcNow.AddHours(-1)},
            new(){Id=4,Username="staff_user",Action="RejectChapter",EntityType="Chapter",EntityId=340,IpAddress="192.168.1.2",CreatedAt=DateTime.UtcNow.AddHours(-2)},
        ];
        return Page();
    }
}
