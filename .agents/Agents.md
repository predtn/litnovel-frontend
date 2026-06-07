# LitNovel — Agent Instructions

> Tài liệu này hướng dẫn AI coding agent làm việc trong codebase LitNovel.  
> Đọc toàn bộ file này trước khi thực hiện bất kỳ thay đổi nào.

---

## 1. Tổng Quan Dự Án

**LitNovel** là nền tảng đọc và xuất bản truyện chữ trực tuyến.

- **Backend:** ASP.NET Core 10 Web API — Clean Architecture 4 layers
- **Frontend:** ASP.NET Core 10 Razor Pages
- **Database:** SQL Server (`LitNovelDB`) — Entity Framework Core 10
- **Auth:** JWT Bearer + Refresh Token

Tài liệu tham chiếu chính:
- [`spec.md`](./spec.md) — Đặc tả yêu cầu đầy đủ
- [`architecture.md`](./architecture.md) — Kiến trúc hệ thống
- [`data.md`](./data.md) — Data model chi tiết
- [`screen.md`](./screen.md) — Màn hình UI
- [`.agents/ARCHITECTURE.md`](./litnovel-backend/.agents/ARCHITECTURE.md) — Backend architecture rules
- [`.agents/CODING_RULES.md`](./litnovel-backend/.agents/CODING_RULES.md) — Coding conventions bắt buộc

---

## 2. Actors & Vai Trò

| Actor | Mô tả | Quyền kế thừa |
|---|---|---|
| **Guest** | Khách chưa đăng nhập | — |
| **User** | Thành viên đã xác thực | — |
| **Staff** | Kiểm duyệt viên nội dung | Kế thừa User |
| **Admin** | Quản trị viên hệ thống | Kế thừa Staff |

### Quyền chi tiết

**Guest:**
- Đăng ký tài khoản, đăng nhập
- Duyệt và xem novel public

**User:**
- Đọc novel, chapter
- Yêu thích novel, like, rate, comment, reply
- Báo cáo vi phạm
- Tạo và quản lý novel của mình (volumes, chapters)

**Staff (+ User permissions):**
- Duyệt / từ chối novel và chapter được submit
- Xử lý reports, cảnh báo user
- Kiểm duyệt comment

**Admin (+ Staff permissions):**
- Quản lý users, phân quyền Staff
- Quản lý categories, tags
- Quản lý notifications, thống kê
- Override bất kỳ nội dung nào

---

## 3. Functional Requirements (FR)

### FR-01 — Authentication
- Đăng ký tài khoản mới (`POST /api/auth/register`)
- Đăng nhập, trả JWT + RefreshToken (`POST /api/auth/login`)
- Đăng xuất, thu hồi RefreshToken (`POST /api/auth/logout`)
- Làm mới token (`POST /api/auth/refresh`)
- Xem / cập nhật profile (`GET/PUT /api/users/me`)

### FR-02 — Novel Discovery
- Danh sách novel public với phân trang (`GET /api/novels`)
- Tìm kiếm theo keyword
- Lọc theo category, tag
- Sắp xếp theo: lượt xem, rating, ngày cập nhật

### FR-03 — Reading
- Chi tiết novel (`GET /api/novels/{slug}`)
- Nội dung chapter (`GET /api/chapters/{slug}`)
- Lưu tiến trình đọc (`PUT /api/reading-progress`)
- Danh sách novel yêu thích

### FR-04 — User Interaction
- Đánh giá novel (1–5 sao + review) — mỗi user 1 lần
- Bình luận chapter, reply (nested comments)
- Like / dislike novel, comment
- Báo cáo vi phạm (novel, chapter, user, comment)

### FR-05 — Novel Publishing
- CRUD novel (Author = chủ sở hữu)
- CRUD volumes trong novel
- CRUD chapters trong volume
- Submit novel/chapter để kiểm duyệt → status `Pending`
- Lên lịch đăng chapter (`ReleaseDate`)

### FR-06 — Moderation Workflow
- Staff xem danh sách nội dung chờ duyệt (`Pending`)
- Approve → chuyển sang `Published` / `Ongoing`
- Reject → ghi lý do, thông báo author
- Lock nội dung
- Xử lý Reports (Resolve / Reject), ghi `ActionTaken`

