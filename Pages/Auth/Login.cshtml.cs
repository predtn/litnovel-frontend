using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _auth;
    [BindProperty] public string Identifier { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }

    public LoginModel(IAuthService auth) { _auth = auth; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (_auth.IsAuthenticated(HttpContext))
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect(GetRoleHome(_auth.GetCurrentUser(HttpContext)?.Role));
        }
        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(Identifier) || string.IsNullOrWhiteSpace(Password))
        {
            TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
            return Page();
        }
        var (success, error, user) = await _auth.LoginAsync(HttpContext, Identifier, Password);
        if (success)
        {
            TempData["ToastMessage"] = "Đăng nhập thành công.";
            TempData["ToastType"] = "success";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect(GetRoleHome(user?.Role));
        }
        TempData["Error"] = error;
        return Page();
    }

    private static string GetRoleHome(string? role) => role?.Trim().ToLowerInvariant() switch
    {
        "admin" => "/Admin/Dashboard",
        "staff" => "/Staff/Dashboard",
        _ => "/"
    };
}
