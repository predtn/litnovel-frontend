# 📚 LitNovel – Tài Liệu Kiến Trúc Hệ Thống

> Hệ thống đọc truyện chữ trực tuyến, bao gồm Backend API (.NET 10) và Frontend Web (.NET 10 Razor Pages).

---

## 1. Tổng Quan Hệ Thống

LitNovel là ứng dụng web cho phép người dùng đọc, viết và quản lý truyện chữ (light novel). Hệ thống được chia thành hai thành phần chính:

| Thành phần | Công nghệ | Mô tả |
|---|---|---|
| **Backend** | ASP.NET Core 10 Web API | Cung cấp RESTful API, xử lý nghiệp vụ, lưu trữ dữ liệu |
| **Frontend** | ASP.NET Core 10 Razor Pages | Giao diện người dùng web, gọi API backend |

---

## 2. Kiến Trúc Tổng Thể

```
┌──────────────────────────────────────────────┐
│              litnovel-frontend               │
│         (ASP.NET Core Razor Pages)           │
│  Pages/ ─── Bootstrap + jQuery + AJAX/HTTP  │
└──────────────────────┬───────────────────────┘
                       │ HTTP REST API
                       ▼
┌──────────────────────────────────────────────┐
│              litnovel-backend                │
│         (ASP.NET Core 10 Web API)            │
│                                              │
│  ┌──────────────┐                            │
│  │  WebAPI      │  Controllers, Middlewares  │
│  └──────┬───────┘                            │
│         │                                    │
│  ┌──────▼───────┐                            │
│  │  Application │  Use Cases, DTOs,          │
│  │              │  Validators (FluentVal.)   │
│  └──────┬───────┘                            │
│         │                                    │
│  ┌──────▼───────┐                            │
│  │  Domain      │  Entities, Enums,          │
│  │              │  BaseEntity, BaseReport    │
│  └──────────────┘                            │
│         │                                    │
│  ┌──────▼───────┐                            │
│  │Infrastructure│  EF Core, SQL Server,      │
│  │              │  Repositories, Services    │
│  └──────────────┘                            │
└──────────────────────────────────────────────┘
                       │
                       ▼
              ┌─────────────────┐
              │  SQL Server DB  │
              │  (LitNovelDB)   │
              └─────────────────┘
```

---

## 3. Backend – Kiến Trúc Clean Architecture

Backend tuân theo **Clean Architecture** (Onion Architecture), gồm 4 layer:

```
litnovel-backend/
├── LitNovel.Domain/          # Layer 1: Domain (lõi nghiệp vụ)
├── LitNovel.Application/     # Layer 2: Application (use cases)
├── LitNovel.Infrastructure/  # Layer 3: Infrastructure (EF Core, DB)
└── LitNovel.WebAPI/          # Layer 4: Presentation (API endpoints)
```

### 3.1 Dependency Flow

```
WebAPI ──► Application ──► Domain
  │                           ▲
  └──► Infrastructure ────────┘
```

- **Domain** không phụ thuộc vào bất kỳ layer nào
- **Application** chỉ phụ thuộc vào Domain
- **Infrastructure** phụ thuộc vào Domain và Application
- **WebAPI** phụ thuộc vào Application và Infrastructure

---

## 4. Chi Tiết Từng Layer

### 4.1 Domain Layer (`LitNovel.Domain`)

Chứa các entity thuần túy và enums, không phụ thuộc vào bất kỳ framework nào.

#### 📁 Cấu trúc thư mục

