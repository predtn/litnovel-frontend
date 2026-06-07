using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public StatisticsDto Stats { get; set; } = new();

    public DashboardModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");

        var result = await _api.GetAsync<StatisticsDto>("/api/admin/statistics", token);
        Stats = result?.Data ?? GetMockStats();

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        return Page();
    }

    private static StatisticsDto GetMockStats() => new()
    {
        Users      = new() { Total = 12450, NewThisWeek = 234, Banned = 12 },
        Novels     = new() { Total = 3200,  Ongoing = 2100, Pending = 45, NewThisMonth = 120 },
        Chapters   = new() { Total = 48000, PublishedThisWeek = 340 },
        Reports    = new() { Total = 890,   Open = 23, ResolvedThisMonth = 67 },
        Engagement = new() { TotalComments = 125000, TotalRatings = 45000, TotalFavorites = 89000 }
    };
}
