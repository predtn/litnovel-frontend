using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Profile;
public class ChangePasswordModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public ChangePasswordModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public IActionResult OnGet() { if (!_auth.IsAuthenticated(HttpContext)) return RedirectToPage("/Auth/Login"); return Page(); }
    public async Task<IActionResult> OnPostAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword) { TempData["Error"] = "Mật khẩu xác nhận không khớp."; return Page(); }
        var token = _auth.GetToken(HttpContext);
        var result = await _api.PutAsync<object>("/api/users/me/password", new { currentPassword, newPassword }, token);
        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true ? "Đổi mật khẩu thành công!" : (result?.Message ?? "Mật khẩu hiện tại không đúng.");
        return RedirectToPage();
    }
}
