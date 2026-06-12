using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class PendingChaptersModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public List<PendingChapterDto> Chapters { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; } = 0;

    public PendingChaptersModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");

        Page = page < 1 ? 1 : page;
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }

        var result = await _api.GetAsync<PagedData<PendingChapterDto>>($"/api/staff/chapters/pending?page={Page}&size=20", token);
        var data = result?.Data;
        Chapters      = data?.Items ?? [];
        TotalPages    = data?.TotalPages ?? 1;
        TotalElements = data?.TotalElements ?? 0;

        return Page();
    }
}
