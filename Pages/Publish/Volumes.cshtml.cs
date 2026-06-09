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
        await LoadAsync(novelId);
        Input.VolumeNumber = (Volumes.Count == 0 ? 1 : Volumes.Max(v => v.VolumeNumber) + 1);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.PostAsync<VolumeDto>($"/api/novels/{novelId}/volumes", Input, Token);
        TempData[result?.Success == false ? "Error" : "Success"] = result?.Message ?? "Volume created.";
        return RedirectToPage(new { novelId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/volumes/{id}", Token);
        TempData[result?.Success == false ? "Error" : "Success"] = result?.Message ?? "Volume deleted.";
        return RedirectToPage(new { novelId });
    }

    private async Task LoadAsync(int novelId)
    {
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
        var volumeTask = Api.GetAsync<List<VolumeDto>>($"/api/novels/{novelId}/volumes", Token);
        await Task.WhenAll(novelTask, volumeTask);
        Novel = novelTask.Result?.Data ?? MockNovel(novelId);
        Volumes = volumeTask.Result?.Data ?? Novel.Volumes.Cast<VolumeDto>().ToList();
    }
}