```
LitNovel.Domain/
├── Common/
│   ├── BaseEntity.cs         # Base class: Id, CreatedAt, UpdatedAt
│   └── BaseReport.cs         # Base class cho báo cáo vi phạm
├── Entities/                 # 19 entities
│   ├── User.cs
│   ├── Novel.cs
│   ├── Volume.cs
│   ├── Chapter.cs
│   ├── ChapterContent.cs
│   ├── Category.cs
│   ├── Tag.cs
│   ├── NovelTag.cs
│   ├── Badge.cs
│   ├── UserBadge.cs
│   ├── UserReputation.cs
│   ├── RefreshToken.cs
│   ├── Favorite.cs
│   ├── NovelRating.cs
│   ├── NovelReport.cs
│   ├── UserReport.cs
│   ├── ReadingProgress.cs
│   ├── CommentChapter.cs
│   └── Notification.cs
└── Enums/
    ├── UserRole.cs           # User, Admin, ...
    ├── UserStatus.cs         # Online, Offline, Banned, ...
    ├── NovelStatus.cs        # Pending, Ongoing, Ended, Hiatus, Dropped, Canceled
    ├── ChapterStatus.cs      # Draft, Published, ...
    ├── ReportType.cs         # Spam, Inappropriate, Copyright, Harassment, Other
    ├── ReportStatus.cs       # Pending, Resolved, ...
    └── NotificationType.cs   # NewChapter, NewComment, CommentReply, CommentLike, NewFollower, BadgeEarned, ReportUpdate, SystemAlert
```

#### 🗂 Entity Relationships (ERD)

```
User ──── (1:N) ──── Novel              # Author viết Novel
User ──── (1:N) ──── RefreshToken       # JWT refresh tokens
User ──── (1:1) ──── UserReputation     # Điểm uy tín
User ──── (N:M) ──── Badge (qua UserBadge)
User ──── (N:M) ──── Novel (Favorites)  # Yêu thích
User ──── (1:N) ──── NovelRating        # Đánh giá truyện
User ──── (1:N) ──── CommentChapter     # Bình luận chương
User ──── (1:N) ──── Notification       # Thông báo
User ──── (1:N) ──── ReadingProgress    # Tiến trình đọc

Novel ──── (N:1) ──── Category          # Thể loại truyện
Novel ──── (N:M) ──── Tag (qua NovelTag)
Novel ──── (1:N) ──── Volume            # Tập truyện
Volume ──── (1:N) ──── Chapter          # Chương
Chapter ──── (1:1) ──── ChapterContent  # Nội dung chương
Chapter ──── (1:N) ──── CommentChapter  # Bình luận
Chapter ──── (1:N) ──── ReadingProgress # Tiến độ đọc

CommentChapter ──── (tự tham chiếu) ──── CommentChapter (Replies)

NovelReport ──── (N:1) ──── Novel       # Báo cáo vi phạm truyện
UserReport ──── báo cáo User/Comment
```

#### 📝 Chi tiết các Entity chính

**User**
```csharp
Id, Username, Email, PasswordHash, Avatar, Bio,
Status (UserStatus), Role (UserRole),
→ RefreshTokens, UserBadges, Novels, CommentChapters,
  TargetReports, Favorites, ReadingProgresses,
  Notifications, NovelRatings, Reputation
```

**Novel**
```csharp
Id, Title, Slug, Description, CoverImage, AuthorId,
Status (NovelStatus), ViewCount, LikeCount, DislikeCount,
CategoryId, TotalChapters, TotalVolumes,
→ Author(User), Category, Volumes, NovelTags,
  Favorites, NovelRatings, TargetReports, NovelProgresses
```

**Chapter**
```csharp
Id, VolumeId, ChapterNumber, Title, Slug,
ReleaseDate, Status (ChapterStatus),
→ Volume, Content(ChapterContent), CommentChapters, ChapterProgresses
```

**CommentChapter** *(hỗ trợ nested comments)*
```csharp
Id, Content, UserId, ChapterId, LikeCount, DislikeCount,
ParentCommentId (nullable → self-reference),
→ User, Chapter, ParentComment, Replies, TargetReports
```

**BaseReport** *(abstract)*
```csharp
ReportType, Description, Status (ReportStatus),
ActionTaken, ResolutionNotes, ReporterId, ProcessedById,
→ Reporter (User), ProcessedBy (User)
```

---

### 4.2 Application Layer (`LitNovel.Application`)

Chứa logic nghiệp vụ (use cases), định nghĩa interfaces, và validation.

#### 📁 Cấu trúc thư mục

```
LitNovel.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── Repositories/     # Interface cho repositories
│   │   ├── Services/         # Interface cho services
│   │   └── UseCases/         # Interface cho use cases
│   ├── Models/               # Shared models
│   └── Exceptions/           # Custom exceptions
├── DTOs/                     # Data Transfer Objects
├── UseCases/
│   └── Validators/           # FluentValidation validators
└── DependencyInjection.cs    # Đăng ký FluentValidation
```

