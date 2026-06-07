using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Reading;
public class LikesModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public LikesModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var r = await _api.GetAsync<List<NovelSummaryDto>>("/api/users/me/likes", token);
        Novels = r?.Data ?? [new() { Title = "Thiên Đạo Thư Viện", Slug = "thien-dao", RatingAverage = 4.8 }];
        var u = _auth.GetCurrentUser(HttpContext); if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
}
