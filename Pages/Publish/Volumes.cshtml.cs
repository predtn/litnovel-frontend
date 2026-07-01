using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class VolumesModel : PublishPageModel
{
    public NovelDetailDto Novel { get; set; } = new();
    public List<VolumeDto> AllVolumes { get; set; } = [];
    public List<VolumeDto> Volumes { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    [BindProperty] public VolumeUpsertRequest Input { get; set; } = new();
    private const int PageSize = 10;

    public VolumesModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int novelId, int page = 1)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        Page = Math.Max(1, page);
        var loaded = await LoadAsync(novelId);
        if (!loaded) return RedirectToPage("/Publish/Index");
        Input.VolumeNumber = (AllVolumes.Count == 0 ? 1 : AllVolumes.Max(v => v.VolumeNumber) + 1);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.PostAsync<VolumeDto>($"/api/novels/{novelId}/volumes", Input, Token);
        SetApiResultMessage(result, "Tạo tập thành công.", "Không thể tạo tập.");
        return RedirectToPage(new { novelId });
    }

    public async Task<IActionResult> OnPostEditAsync(int novelId, int id, int volumeNumber, string title)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Cần nhập tiêu đề tập.";
            return RedirectToPage(new { novelId });
        }

        var input = new VolumeUpsertRequest
        {
            VolumeNumber = volumeNumber,
            Title = title.Trim()
        };
        var result = await Api.PutAsync<VolumeDto>($"/api/volumes/{id}", input, Token);
        SetApiResultMessage(result, "Đã cập nhật tên tập.", "Không thể cập nhật tên tập.");
        return RedirectToPage(new { novelId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/volumes/{id}", Token);
        SetApiResultMessage(result, "Đã xóa tập.", "Không thể xóa tập.");
        return RedirectToPage(new { novelId });
    }

    private async Task<bool> LoadAsync(int novelId)
    {
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
        var volumeTask = Api.GetAsync<List<VolumeDto>>($"/api/novels/{novelId}/volumes", Token);
        await Task.WhenAll(novelTask, volumeTask);
        if (!IsApiSuccess(novelTask.Result) || novelTask.Result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(novelTask.Result, "Không thể tải truyện.");
            return false;
        }

        Novel = novelTask.Result.Data;
        AllVolumes = (volumeTask.Result?.Data ?? Novel.Volumes.Cast<VolumeDto>().ToList())
            .OrderBy(v => v.VolumeNumber)
            .ToList();
        TotalElements = AllVolumes.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalElements / (double)PageSize));
        Page = Math.Min(Page, TotalPages);
        Volumes = AllVolumes
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        return true;
    }
}
