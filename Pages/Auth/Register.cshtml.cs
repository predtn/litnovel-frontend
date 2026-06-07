using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace litnovel_frontend.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _auth;
    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";
    public Dictionary<string, string> Errors { get; set; } = [];

    public RegisterModel(IAuthService auth) { _auth = auth; }

    public IActionResult OnGet()
    {
        if (_auth.IsAuthenticated(HttpContext)) return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Client-side validation
        if (Username.Length < 3 || Username.Length > 50) Errors["username"] = "Tên đăng nhập phải từ 3–50 ký tự.";
        else if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$")) Errors["username"] = "Chỉ chấp nhận chữ cái, số và dấu _";
        if (!Email.Contains('@')) Errors["email"] = "Email không hợp lệ.";
        if (Password.Length < 8) Errors["password"] = "Mật khẩu phải tối thiểu 8 ký tự.";
        else if (!Regex.IsMatch(Password, @"[A-Z]")) Errors["password"] = "Mật khẩu phải có ít nhất 1 chữ hoa.";
        else if (!Regex.IsMatch(Password, @"[0-9]")) Errors["password"] = "Mật khẩu phải có ít nhất 1 chữ số.";
        if (Password != ConfirmPassword) Errors["password"] = "Mật khẩu xác nhận không khớp.";
        if (Errors.Any()) return Page();

        var (success, error) = await _auth.RegisterAsync(HttpContext, Username, Email, Password);
        if (success)
        {
            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Auth/Login");
        }
        TempData["Error"] = error;
        return Page();
    }
}