#### 🔧 NuGet Packages

| Package | Version | Mục đích |
|---|---|---|
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | Validation input |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.8 | DI container |

#### 💉 Dependency Injection

```csharp
// DependencyInjection.cs
services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
```

---

### 4.3 Infrastructure Layer (`LitNovel.Infrastructure`)

Triển khai cụ thể các interfaces, kết nối database qua Entity Framework Core.

#### 📁 Cấu trúc thư mục

```
LitNovel.Infrastructure/
├── Persistences/
│   ├── LitNovelContext.cs         # EF Core DbContext (19 DbSets)
│   ├── Configs/                   # Entity configurations (Fluent API)
│   │   ├── UserConfiguration.cs
│   │   ├── NovelConfiguration.cs
│   │   ├── ChapterConfiguration.cs
│   │   ├── ... (19 files tổng cộng)
│   ├── Migrations/                # EF Core migrations
│   └── Repositories/              # Repository implementations
├── Services/                      # External service implementations
└── DependencyInjection.cs         # Đăng ký DbContext
```

#### 🗄 Database Context (`LitNovelContext`)

```csharp
// 19 DbSets được đăng ký:
DbSet<User>, DbSet<RefreshToken>, DbSet<UserReputation>,
DbSet<Badge>, DbSet<UserBadge>,
DbSet<Novel>, DbSet<Category>, DbSet<Tag>, DbSet<NovelTag>,
DbSet<Volume>, DbSet<Chapter>, DbSet<ChapterContent>,
DbSet<CommentChapter>, DbSet<Favorite>, DbSet<NovelRating>,
DbSet<ReadingProgress>, DbSet<Notification>,
DbSet<NovelReport>, DbSet<UserReport>
```

**Auto-timestamps**: `SaveChanges()` và `SaveChangesAsync()` tự động gán `CreatedAt`/`UpdatedAt` cho mọi `BaseEntity`.

#### 🔧 NuGet Packages

| Package | Version | Mục đích |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.8 | ORM cho SQL Server |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.8 | EF CLI tools (migrations) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.8 | JWT authentication |

#### 💉 Dependency Injection

```csharp
// DependencyInjection.cs
services.AddDbContext<LitNovelContext>(options =>
    options.UseSqlServer(connectionString));
```

---

### 4.4 WebAPI Layer (`LitNovel.WebAPI`)

Presentation layer – nhận HTTP request, trả HTTP response.

#### 📁 Cấu trúc thư mục

```
LitNovel.WebAPI/
├── Controllers/                   # API Controllers (đang phát triển)
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs  # Global error handler
├── Common/
│   └── Models/
│       └── ApiResponse.cs         # Generic API response wrapper
├── Configs/
│   └── JwtConfig.cs               # JWT configuration class
├── Program.cs                     # Entry point
├── DependencyInjection.cs         # Đăng ký services
└── appsettings.json               # Cấu hình ứng dụng
```

#### 🌐 API Response Format

```json
{
  "success": true | false,
  "message": "string | null",
  "data": <T> | null
}
```

#### 🛡 Exception Handling Middleware

Bắt toàn bộ exceptions và map sang HTTP status codes:

| Exception | HTTP Status |
|---|---|
| `ValidationException` (FluentValidation) | 400 Bad Request |
| `BadRequestException` | 400 Bad Request |
| `UnauthorizedException` | 401 Unauthorized |
| `ForbiddenException` | 403 Forbidden |
| `NotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| Các lỗi khác | 500 Internal Server Error |

#### ⚙ Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "MyCnn": "Server=.;Database=LitNovelDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

#### 🔧 NuGet Packages

| Package | Version | Mục đích |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.3 | OpenAPI / Swagger support |
| `Swashbuckle.AspNetCore` | 10.2.1 | Swagger UI |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.8 | EF Core design-time tools |

#### 💉 Dependency Injection

```csharp
// Program.cs
builder.Services.AddWebAPI();         // Controllers + OpenAPI + Middleware
builder.Services.AddApplication();    // FluentValidation
builder.Services.AddInfrastructure(); // EF Core + SQL Server
```

#### 🔗 HTTP Pipeline

```
Request
  → ExceptionHandlingMiddleware
  → HTTPS Redirection
  → Controllers
