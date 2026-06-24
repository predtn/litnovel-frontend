using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Staff;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<UserDetailDto> StaffUsers { get; set; } = [];
    public List<UserDetailDto> CandidateUsers { get; set; } = [];
    public Dictionary<int, int> StaffModerationCounts { get; set; } = [];
    public int StaffPage { get; set; } = 1;
    public int StaffTotalPages { get; set; } = 1;
    public int StaffTotalElements { get; set; }
    public int CandidatePage { get; set; } = 1;
    public int CandidateTotalPages { get; set; } = 1;
    public int CandidateTotalElements { get; set; }
    public string? LoadError { get; set; }
    private const int PageSize = 5;
    private const int CandidateFetchSize = 100;

    public async Task<IActionResult> OnGetAsync([FromQuery(Name = "staffPage")] int staffPageNumber = 1, [FromQuery(Name = "candidatePage")] int candidatePageNumber = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var token = auth.GetToken(HttpContext);
        StaffPage = Math.Max(1, staffPageNumber);
        CandidatePage = Math.Max(1, candidatePageNumber);

        await LoadStaffUsersAsync(token);
        await LoadStaffModerationCountsAsync(token);
        await LoadCandidateUsersAsync(token);
        return Page();
    }

    private async Task LoadStaffUsersAsync(string? token)
    {
        var allStaff = new List<UserDetailDto>();
        var firstPage = await api.GetAsync<PagedData<UserDetailDto>>($"/api/admin/users?role=Staff&page=1&size={CandidateFetchSize}", token);
        if (firstPage?.Success != true || firstPage.Data == null)
        {
            LoadError = firstPage?.Message ?? "Khong the tai danh sach Staff.";
            return;
        }

        allStaff.AddRange(firstPage.Data.Items);
        for (var page = 2; page <= firstPage.Data.TotalPages; page++)
        {
            var result = await api.GetAsync<PagedData<UserDetailDto>>($"/api/admin/users?role=Staff&page={page}&size={CandidateFetchSize}", token);
            if (result?.Success == true && result.Data != null)
            {
                allStaff.AddRange(result.Data.Items);
            }
        }

        var staffUsers = allStaff
            .Where(user => string.Equals(user.Role, "Staff", StringComparison.OrdinalIgnoreCase))
            .ToList();

        StaffTotalElements = staffUsers.Count;
        StaffTotalPages = Math.Max(1, (int)Math.Ceiling(StaffTotalElements / (double)PageSize));
        StaffPage = Math.Min(StaffPage, StaffTotalPages);
        StaffUsers = staffUsers
            .Skip((StaffPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    private async Task LoadCandidateUsersAsync(string? token)
    {
        var allCandidates = new List<UserDetailDto>();
        var firstPage = await api.GetAsync<PagedData<UserDetailDto>>($"/api/admin/users?role=User&page=1&size={CandidateFetchSize}", token);
        if (firstPage?.Success != true || firstPage.Data == null) return;

        allCandidates.AddRange(firstPage.Data.Items);
        for (var page = 2; page <= firstPage.Data.TotalPages; page++)
        {
            var result = await api.GetAsync<PagedData<UserDetailDto>>($"/api/admin/users?role=User&page={page}&size={CandidateFetchSize}", token);
            if (result?.Success == true && result.Data != null)
            {
                allCandidates.AddRange(result.Data.Items);
            }
        }

        var currentUserId = auth.GetCurrentUser(HttpContext)?.Id;
        var eligibleCandidates = allCandidates
            .Where(user => string.Equals(user.Role, "User", StringComparison.OrdinalIgnoreCase))
            .Where(user => !string.Equals(user.Status, "Banned", StringComparison.OrdinalIgnoreCase))
            .Where(user => currentUserId == null || user.Id != currentUserId.Value)
            .ToList();

        CandidateTotalElements = eligibleCandidates.Count;
        CandidateTotalPages = Math.Max(1, (int)Math.Ceiling(CandidateTotalElements / (double)PageSize));
        CandidatePage = Math.Min(CandidatePage, CandidateTotalPages);
        CandidateUsers = eligibleCandidates
            .Skip((CandidatePage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    private async Task LoadStaffModerationCountsAsync(string? token)
    {
        var counts = new Dictionary<int, int>();
        var firstPage = await api.GetAsync<PagedData<ModerationHistoryDto>>("/api/staff/history?page=1&size=100", token);
        if (firstPage?.Success != true || firstPage.Data == null)
        {
            StaffModerationCounts = counts;
            return;
        }

        AddModerationCounts(firstPage.Data.Items, counts);
        for (var page = 2; page <= firstPage.Data.TotalPages; page++)
        {
            var result = await api.GetAsync<PagedData<ModerationHistoryDto>>($"/api/staff/history?page={page}&size=100", token);
            if (result?.Success == true && result.Data != null)
            {
                AddModerationCounts(result.Data.Items, counts);
            }
        }

        StaffModerationCounts = counts;
    }

    private static void AddModerationCounts(IEnumerable<ModerationHistoryDto> items, Dictionary<int, int> counts)
    {
        foreach (var item in items)
        {
            counts[item.StaffId] = counts.GetValueOrDefault(item.StaffId) + 1;
        }
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
