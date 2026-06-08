using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _auth;

    public bool Success { get; set; }
    [BindProperty] public string Token { get; set; } = "";

    public ResetPasswordModel(IAuthService auth)
    {
        _auth = auth;
    }

    public void OnGet(string? token)
    {
        Token = token ?? "";
    }

    public async Task<IActionResult> OnPostAsync(string password, string confirmPassword)
    {
        var (success, error) = await _auth.ResetPasswordAsync(Token, password, confirmPassword);
        if (success)
        {
            Success = true;
            return Page();
        }

        TempData["Error"] = error;
        return Page();
    }
}
