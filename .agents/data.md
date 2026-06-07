# 📊 LitNovel – Tài Liệu Data Model

> Database: **LitNovelDB** (SQL Server)  
> ORM: **Entity Framework Core 10**  
> Tổng số bảng: **19 tables**

---

## Mục Lục

1. [Enums (Kiểu liệt kê)](#1-enums)
2. [Bảng Users](#2-bảng-users)
3. [Bảng UserReputations](#3-bảng-userreputations)
4. [Bảng RefreshTokens](#4-bảng-refreshtokens)
5. [Bảng Badges](#5-bảng-badges)
6. [Bảng UserBadges](#6-bảng-userbadges)
7. [Bảng Categories](#7-bảng-categories)
8. [Bảng Tags](#8-bảng-tags)
9. [Bảng Novels](#9-bảng-novels)
10. [Bảng NovelTags](#10-bảng-noveltags)
11. [Bảng Volumes](#11-bảng-volumes)
12. [Bảng Chapters](#12-bảng-chapters)
13. [Bảng ChapterContents](#13-bảng-chaptercontents)
14. [Bảng CommentChapters](#14-bảng-commentchapters)
15. [Bảng Favorites](#15-bảng-favorites)
16. [Bảng NovelRatings](#16-bảng-novelratings)
17. [Bảng ReadingProgresses](#17-bảng-readingprogresses)
18. [Bảng Notifications](#18-bảng-notifications)
19. [Bảng NovelReports](#19-bảng-novelreports)
20. [Bảng UserReports](#20-bảng-userreports)
21. [Sơ Đồ Quan Hệ Tổng Quan](#21-sơ-đồ-quan-hệ-tổng-quan)
22. [Ghi Chú Kỹ Thuật](#22-ghi-chú-kỹ-thuật)

---

## 1. Enums

Tất cả enum được lưu dưới dạng **string** trong database (`HasConversion<string>()`).

### UserRole
| Giá trị | Số | Mô tả |
|---|---|---|
| `User` | 0 | Người dùng thông thường |
| `Staff` | 1 | Nhân viên quản lý nội dung |
| `Admin` | 2 | Quản trị viên hệ thống |

### UserStatus
| Giá trị | Số | Mô tả |
|---|---|---|
| `Offline` | 0 | Ngoại tuyến (mặc định) |
| `Online` | 1 | Đang trực tuyến |
| `Banned` | 2 | Bị cấm |

### NovelStatus
| Giá trị | Số | Mô tả |
|---|---|---|
| `Pending` | 0 | Chờ duyệt (mặc định) |
| `Ongoing` | 1 | Đang ra chương |
| `Ended` | 2 | Đã hoàn thành |
| `Hiatus` | 3 | Tạm ngừng |
| `Dropped` | 4 | Bỏ dở |
| `Canceled` | 5 | Đã hủy |

### ChapterStatus
| Giá trị | Số | Mô tả |
|---|---|---|
| `Draft` | 0 | Bản nháp (mặc định) |
| `Published` | 1 | Đã đăng |
| `Scheduled` | 2 | Lên lịch đăng |

### ReportType
| Giá trị | Số | Mô tả |
|---|---|---|
| `Spam` | 0 | Spam |
| `Inappropriate` | 1 | Nội dung không phù hợp |
| `Copyright` | 2 | Vi phạm bản quyền |
| `Harassment` | 3 | Quấy rối |
| `Other` | 4 | Khác |

### ReportStatus
| Giá trị | Số | Mô tả |
|---|---|---|
| `Pending` | 0 | Chờ xử lý (mặc định) |
| `Resolved` | 1 | Đã giải quyết |
| `Rejected` | 2 | Bác bỏ |

### NotificationType
| Giá trị | Số | Mô tả |
|---|---|---|
| `NewChapter` | 0 | Chương mới được đăng |
| `NewComment` | 1 | Bình luận mới trên chương |
| `CommentReply` | 2 | Trả lời bình luận |
| `CommentLike` | 3 | Ai đó thích bình luận |
| `NewFollower` | 4 | Người theo dõi mới |
| `BadgeEarned` | 5 | Nhận huy hiệu mới |
| `ReportUpdate` | 6 | Cập nhật về báo cáo vi phạm |
| `SystemAlert` | 7 | Thông báo hệ thống |

---

## 2. Bảng Users

**Table name:** `Users`  
**Entity:** `User : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `Username` | `string` | `nvarchar(50)` | NOT NULL | UNIQUE | Tên đăng nhập |
| `Email` | `string` | `nvarchar(256)` | NOT NULL | UNIQUE | Email |
| `PasswordHash` | `string` | `nvarchar(max)` | NOT NULL | — | Mật khẩu đã hash |
| `Avatar` | `string?` | `nvarchar(512)` | NULL | — | URL ảnh đại diện |
| `Bio` | `string?` | `nvarchar(1000)` | NULL | — | Tiểu sử |
| `Status` | `UserStatus` | `nvarchar` (string) | NOT NULL | — | Trạng thái tài khoản |
| `Role` | `UserRole` | `nvarchar` (string) | NOT NULL | — | Vai trò |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_Users_Username` – UNIQUE
- `IX_Users_Email` – UNIQUE

**Relationships:**
- `→ UserReputation` (1:1, Cascade delete)
- `→ RefreshTokens` (1:N)
- `→ UserBadges` (1:N)
- `→ Novels` (1:N, as Author)
- `→ CommentChapters` (1:N)
- `→ Favorites` (1:N)
- `→ NovelRatings` (1:N)
- `→ ReadingProgresses` (1:N)
- `→ Notifications` (1:N)
- `← NovelReports` (as Reporter / ProcessedBy)
- `← UserReports` (as Reporter / ProcessedBy / TargetUser)

---

## 3. Bảng UserReputations

**Table name:** `UserReputations`  
**Entity:** `UserReputation`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `UserId` | `int` | `int` | NOT NULL | FK → Users.Id | Tham chiếu đến User |
| `Score` | `int` | `int` | NOT NULL | Default = 0 | Điểm uy tín |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | — | Thời điểm cập nhật |

**Indexes:**
- `IX_UserReputations_UserId` – UNIQUE

**Foreign Keys:**
- `UserId` → `Users.Id` (Cascade delete)

---

## 4. Bảng RefreshTokens

**Table name:** `RefreshTokens`  
**Entity:** `RefreshToken : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `UserId` | `int` | `int` | NOT NULL | FK → Users.Id | Chủ sở hữu token |
| `Token` | `string` | `nvarchar(512)` | NOT NULL | UNIQUE | Chuỗi refresh token |
| `ExpiresAt` | `DateTime` | `datetime2` | NOT NULL | — | Thời điểm hết hạn |
| `IsRevoked` | `bool` | `bit` | NOT NULL | — | Đã thu hồi chưa |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_RefreshTokens_Token` – UNIQUE

**Foreign Keys:**
- `UserId` → `Users.Id` (Cascade delete)

---

## 5. Bảng Badges

**Table name:** `Badges`  
**Entity:** `Badge`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `Key` | `string` | `nvarchar(100)` | NOT NULL | UNIQUE | Mã định danh huy hiệu |
| `Name` | `string` | `nvarchar(100)` | NOT NULL | — | Tên hiển thị |
| `Description` | `string` | `nvarchar(500)` | NOT NULL | — | Mô tả |
| `Icon` | `string?` | `nvarchar(512)` | NULL | — | URL icon |
| `Color` | `string?` | `nvarchar(20)` | NULL | — | Mã màu (hex, tên...) |

**Indexes:**
- `IX_Badges_Key` – UNIQUE

---

## 6. Bảng UserBadges

**Table name:** `UserBadges`  
**Entity:** `UserBadge`  
**Loại:** Junction table (N:M giữa Users và Badges)

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `UserId` | `int` | `int` | NOT NULL | PK (composite), FK → Users.Id | |
| `BadgeId` | `int` | `int` | NOT NULL | PK (composite), FK → Badges.Id | |
| `EarnedAt` | `DateTime` | `datetime2` | NOT NULL | — | Thời điểm nhận huy hiệu |

**Primary Key:** Composite `(UserId, BadgeId)`

**Foreign Keys:**
- `UserId` → `Users.Id` (Cascade delete)
- `BadgeId` → `Badges.Id` (Cascade delete)

---

## 7. Bảng Categories

**Table name:** `Categories`  
**Entity:** `Category`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `Name` | `string` | `nvarchar(100)` | NOT NULL | — | Tên thể loại |
| `Slug` | `string` | `nvarchar(120)` | NOT NULL | UNIQUE | Slug URL-friendly |

**Indexes:**
- `IX_Categories_Slug` – UNIQUE

---

## 8. Bảng Tags

**Table name:** `Tags`  
**Entity:** `Tag`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `Name` | `string` | `nvarchar(100)` | NOT NULL | — | Tên tag |
| `Slug` | `string` | `nvarchar(120)` | NOT NULL | UNIQUE | Slug URL-friendly |

**Indexes:**
- `IX_Tags_Slug` – UNIQUE

---

## 9. Bảng Novels

**Table name:** `Novels`  
**Entity:** `Novel : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `Title` | `string` | `nvarchar(200)` | NOT NULL | — | Tiêu đề truyện |
| `Slug` | `string` | `nvarchar(220)` | NOT NULL | UNIQUE | Slug URL-friendly |
| `Description` | `string?` | `nvarchar(5000)` | NULL | — | Mô tả / tóm tắt |
| `CoverImage` | `string?` | `nvarchar(512)` | NULL | — | URL ảnh bìa |
| `AuthorId` | `int` | `int` | NOT NULL | FK → Users.Id | Tác giả |
| `CategoryId` | `int?` | `int` | NULL | FK → Categories.Id | Thể loại |
| `Status` | `NovelStatus` | `nvarchar` (string) | NOT NULL | Default = `'Pending'` | Trạng thái truyện |
| `ViewCount` | `int` | `int` | NOT NULL | Default = 0 | Lượt xem |
| `LikeCount` | `int` | `int` | NOT NULL | Default = 0 | Lượt thích |
| `DislikeCount` | `int` | `int` | NOT NULL | Default = 0 | Lượt không thích |
| `TotalChapters` | `int` | `int` | NOT NULL | Default = 0 | Tổng số chương |
| `TotalVolumes` | `int` | `int` | NOT NULL | Default = 0 | Tổng số tập |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm đăng |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_Novels_Slug` – UNIQUE
- `IX_Novels_AuthorId`
- `IX_Novels_CategoryId`

**Foreign Keys:**
- `AuthorId` → `Users.Id` (Restrict delete)
- `CategoryId` → `Categories.Id` (Set NULL on delete)

---

## 10. Bảng NovelTags

**Table name:** `NovelTags`  
**Entity:** `NovelTag`  
**Loại:** Junction table (N:M giữa Novels và Tags)

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `NovelId` | `int` | `int` | NOT NULL | PK (composite), FK → Novels.Id | |
| `TagId` | `int` | `int` | NOT NULL | PK (composite), FK → Tags.Id | |

**Primary Key:** Composite `(NovelId, TagId)`

**Foreign Keys:**
- `NovelId` → `Novels.Id` (Cascade delete)
- `TagId` → `Tags.Id` (Cascade delete)

---

## 11. Bảng Volumes

**Table name:** `Volumes`  
**Entity:** `Volume`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `NovelId` | `int` | `int` | NOT NULL | FK → Novels.Id | Truyện chứa tập này |
| `VolumeNumber` | `int` | `int` | NOT NULL | — | Số thứ tự tập |
| `Title` | `string` | `nvarchar(200)` | NOT NULL | — | Tên tập |

**Indexes:**
- `IX_Volumes_NovelId_VolumeNumber` – UNIQUE (composite)

**Foreign Keys:**
- `NovelId` → `Novels.Id` (Cascade delete)

---

## 12. Bảng Chapters

**Table name:** `Chapters`  
**Entity:** `Chapter : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `VolumeId` | `int` | `int` | NOT NULL | FK → Volumes.Id | Tập chứa chương này |
| `ChapterNumber` | `int` | `int` | NOT NULL | — | Số thứ tự chương |
| `Title` | `string` | `nvarchar(200)` | NOT NULL | — | Tiêu đề chương |
| `Slug` | `string` | `nvarchar(220)` | NOT NULL | UNIQUE | Slug URL-friendly |
| `ReleaseDate` | `DateTime?` | `datetime2` | NULL | — | Ngày lên lịch đăng |
| `Status` | `ChapterStatus` | `nvarchar` (string) | NOT NULL | Default = `'Draft'` | Trạng thái chương |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_Chapters_VolumeId_ChapterNumber` – UNIQUE (composite)
- `IX_Chapters_Slug` – UNIQUE

**Foreign Keys:**
- `VolumeId` → `Volumes.Id` (Cascade delete)

---

## 13. Bảng ChapterContents

**Table name:** `ChapterContents`  
**Entity:** `ChapterContent`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `ChapterId` | `int` | `int` | NOT NULL | UNIQUE FK → Chapters.Id | Chương tương ứng (1:1) |
| `Content` | `string` | `nvarchar(max)` | NOT NULL | — | Nội dung văn bản đầy đủ |
| `Version` | `int` | `int` | NOT NULL | Default = 1 | Phiên bản nội dung |

**Indexes:**
- `IX_ChapterContents_ChapterId` – UNIQUE (đảm bảo quan hệ 1:1)

**Foreign Keys:**
- `ChapterId` → `Chapters.Id` (Cascade delete)

> **Ghi chú:** Nội dung chương được tách thành bảng riêng (`ChapterContents`) để tối ưu hiệu năng, tránh load nội dung lớn khi chỉ cần thông tin metadata của chương.

---

## 14. Bảng CommentChapters

**Table name:** `CommentChapters`  
**Entity:** `CommentChapter : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `Content` | `string` | `nvarchar(2000)` | NOT NULL | — | Nội dung bình luận |
| `UserId` | `int` | `int` | NOT NULL | FK → Users.Id | Người bình luận |
| `ChapterId` | `int` | `int` | NOT NULL | FK → Chapters.Id | Chương được bình luận |
| `LikeCount` | `int` | `int` | NOT NULL | Default = 0 | Số lượt thích |
| `DislikeCount` | `int` | `int` | NOT NULL | Default = 0 | Số lượt không thích |
| `ParentCommentId` | `int?` | `int` | NULL | FK → CommentChapters.Id | Bình luận cha (nested) |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm bình luận |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm chỉnh sửa |

**Indexes:**
- `IX_CommentChapters_ChapterId`
- `IX_CommentChapters_UserId`

**Foreign Keys:**
- `UserId` → `Users.Id` (Restrict delete)
- `ChapterId` → `Chapters.Id` (Cascade delete)
- `ParentCommentId` → `CommentChapters.Id` (Restrict delete, nullable) — **self-reference**

> **Ghi chú:** Hỗ trợ bình luận lồng nhau (nested comments) thông qua `ParentCommentId`. Một bình luận gốc có `ParentCommentId = NULL`.

---

## 15. Bảng Favorites

**Table name:** `Favorites`  
**Entity:** `Favorite`  
**Loại:** Junction table (N:M giữa Users và Novels)

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `UserId` | `int` | `int` | NOT NULL | PK (composite), FK → Users.Id | |
| `NovelId` | `int` | `int` | NOT NULL | PK (composite), FK → Novels.Id | |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | — | Thời điểm thêm vào yêu thích |

**Primary Key:** Composite `(UserId, NovelId)`

**Foreign Keys:**
- `UserId` → `Users.Id` (Cascade delete)
- `NovelId` → `Novels.Id` (Cascade delete)

---

## 16. Bảng NovelRatings

**Table name:** `NovelRatings`  
**Entity:** `NovelRating`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `NovelId` | `int` | `int` | NOT NULL | FK → Novels.Id | Truyện được đánh giá |
| `UserId` | `int` | `int` | NOT NULL | FK → Users.Id | Người đánh giá |
| `Rating` | `byte` | `tinyint` | NOT NULL | Range: 1–5 | Điểm đánh giá (1–5 sao) |
| `Review` | `string?` | `nvarchar(3000)` | NULL | — | Nội dung nhận xét |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Default = UtcNow | Thời điểm đánh giá |
| `UpdatedAt` | `DateTime?` | `datetime2` | NULL | — | Thời điểm chỉnh sửa |

**Indexes:**
- `IX_NovelRatings_UserId_NovelId` – UNIQUE (mỗi user chỉ đánh giá 1 lần)

**Foreign Keys:**
- `NovelId` → `Novels.Id` (Cascade delete)
- `UserId` → `Users.Id` (Restrict delete)

---

## 17. Bảng ReadingProgresses

**Table name:** `ReadingProgresses`  
**Entity:** `ReadingProgress`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `UserId` | `int` | `int` | NOT NULL | PK (composite), FK → Users.Id | |
| `NovelId` | `int` | `int` | NOT NULL | PK (composite), FK → Novels.Id | |
| `ChapterId` | `int` | `int` | NOT NULL | FK → Chapters.Id | Chương đang đọc |
| `ProgressPercentage` | `byte` | `tinyint` | NOT NULL | Range: 0–100 | % đọc của chương hiện tại |
| `LastReadAt` | `DateTime` | `datetime2` | NOT NULL | Default = UtcNow | Thời điểm đọc gần nhất |

**Primary Key:** Composite `(UserId, NovelId)`

**Indexes:**
- `IX_ReadingProgresses_UserId`

**Foreign Keys:**
- `UserId` → `Users.Id` (Cascade delete)
- `NovelId` → `Novels.Id` (Cascade delete)
- `ChapterId` → `Chapters.Id` (Restrict delete)

> **Ghi chú:** Mỗi user chỉ có 1 bản ghi tiến trình đọc cho mỗi truyện (composite PK). `ChapterId` lưu chương đọc gần nhất.

---

## 18. Bảng Notifications

**Table name:** `Notifications`  
**Entity:** `Notification : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `UserId` | `int` | `int` | NOT NULL | FK → Users.Id | Người nhận thông báo |
| `NotificationType` | `NotificationType` | `nvarchar` (string) | NOT NULL | — | Loại thông báo |
| `EntityType` | `string?` | `nvarchar(100)` | NULL | — | Loại đối tượng liên quan (e.g. `"Chapter"`, `"Comment"`) |
| `EntityId` | `int?` | `int` | NULL | — | ID đối tượng liên quan |
| `Message` | `string` | `nvarchar(1000)` | NOT NULL | — | Nội dung thông báo |
| `IsRead` | `bool` | `bit` | NOT NULL | Default = false | Đã đọc chưa |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_Notifications_UserId_IsRead` (composite)

**Foreign Keys:**
- `UserId` → `Users.Id` (Cascade delete)

---

## 19. Bảng NovelReports

**Table name:** `NovelReports`  
**Entity:** `NovelReport : BaseReport : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `ReportType` | `ReportType` | `nvarchar` (string) | NOT NULL | — | Loại vi phạm |
| `Description` | `string?` | `nvarchar(2000)` | NULL | — | Mô tả chi tiết vi phạm |
| `Status` | `ReportStatus` | `nvarchar` (string) | NOT NULL | Default = `'Pending'` | Trạng thái xử lý |
| `ActionTaken` | `string?` | `nvarchar(1000)` | NULL | — | Hành động đã thực hiện |
| `ResolutionNotes` | `string?` | `nvarchar(1000)` | NULL | — | Ghi chú giải quyết |
| `ReporterId` | `int` | `int` | NOT NULL | FK → Users.Id | Người báo cáo |
| `ProcessedById` | `int?` | `int` | NULL | FK → Users.Id | Admin xử lý |
| `TargetNovelId` | `int` | `int` | NOT NULL | FK → Novels.Id | Truyện bị báo cáo |
| `TargetChapterId` | `int?` | `int` | NULL | FK → Chapters.Id | Chương bị báo cáo (nếu có) |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_NovelReports_ReporterId_TargetNovelId` (composite)

**Foreign Keys:**
- `ReporterId` → `Users.Id` (Restrict delete)
- `ProcessedById` → `Users.Id` (Set NULL on delete, nullable)
- `TargetNovelId` → `Novels.Id` (Restrict delete)
- `TargetChapterId` → `Chapters.Id` (Set NULL on delete, nullable)

---

## 20. Bảng UserReports

**Table name:** `UserReports`  
**Entity:** `UserReport : BaseReport : BaseEntity`

| Cột | Kiểu C# | Kiểu DB | Nullable | Constraint | Mô tả |
|---|---|---|---|---|---|
| `Id` | `int` | `int` | NOT NULL | PK, Auto-increment | Khóa chính |
| `ReportType` | `ReportType` | `nvarchar` (string) | NOT NULL | — | Loại vi phạm |
| `Description` | `string?` | `nvarchar(2000)` | NULL | — | Mô tả chi tiết |
| `Status` | `ReportStatus` | `nvarchar` (string) | NOT NULL | Default = `'Pending'` | Trạng thái xử lý |
| `ActionTaken` | `string?` | `nvarchar(1000)` | NULL | — | Hành động đã thực hiện |
| `ResolutionNotes` | `string?` | `nvarchar(1000)` | NULL | — | Ghi chú giải quyết |
| `ReporterId` | `int` | `int` | NOT NULL | FK → Users.Id | Người báo cáo |
| `ProcessedById` | `int?` | `int` | NULL | FK → Users.Id | Admin xử lý |
| `TargetUserId` | `int` | `int` | NOT NULL | FK → Users.Id | User bị báo cáo |
| `TargetCommentId` | `int?` | `int` | NULL | FK → CommentChapters.Id | Comment bị báo cáo (nếu có) |
| `CreatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | `datetime2` | NOT NULL | Auto-set | Thời điểm cập nhật |

**Indexes:**
- `IX_UserReports_ReporterId_TargetUserId` (composite)

**Foreign Keys:**
- `ReporterId` → `Users.Id` (Restrict delete)
- `ProcessedById` → `Users.Id` (Set NULL on delete, nullable)
- `TargetUserId` → `Users.Id` (Restrict delete)
- `TargetCommentId` → `CommentChapters.Id` (Set NULL on delete, nullable)

---

## 21. Sơ Đồ Quan Hệ Tổng Quan

```
                        ┌──────────────┐
                        │  Categories  │
                        └──────┬───────┘
                               │ (0..1):N
                        ┌──────▼───────┐     ┌──────────────┐
              ┌─────────│    Novels    │────►│   NovelTags  │◄──────┐
              │         └──────┬───────┘     └──────────────┘       │
              │   (Restrict)   │ 1:N                                 │
              │         ┌──────▼───────┐                      ┌──────┴───────┐
              │         │   Volumes    │                       │    Tags      │
              │         └──────┬───────┘                       └──────────────┘
              │   (Cascade)    │ 1:N
              │         ┌──────▼───────┐     ┌──────────────────┐
              │         │   Chapters   │────►│  ChapterContents │ (1:1)
              │         └──────┬───────┘     └──────────────────┘
              │                │ 1:N
              │         ┌──────▼───────────┐
              │         │  CommentChapters │──┐ self-reference (replies)
              │         └──────────────────┘──┘
              │
┌─────────────┴────────────────────────────────────────────────┐
│                          Users                               │
│  (UserRole: User / Staff / Admin)                            │
│  (UserStatus: Offline / Online / Banned)                     │
└─────┬────────────────────────────────────────────────────────┘
      │
      ├──── 1:1 ──── UserReputations
      ├──── 1:N ──── RefreshTokens
      ├──── 1:N ──── UserBadges ──── N:M ──── Badges
      ├──── 1:N ──── Favorites ──── → Novels
      ├──── 1:N ──── NovelRatings ──── → Novels
      ├──── 1:N ──── ReadingProgresses ──── → Novels + Chapters
      ├──── 1:N ──── Notifications
      ├──── 1:N ──── NovelReports (as Reporter) ──── → Novels
      └──── 1:N ──── UserReports (as Reporter) ──── → Users + CommentChapters
```

---

## 22. Ghi Chú Kỹ Thuật

### Auto-timestamp

Tất cả entity kế thừa `BaseEntity` (gồm `Id`, `CreatedAt`, `UpdatedAt`) được tự động gán timestamp trong `LitNovelContext.SaveChanges()`:

```csharp
// Khi thêm mới (EntityState.Added):
entity.CreatedAt = DateTime.UtcNow;
entity.UpdatedAt = DateTime.UtcNow;

// Khi cập nhật (EntityState.Modified):
entity.UpdatedAt = DateTime.UtcNow;
```

### Enum Storage

Tất cả enum được lưu dưới dạng **string** trong database thay vì số nguyên, giúp dữ liệu dễ đọc hơn khi query trực tiếp:

```csharp
builder.Property(u => u.Status).HasConversion<string>().IsRequired();
// → Lưu "Offline", "Online", "Banned" thay vì 0, 1, 2
```

### Delete Behaviors

| Hành vi | Ý nghĩa | Ví dụ áp dụng |
|---|---|---|
| `Cascade` | Xóa cha → tự động xóa con | User → RefreshTokens, Notifications |
| `Restrict` | Không cho xóa cha nếu còn con | User → Novels (không xóa user có truyện) |
| `SetNull` | Xóa cha → set FK thành NULL | Novel → ProcessedBy (admin bị xóa) |

### Bảng Không Có BaseEntity

Các bảng sau **không** kế thừa `BaseEntity` (không có `CreatedAt`/`UpdatedAt` tự động):

| Bảng | Lý do |
|---|---|
| `Category` | Lookup table đơn giản |
| `Tag` | Lookup table đơn giản |
| `Badge` | Lookup table đơn giản |
| `Volume` | Entity phụ trợ |
| `ChapterContent` | Chỉ chứa nội dung |
| `UserBadge` | Junction table |
| `NovelTag` | Junction table |
| `Favorite` | Junction table (có `CreatedAt` riêng) |
| `NovelRating` | Có `CreatedAt`/`UpdatedAt` riêng |
| `ReadingProgress` | Có `LastReadAt` riêng |
| `UserReputation` | Có `UpdatedAt` riêng |

### Tách Nội Dung Chương (CQRS-friendly)

```
Chapters (metadata)          ChapterContents (content)
├── Id                        ├── Id
├── VolumeId                  ├── ChapterId (UNIQUE FK)
├── ChapterNumber             ├── Content (nvarchar(max))
├── Title                     └── Version
├── Slug
├── Status
└── ReleaseDate
```

Cách thiết kế này cho phép:
- List/search chương mà không cần load nội dung lớn
- Versioning nội dung chương qua cột `Version`

---

*Tài liệu được tạo từ phân tích source code – LitNovel Backend – PRN232*
