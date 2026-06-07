# LitNovel Platform Specification

Version: 1.0
Project: LitNovel – Online Novel Reading & Publishing Platform

---

# 1. Context & Goal

## Context

LitNovel is a web-based platform that enables users to discover, read, publish, review, and discuss novels online.

The platform supports both readers and writers. Readers can browse novels, read chapters, interact through ratings and comments, and participate in forum discussions. Writers can create and manage their own novels, volumes, and chapters.

To ensure content quality and community safety, all published content is subject to moderation by Staff members. Administrative users manage platform-wide settings, categories, tags, users, and reports.

---

## Business Goals

* Build an online community for novel readers and writers.
* Allow authors to publish and manage novels efficiently.
* Provide moderation tools to ensure content quality.
* Encourage community engagement through reviews, comments, likes, and forums.
* Support scalable management of novels, chapters, users, and reports.

---

## User Goals

### Readers

* Find interesting novels quickly.
* Continue reading from previous progress.
* Interact through ratings, reviews, comments, and likes.

### Authors

* Publish and manage novels.
* Track moderation status.
* Build an audience of followers and readers.

### Moderators

* Review and approve submitted content.
* Resolve community reports.

### Administrators

* Manage users and platform data.
* Monitor overall platform performance.

---

# 2. Actors & Roles

## Guest

Unauthenticated visitor.

Permissions:

* Register account
* Login
* Browse public novels
* View novel details

---

## User

Authenticated platform member.

Permissions:

* Read novels and chapters
* Favorite novels
* Like novels
* Comment and review content
* Report violations
* Publish novels and chapters
* Participate in forums

---

## Staff

Content moderator.

Permissions:

* Review submitted novels
* Review submitted chapters
* Handle reports
* Moderate comments
* Moderate forums
* Warn users

Staff inherits all User permissions.

---

## Admin

System administrator.

Permissions:

* Manage users
* Manage staff roles
* Manage categories and tags
* Manage forums
* Manage notifications
* View system statistics
* Override any content

Admin inherits all Staff permissions.

---

# 3. Functional Requirements

## FR-01 Authentication

The system shall:

* Register new accounts.
* Authenticate users.
* Support logout.
* Support password reset.
* Support profile management.

---

## FR-02 Novel Discovery

The system shall:

* Display novel catalog.
* Search novels by keyword.
* Filter novels by category.
* Filter novels by tags.
* Sort novels by popularity, views, rating, and update date.

---

## FR-03 Reading

The system shall:

* Display novel details.
* Display chapter content.
* Track reading progress.
* Maintain reading history.
* Maintain favorite novels.
* Maintain liked novels.

---

## FR-04 User Interaction

The system shall:

* Allow ratings.
* Allow reviews.
* Allow comments.
* Allow nested replies.
* Allow likes.
* Allow violation reports.

---

## FR-05 Novel Publishing

The system shall:

* Create novels.
* Update novels.
* Delete novels.
* Create volumes.
* Manage volume ordering.
* Create chapters.
* Update chapters.
* Delete chapters.

---

## FR-06 Moderation Workflow

The system shall:

* Submit novels for approval.
* Submit chapters for approval.
* Approve content.
* Reject content.
* Lock content.
* Record moderation history.

---

## FR-07 Forum

The system shall:

* Create threads.
* Reply to threads.
* Edit owned content.
* Delete owned content.
* Vote on discussions.
* Report violations.

---

## FR-08 Notifications

The system shall:

* Notify users of moderation results.
* Notify users of warnings.
* Notify users of system announcements.
* Notify users of forum interactions.

---

## FR-09 Administration

The system shall:

* Manage users.
* Manage staff roles.
* Manage categories.
* Manage tags.
* Manage forums.
* Manage notifications.
* View reports.
* View audit logs.
* View statistics.

---

# 4. Non-Functional Requirements

## Performance

* Page load time < 3 seconds under normal load.
* Search results returned within 2 seconds.
* Support at least 1,000 concurrent users.

---

## Security

* Passwords stored using BCrypt.
* JWT-based authentication.
* Role-based access control (RBAC).
* Protection against SQL Injection.
* Protection against XSS attacks.
* Protection against CSRF attacks.

---

## Availability

* System uptime target: 99%.
* Daily database backup.

---

## Scalability

* Support growth to 100,000+ users.
* Support millions of chapter views.