Response
```

---

## 5. Frontend – ASP.NET Core Razor Pages

Frontend là một ứng dụng Razor Pages riêng biệt, giao tiếp với Backend qua HTTP.

#### 📁 Cấu trúc thư mục

```
litnovel-frontend/
└── litnovel-frontend/
    ├── Pages/
    │   ├── Index.cshtml / .cs       # Trang chủ
    │   ├── Privacy.cshtml / .cs     # Trang Privacy
    │   ├── Error.cshtml / .cs       # Trang lỗi
    │   ├── _ViewImports.cshtml      # Global using directives
    │   ├── _ViewStart.cshtml        # Khai báo layout mặc định
    │   └── Shared/
    │       ├── _Layout.cshtml       # Master layout (Bootstrap 5)
    │       ├── _Layout.cshtml.css   # Scoped CSS cho layout
    │       └── _ValidationScriptsPartial.cshtml
    ├── wwwroot/
    │   ├── css/site.css             # Custom styles
    │   ├── js/site.js               # Custom scripts
    │   ├── lib/
    │   │   ├── bootstrap/           # Bootstrap 5
    │   │   └── jquery/              # jQuery
    │   └── favicon.ico
    ├── Program.cs                   # Entry point (Razor Pages)
    └── appsettings.json
```

#### 🖥 Tech Stack Frontend

| Công nghệ | Vai trò |
|---|---|
| ASP.NET Core 10 Razor Pages | Server-side rendering |
| Bootstrap 5 | CSS framework |
| jQuery | DOM manipulation, AJAX |
| Vanilla CSS (site.css) | Custom styles |

#### 🔗 HTTP Pipeline (Frontend)

```
Request
  → Static Files
  → Routing
  → Authorization
  → Razor Pages
Response
```

---

## 6. Luồng Dữ Liệu (Data Flow)

### Ví dụ: Người dùng đọc một chương truyện

```
Browser
  │
  │ GET /novel/{slug}/chapter/{chapterSlug}
  ▼
Frontend (Razor Page)
  │
  │ HTTP GET /api/chapters/{id}
  ▼
Backend WebAPI (Controller)
  │
  │ gọi Use Case / Service
  ▼
Application Layer
  │
  │ validate input (FluentValidation)
  │ xử lý nghiệp vụ
  ▼
Infrastructure Layer (Repository)
  │
  │ LINQ query qua EF Core
  ▼
SQL Server (LitNovelDB)
  │
  │ trả dữ liệu ngược lên
  ▼
ApiResponse<ChapterDTO>
  │
  ▼
Frontend renders HTML → Browser
```

---

## 7. Database Schema Tổng Quan

```
┌─────────────┐    ┌──────────────┐    ┌──────────────┐
│    Users    │    │    Novels    │    │  Categories  │
├─────────────┤    ├──────────────┤    ├──────────────┤
│ Id (PK)     │◄───│ AuthorId(FK) │    │ Id (PK)      │
│ Username    │    │ CategoryId──►│───►│ Name         │
│ Email       │    │ Title        │    └──────────────┘
│ PasswordHash│    │ Slug         │
│ Avatar      │    │ Status       │    ┌──────────────┐
│ Role        │    │ ViewCount    │    │     Tags     │
│ Status      │    │ LikeCount    │    ├──────────────┤
└──────┬──────┘    └──────┬───────┘    │ Id (PK)      │
       │                  │            │ Name         │
       │           ┌──────▼───────┐    └──────┬───────┘
       │           │  NovelTags   │           │
       │           ├──────────────┤    ────────┘
       │           │ NovelId (FK) │
       │           │ TagId (FK)   │
       │           └──────────────┘
       │
       │           ┌──────────────┐    ┌──────────────┐
       │           │   Volumes    │    │   Chapters   │
       │           ├──────────────┤    ├──────────────┤
       │           │ Id (PK)      │◄───│ VolumeId(FK) │
       │           │ NovelId (FK) │    │ ChapterNumber│
       │           │ VolumeNumber │    │ Title, Slug  │
       │           │ Title        │    │ Status       │
       │           └──────────────┘    └──────┬───────┘
       │                                      │
       │           ┌──────────────┐    ┌──────▼───────┐
       │           │ChapterContent│    │  CommentChapter│
       │           ├──────────────┤    ├──────────────┤
       │           │ ChapterId(FK)│    │ Id, Content  │
       │           │ Content(text)│    │ UserId (FK)  │
       │           └──────────────┘    │ ChapterId(FK)│
       │                               │ ParentId(FK) │ ← self-ref
       │                               └──────────────┘
       │
       ├─── UserReputation (1:1)
       ├─── RefreshTokens (1:N)
       ├─── UserBadges (N:M với Badges)
       ├─── Favorites (N:M với Novels)
       ├─── NovelRatings (N:M với Novels)
       ├─── ReadingProgress (N:M với Novels + Chapters)
       ├─── Notifications (1:N)
       ├─── NovelReports (as Reporter)
       └─── UserReports (as Reporter)
