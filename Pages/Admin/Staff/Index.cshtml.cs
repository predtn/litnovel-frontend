using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Staff;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<UserDetailDto> StaffUsers { get; set; } = [];
    public List<UserDetailDto> CandidateUsers { get; set; } = [];
    public int StaffPage { get; set; } = 1;
    public int StaffTotalPages { get; set; } = 1;
    public int StaffTotalElements { get; set; }
    public int CandidatePage { get; set; } = 1;
    public int CandidateTotalPages { get; set; } = 1;
    public int CandidateTotalElements { get; set; }
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(int staffPage = 1, int candidatePage = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var token = auth.GetToken(HttpContext);
        StaffPage = Math.Max(1, staffPage);
        CandidatePage = Math.Max(1, candidatePage);

        var staff = await api.GetAsync<PagedData<UserDetailDto>>($"/api/admin/users?role=Staff&page={StaffPage}&size=5", token);
        var users = await api.GetAsync<PagedData<UserDetailDto>>($"/api/admin/users?role=User&page={CandidatePage}&size=5", token);
        if (staff?.Success == true && staff.Data != null)
        {
            StaffUsers = staff.Data.Items
                .Where(user => string.Equals(user.Role, "Staff", StringComparison.OrdinalIgnoreCase))
                .ToList();
            StaffTotalPages = Math.Max(1, staff.Data.TotalPages);
            StaffTotalElements = staff.Data.TotalElements;
        }
        else
        {
            LoadError = staff?.Message ?? "Khong the tai danh sach Staff.";
        }

        if (users?.Success == true && users.Data != null)
        {
            var currentUserId = auth.GetCurrentUser(HttpContext)?.Id;
            CandidateUsers = users.Data.Items
                .Where(user => string.Equals(user.Role, "User", StringComparison.OrdinalIgnoreCase))
                .Where(user => currentUserId == null || user.Id != currentUserId.Value)
                .ToList();
            CandidateTotalPages = Math.Max(1, users.Data.TotalPages);
            CandidateTotalElements = users.Data.TotalElements;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAssignAsync(int userId)
    {
        var token = auth.GetToken(HttpContext);
        var result = await api.PostAsync<object>($"/api/admin/users/{userId}/assign-staff", null, token);
        if (result?.Success == true)
        {
            await SendUserNotificationAsync(userId, "Tài khoản của bạn đã được cấp quyền Staff. Staff Dashboard hiện đã sẵn sàng để sử dụng.", token);
            TempData["Success"] = "Đã cấp quyền Staff và gửi thông báo cho người dùng.";
        }
        else TempData["Error"] = result?.Message ?? "Assign Staff failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevokeAsync(int userId)
    {
        var result = await api.PostAsync<object>($"/api/admin/users/{userId}/revoke-staff", null, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Removed Staff role.";
        else TempData["Error"] = result?.Message ?? "Revoke Staff failed.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "staff";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }

    private async Task<ApiResponse<object>?> SendUserNotificationAsync(int userId, string message, string? token)
    {
        return await api.PostAsync<object>("/api/admin/notifications", new
        {
            notificationType = "SystemAlert",
            message,
            targetAll = false,
            targetUserId = userId
        }, token);
    }

}
