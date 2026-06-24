using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Statistics;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public StatisticsChartDto DailyGrowth { get; set; } = new();
    public StatisticsChartDto MonthlyGrowth { get; set; } = new();
    public StatisticsChartDto YearlyGrowth { get; set; } = new();
    public string DayRange { get; set; } = "30";
    public string? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
    public List<int> YearOptions { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(string? dayRange, string? month, int? year)
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
        DayRange = NormalizeDayRange(dayRange);
        SelectedMonth = string.IsNullOrWhiteSpace(month) ? null : month;
        SelectedYear = year;
        YearOptions = Enumerable.Range(today.Year - 2, 3).Reverse().ToList();

        var dailyFrom = today.AddDays(-int.Parse(DayRange) + 1);
        var dailyTask = LoadChartAsync("userGrowth", dailyFrom, today, "day", token);

        var (monthlyFrom, monthlyTo, monthlyGranularity) = ResolveMonthlyRange(SelectedMonth, today);
        var monthlyTask = LoadChartAsync("userGrowth", monthlyFrom, monthlyTo, monthlyGranularity, token);

        var (yearlyFrom, yearlyTo, yearlyGranularity) = ResolveYearlyRange(SelectedYear, today);
        var yearlyTask = LoadChartAsync("userGrowth", yearlyFrom, yearlyTo, yearlyGranularity, token);

        DailyGrowth = await dailyTask;
        MonthlyGrowth = await monthlyTask;
        YearlyGrowth = await yearlyTask;

        return Page();
    }

    private async Task<StatisticsChartDto> LoadChartAsync(string metric, DateTime from, DateTime to, string granularity, string? token)
    {
        var endpoint = $"/api/admin/statistics/chart?metric={metric}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&granularity={granularity}";
        var result = await api.GetAsync<StatisticsChartDto>(endpoint, token);
        if (result?.Success == true && result.Data != null)
        {
            return result.Data;
        }

        LoadError ??= result?.Message ?? "Khong the tai bieu do thong ke tu API.";
        return new StatisticsChartDto { Metric = metric };
    }

    private static string NormalizeDayRange(string? dayRange)
        => dayRange is "7" or "30" or "90" ? dayRange : "30";

    private static (DateTime From, DateTime To, string Granularity) ResolveMonthlyRange(string? selectedMonth, DateTime today)
    {
        if (!string.IsNullOrWhiteSpace(selectedMonth)
            && DateTime.TryParseExact(selectedMonth, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var monthStart))
        {
            return (monthStart, monthStart.AddMonths(1).AddDays(-1), "day");
        }

        var from = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
        return (from, today, "month");
    }

    private static (DateTime From, DateTime To, string Granularity) ResolveYearlyRange(int? selectedYear, DateTime today)
    {
        if (selectedYear.HasValue)
        {
            var start = new DateTime(selectedYear.Value, 1, 1);
            return (start, start.AddYears(1).AddDays(-1), "month");
        }

        var from = new DateTime(today.Year - 2, 1, 1);
        return (from, today, "year");
    }
}
