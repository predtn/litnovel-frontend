using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Staff;
public class ReportsModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<ReportDto> Reports { get; set; } = [];
    public string? StatusFilter { get; set; }
    public int PendingCount { get; set; }
    public ReportsModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(string? status)
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");
        StatusFilter = status ?? "Pending";
        var qs = $"/api/staff/reports?page=1&size=50&status={StatusFilter}";
        var r = await _api.GetAsync<PagedData<ReportDto>>(qs, token);
        Reports = r?.Data?.Items ?? GetMock();
        PendingCount = Reports.Count(x => x.Status == "Pending");
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
    private List<ReportDto> GetMock() =>
    [
        new(){Id=1,ReportType="Inappropriate",Status="Pending",Reporter=new(){Username="user_x"},TargetNovel=new(){Title="Truyện A"},CreatedAt=DateTime.UtcNow.AddHours(-2)},
        new(){Id=2,ReportType="Copyright",Status="Pending",Reporter=new(){Username="user_y"},TargetNovel=new(){Title="Truyện B"},CreatedAt=DateTime.UtcNow.AddHours(-5)},
        new(){Id=3,ReportType="Spam",Status="Resolved",Reporter=new(){Username="user_z"},TargetUser=new(){Username="spammer"},CreatedAt=DateTime.UtcNow.AddDays(-2)},
    ];
}
