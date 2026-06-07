using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Auth;
public class ResetPasswordModel : PageModel
{
    private readonly IApiService _api;
    public bool Success { get; set; }
    [BindProperty] public string Token { get; set; } = "";
    public ResetPasswordModel(IApiService api) { _api = api; }
    public void OnGet(string? token) { Token = token ?? ""; }
    public async Task<IActionResult> OnPostAsync(string password, string confirmPassword)
    {
        if (password != confirmPassword) { TempData["Error"] = "Mật khẩu xác nhận không khớp."; return Page(); }
        var result = await _api.PostAsync<object>("/api/auth/reset-password", new { token = Token, newPassword = password });
        if (result?.Success == true) { Success = true; return Page(); }
        TempData["Error"] = result?.Message ?? "Token không hợp lệ hoặc đã hết hạn.";
        return Page();
    }
}
