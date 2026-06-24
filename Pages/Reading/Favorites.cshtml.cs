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
        var r = await _api.GetAsync<PagedData<NovelSummaryDto>>("/api/users/me/favorites?page=1&size=50", token);
        Novels = r?.Data?.Items ?? [];
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int novelId)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var result = await _api.DeleteAsync<object>($"/api/novels/{novelId}/favorites", token);
        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true
            ? "Đã bỏ yêu thích."
            : (result?.Message ?? "Không thể bỏ yêu thích.");
        return RedirectToPage();
    }
}
