using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Profile;

public class ChangePasswordModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public ChangePasswordModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public IActionResult OnGet()
    {
        return _auth.IsAuthenticated(HttpContext) ? Page() : RedirectToPage("/Auth/Login");
    }

    public async Task<IActionResult> OnPostAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var result = await _api.PutAsync<object>("/api/users/me/password", new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        }, token);

        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true
            ? "Password changed successfully."
            : (result?.Message ?? "Could not change password.");

        return RedirectToPage();
    }
}