### FR-08 — Notifications
- Tự động tạo thông báo khi: chapter mới, có comment/reply, badged earned, report update, system alert
- Đánh dấu đã đọc (`PUT /api/notifications/{id}/read`)
- Xem danh sách thông báo chưa đọc

### FR-09 — Administration
- CRUD users, đổi role
- CRUD categories, tags
- Xem thống kê tổng quan
- Xem và xử lý reports

---

## 4. Data Model Tóm Tắt

> Chi tiết đầy đủ xem [`data.md`](./data.md)

### Entities chính (19 tables)

```
Users
  ├── UserReputation (1:1)
  ├── RefreshTokens (1:N)
  ├── UserBadges ←→ Badges (N:M)
  ├── Novels (1:N, as Author)
  ├── CommentChapters (1:N)
  ├── Favorites ←→ Novels (N:M)
  ├── NovelRatings (1:N)
  ├── ReadingProgresses (1:N)
  ├── Notifications (1:N)
  ├── NovelReports (as Reporter)
  └── UserReports (as Reporter / Target)

Novels
  ├── Category (N:1, nullable)
  ├── NovelTags ←→ Tags (N:M)
  ├── Volumes (1:N)
  │     └── Chapters (1:N)
  │           └── ChapterContent (1:1)
  │           └── CommentChapters (1:N, nested self-ref)
  ├── Favorites (1:N)
  ├── NovelRatings (1:N)
  ├── ReadingProgresses (1:N)
  └── NovelReports (1:N)
```

### Enums (lưu dạng string trong DB)

| Enum | Giá trị |
|---|---|
| `UserRole` | `User`, `Staff`, `Admin` |
| `UserStatus` | `Offline`, `Online`, `Banned` |
| `NovelStatus` | `Pending`, `Ongoing`, `Ended`, `Hiatus`, `Dropped`, `Canceled` |
| `ChapterStatus` | `Draft`, `Published`, `Scheduled` |
| `ReportType` | `Spam`, `Inappropriate`, `Copyright`, `Harassment`, `Other` |
| `ReportStatus` | `Pending`, `Resolved`, `Rejected` |
| `NotificationType` | `NewChapter`, `NewComment`, `CommentReply`, `CommentLike`, `NewFollower`, `BadgeEarned`, `ReportUpdate`, `SystemAlert` |

---

## 5. API Response Format

Mọi response đều dùng `ApiResponse<T>`:

```json
{
  "success": true,
  "message": null,
  "data": { ... }
}
```

| Tình huống | HTTP Status | `success` |
|---|---|---|
| GET / PUT thành công | 200 OK | `true` |
| POST tạo mới | 201 Created | `true` |
| DELETE | 200 OK | `true` |
| Lỗi validation | 400 Bad Request | `false` |
| Chưa xác thực | 401 Unauthorized | `false` |
| Không có quyền | 403 Forbidden | `false` |
| Không tìm thấy | 404 Not Found | `false` |
| Trùng lặp | 409 Conflict | `false` |
| Lỗi server | 500 Internal Server Error | `false` |

---

## 6. Coding Checklist — Thêm Feature Mới

Thực hiện **đúng thứ tự**:

```
1.  Domain        → Entity / Enum nếu cần
2.  Infrastructure → Fluent API config (Persistences/Configs/)
3.  Infrastructure → EF Migration
4.  Application   → IRepository interface (Common/Interfaces/Repositories/)
5.  Application   → DTOs (DTOs/<Feature>/RequestDto + ResponseDto)
6.  Application   → IUseCase interface (Common/Interfaces/UseCases/)
7.  Application   → FluentValidation validator (UseCases/Validators/<Feature>/)
8.  Application   → UseCase implementation (gọi ValidateAndThrowAsync() đầu tiên)
9.  Application   → Đăng ký UseCase AddScoped<> trong DependencyInjection.cs
10. Infrastructure → Repository implementation
11. Infrastructure → Đăng ký trong DependencyInjection.cs
12. WebAPI        → Controller endpoint (thin — chỉ gọi use case, trả ApiResponse)
13. WebAPI        → Thêm [Authorize] + [Authorize(Roles = "...")] nếu cần
```

---

## 7. Các Quy Tắc Bắt Buộc

### Architecture
- **Controllers không được** inject `LitNovelContext`, Repository class, hoặc Infrastructure type. Chỉ inject interface từ `Application.Common.Interfaces`.
- **Controllers không được** chứa business logic hay bắt exception để convert sang HTTP.
- **Entity → DTO mapping** thuộc về UseCase, không phải Controller.
- **Repository với `Select()`** có thể project thẳng sang DTO — không cần mapping thêm trong UseCase.

