using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Notifications;

public class IndexModel(IAuthService auth) : PageModel
{
    public List<SentNotificationDto> Notifications { get; set; } = [];
    public List<UserDetailDto> RecipientUsers { get; set; } = [];
    public string? LoadError { get; set; }

    public IActionResult OnGet()
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        return RedirectToPage("/Admin/Dashboard");
    }

    public IActionResult OnPostSend()
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        return RedirectToPage("/Admin/Dashboard");
    }
}
