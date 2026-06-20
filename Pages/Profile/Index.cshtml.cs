using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public UserDetailDto? UserDetail { get; set; }

    public IndexModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var result = await _api.GetAsync<UserDetailDto>("/api/users/me", token);
        if (result?.Success != true || result.Data == null)
        {
            TempData["Error"] = result?.Message ?? "Could not load profile.";
            return RedirectToPage("/Index");
        }

        UserDetail = result.Data;
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"] = user.Avatar;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? bio, string? avatar)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var result = await _api.PutAsync<object>("/api/users/me", new UpdateProfileRequest { Bio = bio, Avatar = avatar }, token);
        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true
            ? "Profile updated successfully."
            : (result?.Message ?? "Could not update profile.");

        return RedirectToPage();
    }
}
