using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly IAuthService _auth;

    public LogoutModel(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await _auth.LogoutAsync(HttpContext);
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _auth.LogoutAsync(HttpContext);
        return RedirectToPage("/Index");
    }
}
