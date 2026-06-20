using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Badges;

public class IndexModel(IApiService api, IAuthService auth, IWebHostEnvironment environment) : PageModel
{
    public List<BadgeDto> Badges { get; set; } = [];
    public List<UserDetailDto> AwardUsers { get; set; } = [];
    public Dictionary<string, List<UserDetailDto>> BadgeOwners { get; set; } = [];
    public string? LoadError { get; set; }

    [BindProperty]
    public IFormFile? CreateIconFile { get; set; }

    [BindProperty]
    public IFormFile? EditIconFile { get; set; }

    [BindProperty]
    public string? CreateIconValue { get; set; }

    [BindProperty]
    public string? EditIconValue { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = auth.GetToken(HttpContext);
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");

        ViewData["AdminSection"] = "badges";
        var currentUser = auth.GetCurrentUser(HttpContext);
        if (currentUser != null)
        {
            ViewData["UserName"] = currentUser.Username;
            ViewData["UserEmail"] = currentUser.Email;
        }

        var result = await api.GetAsync<List<BadgeDto>>("/api/admin/badges", token);
        var usersResult = await api.GetAsync<PagedData<UserDetailDto>>("/api/admin/users?page=1&size=1000", token);
        if (result?.Success == true && result.Data != null)
        {
            Badges = result.Data;
        }
        else
        {
            LoadError = result?.Message ?? "Khong the tai danh sach huy hieu.";
        }

        if (usersResult?.Success == true && usersResult.Data != null)
        {
            AwardUsers = usersResult.Data.Items
                .Where(user => user.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
                .Where(user => !user.Status.Equals("Banned", StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.Username)
                .ToList();

            await LoadBadgeOwnersAsync(token);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string name, string description, string? color)
    {
        var icon = await SaveBadgeIconAsync(CreateIconFile) ?? NormalizeIconValue(CreateIconValue);
        if (TempData["Error"] != null) return RedirectToPage();

        var result = await api.PostAsync<object>("/api/admin/badges", new { key = ToKey(name), name, description, icon, color }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Badge created.";
        else TempData["Error"] = result?.Message ?? "Create badge failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name, string description, string? icon, string? color)
    {
        var uploadedIcon = await SaveBadgeIconAsync(EditIconFile);
        if (TempData["Error"] != null) return RedirectToPage();

        var iconValue = uploadedIcon ?? NormalizeIconValue(EditIconValue) ?? icon;
        var result = await api.PutAsync<object>($"/api/admin/badges/{id}", new { name, description, icon = iconValue, color }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Badge updated.";
        else TempData["Error"] = result?.Message ?? "Update badge failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteAsync<object>($"/api/admin/badges/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Badge deleted.";
        else TempData["Error"] = result?.Message ?? "Delete badge failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAwardAsync(int id, int userId)
    {
        var token = auth.GetToken(HttpContext);
        var userResult = await api.GetAsync<UserDetailDto>($"/api/admin/users/{userId}", token);
        if (userResult?.Success != true || userResult.Data == null)
        {
            TempData["Error"] = userResult?.Message ?? "Không tìm thấy người dùng để trao huy hiệu.";
            return RedirectToPage();
        }

        if (!userResult.Data.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Chỉ được trao huy hiệu cho tài khoản User. Staff/Admin không nhận huy hiệu người đọc.";
            return RedirectToPage();
        }

        if (userResult.Data.Status.Equals("Banned", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Không thể trao huy hiệu cho tài khoản đang bị cấm.";
            return RedirectToPage();
        }

        var result = await api.PostAsync<object>($"/api/admin/badges/{id}/award/{userId}", null, token);
        if (result?.Success == true) TempData["Success"] = "Badge awarded.";
        else TempData["Error"] = result?.Message ?? "Award badge failed.";
        return RedirectToPage();
    }

    public List<UserDetailDto> GetBadgeOwners(BadgeDto badge)
        => BadgeOwners.TryGetValue(GetBadgeIdentity(badge), out var owners) ? owners : [];

    public int GetAwardedCount(BadgeDto badge)
    {
        var owners = GetBadgeOwners(badge);
        return owners.Count > 0 ? owners.Count : badge.EarnedCount;
    }

    private static string ToKey(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        var key = System.Text.RegularExpressions.Regex.Replace(new string(chars).Normalize(System.Text.NormalizationForm.FormC), @"[^a-z0-9]+", "_").Trim('_');
        return string.IsNullOrWhiteSpace(key) ? $"badge_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" : key;
    }

    private async Task LoadBadgeOwnersAsync(string? token)
    {
        BadgeOwners = Badges.ToDictionary(GetBadgeIdentity, _ => new List<UserDetailDto>());

        var detailTasks = AwardUsers.Select(user => api.GetAsync<UserDetailDto>($"/api/admin/users/{user.Id}", token)).ToList();
        while (detailTasks.Count > 0)
        {
            var batch = detailTasks.Take(8).ToList();
            detailTasks.RemoveRange(0, batch.Count);
            var results = await Task.WhenAll(batch);

            foreach (var result in results.Where(result => result?.Success == true && result.Data != null))
            {
                var user = result!.Data!;
                foreach (var badge in user.Badges)
                {
                    var identity = GetBadgeIdentity(badge);
                    if (!BadgeOwners.TryGetValue(identity, out var owners)) continue;
                    owners.Add(user);
                }
            }
        }
    }

    private static string GetBadgeIdentity(BadgeDto badge)
    {
        if (!string.IsNullOrWhiteSpace(badge.Key)) return $"key:{badge.Key.Trim().ToLowerInvariant()}";
        if (badge.Id > 0) return $"id:{badge.Id}";
        return $"name:{badge.Name.Trim().ToLowerInvariant()}";
    }

    private static string? NormalizeIconValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string?> SaveBadgeIconAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        if (file.Length > 1024 * 1024)
        {
            TempData["Error"] = "Badge icon must be 1MB or smaller.";
            return null;
        }

        var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/svg+xml",
            "image/png",
            "image/jpeg",
            "image/webp"
        };

        if (!allowedTypes.Contains(file.ContentType))
        {
            TempData["Error"] = "Badge icon must be SVG, PNG, JPG, or WEBP.";
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = file.ContentType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase) ? ".svg" : ".png";
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(environment.WebRootPath, "uploads", "badges");
        Directory.CreateDirectory(uploadDirectory);

        var filePath = Path.Combine(uploadDirectory, fileName);
        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return $"{Request.Scheme}://{Request.Host}/uploads/badges/{fileName}";
    }
}
