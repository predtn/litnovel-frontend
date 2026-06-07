using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public List<UserDetailDto> Users { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? Keyword { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }

    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync(string? keyword, string? role, string? status, int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        Keyword = keyword; RoleFilter = role; StatusFilter = status; Page = page;

        var qs = $"/api/admin/users?page={page}&size=20"
            + (string.IsNullOrEmpty(keyword) ? "" : $"&keyword={Uri.EscapeDataString(keyword)}")
            + (string.IsNullOrEmpty(role)    ? "" : $"&role={role}")
            + (string.IsNullOrEmpty(status)  ? "" : $"&status={status}");

        var result = await _api.GetAsync<PagedData<UserDetailDto>>(qs, token);
        Users         = result?.Data?.Items  ?? GetMockUsers();
        TotalPages    = result?.Data?.TotalPages ?? 1;
        TotalElements = result?.Data?.TotalElements ?? Users.Count;

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        return Page();
    }

    public async Task<IActionResult> OnPostBanAsync(int userId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/admin/users/{userId}/ban", new { reason = "Admin action" }, token);
        TempData["Success"] = "Đã cấm người dùng.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnbanAsync(int userId)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PostAsync<object>($"/api/admin/users/{userId}/unban", null, token);
        TempData["Success"] = "Đã bỏ cấm người dùng.";
        return RedirectToPage();
    }

    private List<UserDetailDto> GetMockUsers() =>
    [
        new() { Id = 1, Username = "admin_user",  Email = "admin@litnovel.com",  Role = "Admin",  Status = "Online",  CreatedAt = DateTime.UtcNow.AddDays(-365) },
        new() { Id = 2, Username = "staff_user",  Email = "staff@litnovel.com",  Role = "Staff",  Status = "Online",  CreatedAt = DateTime.UtcNow.AddDays(-180) },
        new() { Id = 3, Username = "john_reader", Email = "john@example.com",    Role = "User",   Status = "Online",  CreatedAt = DateTime.UtcNow.AddDays(-60) },
        new() { Id = 4, Username = "bad_actor",   Email = "bad@example.com",     Role = "User",   Status = "Banned",  CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new() { Id = 5, Username = "author_pro",  Email = "author@example.com",  Role = "User",   Status = "Offline", CreatedAt = DateTime.UtcNow.AddDays(-90) },
    ];
}