### Database
- `AsNoTracking()` cho mọi read-only query.
- `Select()` để project chỉ các field cần thiết.
- Không dùng `Include()` khi đã dùng `Select()`.
- `AsSplitQuery()` khi có nhiều collection `Include()`.
- **Lazy loading bị cấm hoàn toàn.**
- `SaveChangesAsync()` chỉ được gọi **một lần** cuối UseCase thông qua `IUnitOfWork`.

### Async
- Mọi I/O phải là `async Task<T>`.
- **Không dùng** `.Result`, `.Wait()`, `async void`.
- `CancellationToken` bắt buộc trên mọi async method.

### Validation
- Chỉ dùng **FluentValidation** — không dùng DataAnnotations.
- `ValidateAndThrowAsync()` là lệnh **đầu tiên** trong `ExecuteAsync()`.

### Enums
- Luôn dùng enum value, không dùng magic string.
- Lưu dưới dạng string trong DB qua `.HasConversion<string>()`.

### Error Handling
- Throw custom exception từ UseCase/Service.
- `ExceptionHandlingMiddleware` xử lý tất cả → trả `ApiResponse<T>`.

```csharp
throw new NotFoundException("Novel not found.");
throw new ForbiddenException("You do not have permission.");
throw new ConflictException("Email already exists.");
```

### Authorization
- Dùng `[Authorize]` cho endpoint yêu cầu đăng nhập.
- Dùng `[Authorize(Roles = "Staff,Admin")]` cho endpoint giới hạn role.
- Kiểm tra ownership (author của novel) trong UseCase thông qua `ICurrentUserService`.

---

## 8. Business Rules Quan Trọng

### Novel Lifecycle
```
Draft (author tạo)
  → Pending (submit để duyệt)
  → Ongoing / Rejected (Staff duyệt)
  → Ended / Hiatus / Dropped / Canceled
```

### Chapter Lifecycle
```
Draft → Pending → Published / Rejected
      → Scheduled (lên lịch đăng)
```

### Ownership Rules
- Chỉ **Author** (người tạo) mới được edit/delete novel, volume, chapter của mình.
- **Staff/Admin** có thể override bất kỳ nội dung nào.
- **User** chỉ edit/delete comment của chính mình.

### Rating
- Mỗi User chỉ được đánh giá **1 lần** mỗi novel (unique constraint `UserId + NovelId`).
- Rating từ 1–5 sao (byte).

### Comments
- Hỗ trợ nested comments (1 cấp reply) thông qua `ParentCommentId`.
- Xóa chapter → cascade xóa toàn bộ comments.
- Xóa user → Restrict (không xóa được nếu còn comment — dùng soft ban).

### ReadingProgress
- Mỗi user có **1 bản ghi** per novel (composite PK `UserId + NovelId`).
- Lưu chapter đang đọc gần nhất và % progress.

### Reports
- `NovelReport`: báo cáo truyện (có thể kèm chapter cụ thể).
- `UserReport`: báo cáo user (có thể kèm comment cụ thể).
- Staff/Admin xử lý → cập nhật `Status`, `ActionTaken`, `ResolutionNotes`.

### Notifications
- Tạo `Notification` record khi có sự kiện liên quan.
- `EntityType` + `EntityId` xác định đối tượng liên quan (lazy reference).
- `IsRead = false` mặc định, user đánh dấu đã đọc.

---

## 9. Out of Scope (v1.0)

Không implement các tính năng sau:

- Mobile app (iOS/Android)
- Real-time chat
- Live streaming, Audio novel
- AI-generated content
- Monetization / Premium subscription
- Payment gateway
- Multi-language translation
- Offline reading
- ML recommendation engine

---

## 10. Acceptance Criteria

| Feature | Criteria |
|---|---|
| Authentication | Register ✓, Login ✓, Logout ✓ |
| Reading | Browse novels ✓, Read chapters ✓, History tracked ✓ |
| Publishing | Create novel ✓, Create chapter ✓, Moderation workflow ✓ |
| Moderation | Staff approve/reject ✓, Reports processed ✓ |
| Administration | Manage users ✓, Manage categories/tags ✓ |
