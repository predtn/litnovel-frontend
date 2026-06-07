using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly IApiService _api;
    public bool Sent { get; set; }
    [BindProperty] public string Email { get; set; } = "";

    public ForgotPasswordModel(IApiService api) { _api = api; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        await _api.PostAsync<object>("/api/auth/forgot-password", new { email = Email });
        Sent = true;
        return Page();
    }
}
