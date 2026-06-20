using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Statistics;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public StatisticsChartDto UserGrowth { get; set; } = new();
    public StatisticsChartDto NovelPublishRate { get; set; } = new();
    public string FromDate { get; set; } = "";
    public string ToDate { get; set; } = "";
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(string? from, string? to)
    {
        var token = auth.GetToken(HttpContext);
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");

        ViewData["AdminSection"] = "statistics";
        var user = auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
        }

        var today = DateTime.UtcNow.Date;
        FromDate = string.IsNullOrWhiteSpace(from) ? today.AddDays(-30).ToString("yyyy-MM-dd") : from;
        ToDate = string.IsNullOrWhiteSpace(to) ? today.ToString("yyyy-MM-dd") : to;

        var chartResult = await api.GetAsync<StatisticsChartDto>($"/api/admin/statistics/chart?metric=userGrowth&from={FromDate}&to={ToDate}&granularity=day", token);
        if (chartResult?.Success == true && chartResult.Data != null)
        {
            UserGrowth = chartResult.Data;
        }
        else
        {
            LoadError = chartResult?.Message ?? "Khong the tai bieu do thong ke tu API.";
        }

        var novelChartResult = await api.GetAsync<StatisticsChartDto>($"/api/admin/statistics/chart?metric=novelPublishRate&from={FromDate}&to={ToDate}&granularity=day", token);
        if (novelChartResult?.Success == true && novelChartResult.Data != null)
        {
            NovelPublishRate = novelChartResult.Data;
        }

        return Page();
    }
}
