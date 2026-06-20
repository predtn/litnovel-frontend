using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly IAuthService _auth;

    public bool Sent { get; set; }
    [BindProperty] public string Email { get; set; } = "";

    public ForgotPasswordModel(IAuthService auth)
    {
        _auth = auth;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _auth.ForgotPasswordAsync(Email);
        if (!success)
        {
            TempData["Error"] = error;
            return Page();
        }

        Sent = true;
        return Page();
    }
}
