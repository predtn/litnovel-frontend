using litnovel_frontend.Services;

namespace litnovel_frontend.Endpoints;

public static class ApiProxyEndpoints
{
    public static IEndpointRouteBuilder MapApiProxyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapNovelProxyEndpoints();
        endpoints.MapCommentProxyEndpoints();
        endpoints.MapNotificationProxyEndpoints();
        endpoints.MapAnnouncementProxyEndpoints();
        endpoints.MapReportProxyEndpoints();
        return endpoints;
    }

    private static IEndpointRouteBuilder MapNovelProxyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/novels/{id:int}/favorites", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.PostAsync<object>($"/api/novels/{id}/favorites", null, token);
            return ToProxyResult(result);
        });

        endpoints.MapDelete("/api/novels/{id:int}/favorites", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.DeleteAsync<object>($"/api/novels/{id}/favorites", token);
            return ToProxyResult(result);
        });

        endpoints.MapPost("/api/novels/{id:int}/likes", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.PostAsync<object>($"/api/novels/{id}/likes", null, token);
            return ToProxyResult(result);
        });

        endpoints.MapDelete("/api/novels/{id:int}/likes", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.DeleteAsync<object>($"/api/novels/{id}/likes", token);
            return ToProxyResult(result);
        });

        endpoints.MapPost("/api/novels/{id:int}/views", async (
            int id,
            IApiService api) =>
        {
            var result = await api.PostAsync<object>($"/api/novels/{id}/views", null);
            return ToProxyResult(result);
        });

        endpoints.MapGet("/api/users/me/favorites", async (
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.GetAsync<System.Text.Json.JsonElement>("/api/users/me/favorites?page=1&size=100", token);
            return Results.Json(result);
        });

        return endpoints;
    }

    private static IEndpointRouteBuilder MapCommentProxyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/comments/{id:int}/likes", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.PostAsync<object>($"/api/comments/{id}/likes", null, token);
            return ToProxyResult(result);
        });

        endpoints.MapDelete("/api/comments/{id:int}/likes", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.DeleteAsync<object>($"/api/comments/{id}/likes", token);
            return ToProxyResult(result);
        });

        endpoints.MapDelete("/api/comments/{id:int}", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.DeleteAsync<object>($"/api/comments/{id}", token);
            return ToProxyResult(result);
        });

        return endpoints;
    }

    private static IEndpointRouteBuilder MapNotificationProxyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/notifications", async (
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
            var result = await api.GetAsync<NotificationListDto>($"/api/notifications{query}", token);
            return ToProxyResult(result);
        });

        endpoints.MapPut("/api/notifications/{id:int}/read", async (
            int id,
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.PutAsync<object>($"/api/notifications/{id}/read", null, token);
            ClearNotificationCountCache(context, result?.Success == true);
            return ToProxyResult(result);
        });

        endpoints.MapPut("/api/notifications/read-all", async (
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var result = await api.PutAsync<object>("/api/notifications/read-all", null, token);
            ClearNotificationCountCache(context, result?.Success == true);
            return ToProxyResult(result);
        });

        return endpoints;
    }

    private static IEndpointRouteBuilder MapAnnouncementProxyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/announcements", async (
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            if (context.User.IsInRole("Admin"))
            {
                return Results.Json(new ApiResponse<List<AnnouncementDto>>
                {
                    Success = true,
                    Data = []
                });
            }

            var token = auth.GetToken(context);
            var result = await api.GetAsync<List<AnnouncementDto>>("/api/announcements", token);
            return ToProxyResult(result);
        });

        return endpoints;
    }

    private static IEndpointRouteBuilder MapReportProxyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/reports/users", async (
            HttpContext context,
            IApiService api,
            IAuthService auth) =>
        {
            var token = auth.GetToken(context);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            var request = await context.Request.ReadFromJsonAsync<object>();
            var result = await api.PostAsync<object>("/api/reports/users", request, token);
            return ToProxyResult(result);
        });

        return endpoints;
    }

    private static IResult ToProxyResult<T>(ApiResponse<T>? result)
    {
        return Results.Json(
            result,
            statusCode: result?.Success == true ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest);
    }

    private static void ClearNotificationCountCache(HttpContext context, bool shouldClear)
    {
        if (!shouldClear) return;

        context.Session.Remove("_notif_count");
        context.Session.Remove("_notif_count_at");
    }
}
