using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Reading;
public class HistoryModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<ReadingProgressDto> History { get; set; } = [];
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public HistoryModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        PageNumber = GetQueryInt("page", page);
        var r = await _api.GetAsync<PagedData<ReadingProgressDto>>($"/api/users/me/reading-history?page={PageNumber}&size=10", token);
        var data = r?.Data;
        if (data is { TotalPages: > 0 } && PageNumber > data.TotalPages)
        {
            PageNumber = data.TotalPages;
            data = (await _api.GetAsync<PagedData<ReadingProgressDto>>($"/api/users/me/reading-history?page={PageNumber}&size=6", token))?.Data;
        }

        History = data?.Items ?? [];
        TotalPages = data?.TotalPages ?? 0;
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int novelId, int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.DeleteAsync<object>($"/api/users/me/reading-history/{novelId}", token);
        return RedirectToPage(new { page });
    }

    private int GetQueryInt(string key, int fallback)
    {
        if (Request.Query.TryGetValue(key, out var values)
            && int.TryParse(values.FirstOrDefault(), out var value)
            && value > 0)
        {
            return value;
        }

        return Math.Max(1, fallback);
    }
}
