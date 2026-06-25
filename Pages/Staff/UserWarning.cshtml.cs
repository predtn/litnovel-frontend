using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class UserWarningModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public int? TargetUserId { get; set; }
    public string? TargetUsername { get; set; }
    public string? ErrorMessage { get; set; }

    public UserWarningModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public IActionResult OnGet([FromQuery] int? userId = null, [FromQuery] string? username = null)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }

        TargetUserId   = userId;
        TargetUsername = username;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] int userId, [FromForm] string message, [FromForm] string severity = "Minor")
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }

        var payload = new { reason = message, severity };
        var result = await _api.PostAsync<object>($"/api/staff/users/{userId}/warn", payload, token);
        if (result?.Success == true)
        {
            TempData["SuccessMessage"] = "Cảnh báo đã được gửi thành công đến người dùng!";
            return RedirectToPage("/Staff/Reports");
        }

        TargetUserId  = userId;
        ErrorMessage  = result?.Message ?? "Có lỗi xảy ra.";
        return Page();
    }
}
