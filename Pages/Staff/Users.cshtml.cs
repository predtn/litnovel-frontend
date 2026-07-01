using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace litnovel_frontend.Pages.Staff
{
    public class UsersModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public UsersModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public class StaffUserItem
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTimeOffset CreatedAt { get; set; }
            public int WarningCount { get; set; }
        }

        public List<StaffUserItem> Users { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalElements { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public string ActionMessage { get; set; } = string.Empty;
        public bool ActionSuccess { get; set; } = false;

        public async Task<IActionResult> OnGetAsync([FromQuery] int p = 1)
        {
            CurrentPage = p;
            
            var query = $"page={p}&size=20";
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                query += $"&searchKeyword={Uri.EscapeDataString(Keyword)}";
            }

            var token = _authService.GetToken(HttpContext);
            if (string.IsNullOrEmpty(token) || !_authService.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Auth/Login");

            var res = await _apiService.GetAsync<PagedData<StaffUserItem>>($"/api/staff/users?{query}", token);
            if (res != null && res.Success && res.Data != null)
            {
                Users = res.Data.Items;
                TotalPages = res.Data.TotalPages;
                TotalElements = res.Data.TotalElements;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBanUserAsync(int userId, string reason, int p = 1)
        {
            var token = _authService.GetToken(HttpContext);
            if (string.IsNullOrEmpty(token) || !_authService.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Auth/Login");

            var res = await _apiService.PutAsync<object>($"/api/staff/users/{userId}/status", new { status = "Banned", reason }, token);
            ActionSuccess = res?.Success ?? false;
            ActionMessage = ActionSuccess ? "Đã khóa tài khoản thành công." : (res?.Message ?? "Có lỗi xảy ra.");
            return RedirectToPage(new { p, Keyword });
        }

        public async Task<IActionResult> OnPostUnbanUserAsync(int userId, string reason, int p = 1)
        {
            var token = _authService.GetToken(HttpContext);
            if (string.IsNullOrEmpty(token) || !_authService.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Auth/Login");

            var res = await _apiService.PutAsync<object>($"/api/staff/users/{userId}/status", new { status = "Online", reason }, token);
            ActionSuccess = res?.Success ?? false;
            ActionMessage = ActionSuccess ? "Đã mở khóa tài khoản thành công." : (res?.Message ?? "Có lỗi xảy ra.");
            return RedirectToPage(new { p, Keyword });
        }
    }
}
