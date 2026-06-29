namespace litnovel_frontend.Services;

public static class DisplayText
{
    public static string Status(string? value) => value?.Trim() switch
    {
        "Draft" => "Bản nháp",
        "Pending" => "Chờ duyệt",
        "Ongoing" => "Đang tiến hành",
        "Ended" => "Đã kết thúc",
        "Hiatus" => "Tạm ngưng",
        "Dropped" => "Đã bỏ",
        "Canceled" => "Đã hủy",
        "Published" => "Đã xuất bản",
        "Rejected" => "Bị từ chối",
        "Resolved" => "Đã xử lý",
        "Online" => "Đang online",
        "Offline" => "Offline",
        "Banned" => "Bị cấm",
        "Completed" => "Hoàn tất",
        "InProgress" => "Đang chạy",
        "Scheduled" => "Đã lên lịch",
        "Locked" => "Bị khóa",
        _ => string.IsNullOrWhiteSpace(value) ? "-" : value
    };

    public static string Role(string? value) => value?.Trim() switch
    {
        "User" => "Người dùng",
        "Staff" => "Nhân viên",
        "Admin" => "Quản trị viên",
        _ => string.IsNullOrWhiteSpace(value) ? "-" : value
    };

    public static string ModerationType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "novel" => "Truyện",
        "chapter" => "Chương",
        _ => string.IsNullOrWhiteSpace(value) ? "-" : value
    };

    public static string ReportType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "novel" => "Báo cáo truyện",
        "user" => "Báo cáo người dùng",
        "comment" => "Báo cáo bình luận",
        "chapter" => "Báo cáo chương",
        _ => string.IsNullOrWhiteSpace(value) ? "-" : value
    };

    public static string NotificationType(string? value) => value?.Trim() switch
    {
        "SystemAlert" => "Cảnh báo hệ thống",
        "ReportUpdate" => "Cập nhật báo cáo",
        "BadgeEarned" => "Nhận huy hiệu",
        "NewChapter" => "Chương mới",
        _ => string.IsNullOrWhiteSpace(value) ? "-" : value
    };
}
