using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Reading;

public class FavoritesModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<NovelSummaryDto> Novels { get; set; } = [];
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }

    public FavoritesModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        PageNumber = GetQueryInt("page", page);
        var result = await _api.GetAsync<PagedData<NovelSummaryDto>>($"/api/users/me/favorites?page={PageNumber}&size=12", token);
        var data = result?.Data;
        if (data is { TotalPages: > 0 } && PageNumber > data.TotalPages)
        {
            PageNumber = data.TotalPages;
            data = (await _api.GetAsync<PagedData<NovelSummaryDto>>($"/api/users/me/favorites?page={PageNumber}&size=12", token))?.Data;
        }

        Novels = data?.Items ?? [];
        TotalPages = data?.TotalPages ?? 0;

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserAvatar"] = user.Avatar;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int novelId, int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var result = await _api.DeleteAsync<object>($"/api/novels/{novelId}/favorites", token);
        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true
            ? "Đã bỏ yêu thích."
            : (result?.Message ?? "Không thể bỏ yêu thích.");

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
