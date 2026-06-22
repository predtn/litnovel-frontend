using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Reading;
public class FavoritesModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public FavoritesModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var r = await _api.GetAsync<List<NovelSummaryDto>>("/api/users/me/favorites", token);
        Novels = r?.Data ?? [];
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int novelId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.DeleteAsync<object>($"/api/novels/{novelId}/favorites", token);
        return RedirectToPage();
    }
}
