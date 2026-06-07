using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Profile;
public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public UserDetailDto? UserDetail { get; set; }
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var result = await _api.GetAsync<UserDetailDto>("/api/users/me", token);
        UserDetail = result?.Data ?? GetMockUser();
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string email, string? bio, string? avatar)
    {
        var token = _auth.GetToken(HttpContext);
        var result = await _api.PutAsync<object>("/api/users/me", new { email, bio, avatar }, token);
        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true ? "Cập nhật thành công!" : (result?.Message ?? "Có lỗi xảy ra.");
        return RedirectToPage();
    }
    private UserDetailDto GetMockUser() => new()
    {
        Id = 1, Username = "user_demo", Email = "user@litnovel.com", Role = "User",
        Bio = "Người yêu thích truyện tiên hiệp và huyền huyễn.",
        Reputation = 1250,
        Stats = new() { NovelsCreated = 3, ChaptersPublished = 45, FavoritesCount = 28, CommentsCount = 142 },
        Badges = [new() { Key = "first_novel", Name = "Tác giả đầu tiên", Icon = "✍️" }, new() { Key = "reader_100", Name = "Độc giả chăm chỉ", Icon = "📚" }],
        CreatedAt = DateTime.UtcNow.AddDays(-180)
    };
}
