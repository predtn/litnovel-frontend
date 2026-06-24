using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    private readonly IWebHostEnvironment _environment;

    public UserDetailDto? UserDetail { get; set; }
    [BindProperty] public IFormFile? AvatarFile { get; set; }

    public IndexModel(IApiService api, IAuthService auth, IWebHostEnvironment environment)
    {
        _api = api;
        _auth = auth;
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var result = await _api.GetAsync<UserDetailDto>("/api/users/me", token);
        if (result?.Success != true || result.Data == null)
        {
            TempData["Error"] = result?.Message ?? "Could not load profile.";
            return RedirectToPage("/Index");
        }

        UserDetail = result.Data;
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"] = user.Avatar;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? bio, string? avatar)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var uploadedAvatar = await SaveAvatarAsync();
        if (!ModelState.IsValid)
        {
            await LoadProfileForPageAsync(token);
            return Page();
        }

        avatar = uploadedAvatar ?? avatar;
        var result = await _api.PutAsync<object>("/api/users/me", new UpdateProfileRequest { Bio = bio, Avatar = avatar }, token);
        if (result?.Success == true)
        {
            await _auth.ValidateSessionAsync(HttpContext);
        }

        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true
            ? "Cập nhật hồ sơ thành công."
            : (result?.Message ?? "Không thể cập nhật hồ sơ.");

        return RedirectToPage();
    }

    private async Task LoadProfileForPageAsync(string token)
    {
        var result = await _api.GetAsync<UserDetailDto>("/api/users/me", token);
        UserDetail = result?.Data;

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"] = UserDetail?.Avatar ?? user.Avatar;
        }
    }

    private async Task<string?> SaveAvatarAsync()
    {
        if (AvatarFile == null || AvatarFile.Length == 0) return null;

        if (AvatarFile.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(AvatarFile), "Ảnh đại diện phải nhỏ hơn hoặc bằng 2MB.");
            return null;
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(AvatarFile.ContentType))
        {
            ModelState.AddModelError(nameof(AvatarFile), "Ảnh đại diện phải là JPG, PNG hoặc WEBP.");
            return null;
        }

        var extension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(AvatarFile), "Ảnh đại diện phải là JPG, PNG hoặc WEBP.");
            return null;
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine("uploads", "avatars", fileName);
        var outputDirectory = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, fileName);
        await using (var stream = System.IO.File.Create(outputPath))
        {
            await AvatarFile.CopyToAsync(stream);
        }

        return $"{Request.Scheme}://{Request.Host}/{relativePath.Replace('\\', '/')}";
    }
}