---

## Maintainability

* Clean architecture.
* Modular backend services.
* API documentation available.

---

## Usability

* Responsive design.
* Mobile-friendly interface.
* Consistent UI components.

---

# 5. Data Model

> Database: **SQL Server (LitNovelDB)** — ORM: **Entity Framework Core 10**  
> Chi tiết đầy đủ xem tại [data.md](./data.md)

---

## Enums

| Enum | Giá trị |
|---|---|
| **UserRole** | `User`, `Staff`, `Admin` |
| **UserStatus** | `Offline`, `Online`, `Banned` |
| **NovelStatus** | `Pending`, `Ongoing`, `Ended`, `Hiatus`, `Dropped`, `Canceled` |
| **ChapterStatus** | `Draft`, `Published`, `Scheduled` |
| **ReportType** | `Spam`, `Inappropriate`, `Copyright`, `Harassment`, `Other` |
| **ReportStatus** | `Pending`, `Resolved`, `Rejected` |
| **NotificationType** | `NewChapter`, `NewComment`, `CommentReply`, `CommentLike`, `NewFollower`, `BadgeEarned`, `ReportUpdate`, `SystemAlert` |

---

## User

* `Id` — int, PK
* `Username` — nvarchar(50), NOT NULL, UNIQUE
* `Email` — nvarchar(256), NOT NULL, UNIQUE
* `PasswordHash` — nvarchar(max), NOT NULL
* `Avatar` — nvarchar(512), nullable
* `Bio` — nvarchar(1000), nullable
* `Status` — UserStatus (string), NOT NULL, default `Offline`
* `Role` — UserRole (string), NOT NULL, default `User`
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## UserReputation

* `Id` — int, PK
* `UserId` — FK → Users (Cascade)
* `Score` — int, default 0
* `UpdatedAt` — datetime2

---

## RefreshToken

* `Id` — int, PK
* `UserId` — FK → Users (Cascade)
* `Token` — nvarchar(512), UNIQUE
* `ExpiresAt` — datetime2
* `IsRevoked` — bool
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## Badge

* `Id` — int, PK
* `Key` — nvarchar(100), UNIQUE
* `Name` — nvarchar(100)
* `Description` — nvarchar(500)
* `Icon` — nvarchar(512), nullable
* `Color` — nvarchar(20), nullable

---

## UserBadge *(junction)*

* `UserId` — FK → Users (Cascade) *(PK composite)*
* `BadgeId` — FK → Badges (Cascade) *(PK composite)*
* `EarnedAt` — datetime2

---

## Category

* `Id` — int, PK
* `Name` — nvarchar(100)
* `Slug` — nvarchar(120), UNIQUE

---

## Tag

* `Id` — int, PK
* `Name` — nvarchar(100)
* `Slug` — nvarchar(120), UNIQUE

---

## Novel

* `Id` — int, PK
* `Title` — nvarchar(200), NOT NULL
* `Slug` — nvarchar(220), UNIQUE
* `Description` — nvarchar(5000), nullable
* `CoverImage` — nvarchar(512), nullable
* `AuthorId` — FK → Users (Restrict)
* `CategoryId` — FK → Categories (SetNull), nullable
* `Status` — NovelStatus (string), default `Pending`
* `ViewCount`, `LikeCount`, `DislikeCount` — int, default 0
* `TotalChapters`, `TotalVolumes` — int, default 0
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## NovelTag *(junction)*

* `NovelId` — FK → Novels (Cascade) *(PK composite)*
* `TagId` — FK → Tags (Cascade) *(PK composite)*

---

## Volume

* `Id` — int, PK
* `NovelId` — FK → Novels (Cascade)
* `VolumeNumber` — int, UNIQUE per novel
* `Title` — nvarchar(200)

---

## Chapter

* `Id` — int, PK
* `VolumeId` — FK → Volumes (Cascade)
* `ChapterNumber` — int, UNIQUE per volume
* `Title` — nvarchar(200)
* `Slug` — nvarchar(220), UNIQUE
* `ReleaseDate` — datetime2, nullable
* `Status` — ChapterStatus (string), default `Draft`
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## ChapterContent

* `Id` — int, PK
* `ChapterId` — FK → Chapters (Cascade), UNIQUE (1:1)
* `Content` — nvarchar(max), NOT NULL
* `Version` — int, default 1