```

---

## 8. Security & Authentication

| Cơ chế | Chi tiết |
|---|---|
| **Authentication** | JWT Bearer Token (cấu hình qua `JwtConfig`) |
| **Token Refresh** | Entity `RefreshToken` với `ExpiresAt`, `IsRevoked` |
| **Password** | Lưu dạng `PasswordHash` (không plain text) |
| **Authorization** | Role-based: `UserRole` (User, Admin, ...) |
| **HTTPS** | Bắt buộc (`UseHttpsRedirection`) |

---

## 9. Công Nghệ Sử Dụng

### Backend

| Công nghệ | Version | Mục đích |
|---|---|---|
| .NET | 10.0 | Runtime |
| ASP.NET Core Web API | 10.0 | REST API framework |
| Entity Framework Core | 10.0.8 | ORM |
| SQL Server | - | Cơ sở dữ liệu |
| FluentValidation | 12.1.1 | Input validation |
| JWT Bearer | 10.0.8 | Authentication |
| Swagger / OpenAPI | 10.0.3 / 10.2.1 | API documentation |

### Frontend

| Công nghệ | Version | Mục đích |
|---|---|---|
| .NET | 10.0 | Runtime |
| ASP.NET Core Razor Pages | 10.0 | Server-side web UI |
| Bootstrap | 5.x | CSS framework |
| jQuery | - | JavaScript utilities |

---

## 10. Cấu Trúc Solution Files

```
d:\Ky8\PRN232\Project\
├── litnovel-backend/
│   ├── LitNovel.slnx                  # Solution file
│   ├── LitNovel.Domain/
│   ├── LitNovel.Application/
│   ├── LitNovel.Infrastructure/
│   └── LitNovel.WebAPI/
│
└── litnovel-frontend/
    ├── litnovel-frontend.slnx          # Solution file
    └── litnovel-frontend/
        └── (Razor Pages app)
```

---

## 11. Trạng Thái Phát Triển

> ⚠️ Dự án hiện đang trong giai đoạn **khởi tạo / phát triển ban đầu**.

| Thành phần | Trạng thái |
|---|---|
| Domain Entities (19 entities) | ✅ Hoàn thành |
| EF Core Configurations (19 files) | ✅ Hoàn thành |
| Database Context | ✅ Hoàn thành |
| Exception Middleware | ✅ Hoàn thành |
| API Response Wrapper | ✅ Hoàn thành |
| Application Interfaces | 🔲 Đang phát triển (placeholder) |
| Repositories | 🔲 Đang phát triển (placeholder) |
| Use Cases / Services | 🔲 Đang phát triển |
| API Controllers | 🔲 Chưa có |
| JWT Authentication | 🔲 Package cài, chưa implement |
| Frontend Pages | 🔲 Chỉ có trang mặc định |
| Frontend ↔ Backend integration | 🔲 Chưa thực hiện |

---

*Tài liệu được tạo tự động từ phân tích source code – LitNovel Project – PRN232*
