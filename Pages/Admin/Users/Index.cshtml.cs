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
    public string? LoadError { get; set; }

    public IndexModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync(string? keyword, string? role, string? status, [FromQuery(Name = "page")] int pageNumber = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");

        Keyword = keyword;
        RoleFilter = "User";
        StatusFilter = status;
        Page = Math.Max(1, pageNumber);

        var qs = "/api/admin/users" + ODataQuery.Build(
            page: Page,
            size: 5,
            filters:
            [
                ODataQuery.Eq("Role", RoleFilter),
                ODataQuery.Eq("Status", StatusFilter),
                ODataQuery.ContainsAny(Keyword ?? "", "Username", "Email")
            ]);

        var result = await _api.GetAsync<PagedData<UserDetailDto>>(qs, token);
        if (result?.Success == true && result.Data != null)
        {
            Users = result.Data.Items
                .Where(user => user.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
                .ToList();
            TotalPages = result.Data.TotalPages > 0
                ? result.Data.TotalPages
                : Math.Max(1, (int)Math.Ceiling(result.Data.TotalElements / 5.0));
            TotalElements = result.Data.TotalElements;
        }
        else
        {
            LoadError = result?.Message ?? "Không thể tải danh sách người dùng.";
            TotalPages = 1;
            TotalElements = 0;
        }

        SetShell();
        return Page();
    }

    private void SetShell()
    {
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
        }

        ViewData["AdminSection"] = "users";
    }
}