> Tách nội dung chương thành bảng riêng để tối ưu hiệu năng khi liệt kê chương.

---

## CommentChapter

* `Id` — int, PK
* `Content` — nvarchar(2000), NOT NULL
* `UserId` — FK → Users (Restrict)
* `ChapterId` — FK → Chapters (Cascade)
* `LikeCount`, `DislikeCount` — int, default 0
* `ParentCommentId` — FK → CommentChapters (Restrict), nullable *(self-reference — nested comments)*
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## Favorite *(junction)*

* `UserId` — FK → Users (Cascade) *(PK composite)*
* `NovelId` — FK → Novels (Cascade) *(PK composite)*
* `CreatedAt` — datetime2

---

## NovelRating

* `Id` — int, PK
* `NovelId` — FK → Novels (Cascade)
* `UserId` — FK → Users (Restrict)
* `Rating` — tinyint (1–5), UNIQUE per user per novel
* `Review` — nvarchar(3000), nullable
* `CreatedAt` — datetime2
* `UpdatedAt` — datetime2, nullable

---

## ReadingProgress

* `UserId` — FK → Users (Cascade) *(PK composite)*
* `NovelId` — FK → Novels (Cascade) *(PK composite)*
* `ChapterId` — FK → Chapters (Restrict)
* `ProgressPercentage` — tinyint (0–100)
* `LastReadAt` — datetime2

---

## Notification

* `Id` — int, PK
* `UserId` — FK → Users (Cascade)
* `NotificationType` — NotificationType (string)
* `EntityType` — nvarchar(100), nullable *(e.g. `"Chapter"`, `"Comment"`)*
* `EntityId` — int, nullable
* `Message` — nvarchar(1000), NOT NULL
* `IsRead` — bool, default false
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## NovelReport

* `Id` — int, PK
* `ReportType` — ReportType (string)
* `Description` — nvarchar(2000), nullable
* `Status` — ReportStatus (string), default `Pending`
* `ActionTaken` — nvarchar(1000), nullable
* `ResolutionNotes` — nvarchar(1000), nullable
* `ReporterId` — FK → Users (Restrict)
* `ProcessedById` — FK → Users (SetNull), nullable
* `TargetNovelId` — FK → Novels (Restrict)
* `TargetChapterId` — FK → Chapters (SetNull), nullable
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

## UserReport

* `Id` — int, PK
* `ReportType` — ReportType (string)
* `Description` — nvarchar(2000), nullable
* `Status` — ReportStatus (string), default `Pending`
* `ActionTaken` — nvarchar(1000), nullable
* `ResolutionNotes` — nvarchar(1000), nullable
* `ReporterId` — FK → Users (Restrict)
* `ProcessedById` — FK → Users (SetNull), nullable
* `TargetUserId` — FK → Users (Restrict)
* `TargetCommentId` — FK → CommentChapters (SetNull), nullable
* `CreatedAt`, `UpdatedAt` — datetime2, auto-set

---

# 6. Error Handling

## Authentication Errors

* Invalid credentials.
* Account banned.
* Session expired.
* Unauthorized access.

---

## Validation Errors

* Required field missing.
* Invalid email format.
* Password policy violation.
* File upload exceeds limit.

---

## Publishing Errors

* Novel not found.
* Chapter not found.
* Permission denied.
* Content pending moderation.

---

## System Errors

* Internal server error.
* Database connection failure.
* External service unavailable.

---

# 7. Acceptance Criteria

## Authentication

* Users can register successfully.
* Users can login successfully.
* Users can logout successfully.

---

## Reading

* Users can browse novels.
* Users can read chapters.
* Reading history is recorded correctly.

---

## Publishing

* Authors can create novels.
* Authors can create chapters.
* Moderation workflow functions correctly.

---


## Moderation

* Staff can approve/reject content.
* Reports can be processed successfully.

---

## Administration

* Admin can manage users.
* Admin can manage categories and tags.
* Audit logs are generated correctly.

---

# 8. Out of Scope

The following features are excluded from Version 1.0:

* Mobile applications (iOS/Android)
* Real-time chat system
* Live streaming
* Audio novels
* AI-generated novel content
* Novel monetization system
* Premium subscription system
* Payment gateway integration
* Advertisement management
* Multi-language translation engine
* Offline reading mode
* Recommendation engine based on machine learning
