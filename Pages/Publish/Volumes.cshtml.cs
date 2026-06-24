using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class VolumesModel : PublishPageModel
{
    public NovelDetailDto Novel { get; set; } = new();
    public List<VolumeDto> Volumes { get; set; } = [];
    [BindProperty] public VolumeUpsertRequest Input { get; set; } = new();

    public VolumesModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(novelId);
        if (!loaded) return RedirectToPage("/Publish/Index");
        Input.VolumeNumber = (Volumes.Count == 0 ? 1 : Volumes.Max(v => v.VolumeNumber) + 1);
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
        Volumes = volumeTask.Result?.Data ?? Novel.Volumes.Cast<VolumeDto>().ToList();
        return true;
    }
}
