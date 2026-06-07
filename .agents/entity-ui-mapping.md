# LitNovel — Entity-to-UI Mapping Specification

> Version: 1.0  
> Source of truth: `data.md` (19 backend entities) + `screen.md` (64 screens)  
> Used by: UI generators, Figma generators, Frontend / Backend developers, QA engineers

---

# 1. Mapping Rules

## 1.1 Entity Field Types

| Type | Definition | Examples |
|---|---|---|
| **PK** | Primary Key — system-generated, never editable by user | `Id` on all entities |
| **FK** | Foreign Key — references another entity | `Novel.AuthorId`, `Chapter.VolumeId` |
| **System Field** | Auto-managed by backend / EF Core | `CreatedAt`, `UpdatedAt` |
| **Business Field** | Core domain data entered or managed by users | `Novel.Title`, `User.Bio` |
| **Computed Field** | Derived at runtime, not stored or derived from aggregates | `Novel.ViewCount` (counter), rating average |

## 1.2 UI Usage Types

Each field is tagged with one or more of:

| Tag | Meaning |
|---|---|
| **Display** | Read-only display in detail view or table column |
| **Editable** | User can modify in a form |
| **CreateOnly** | Set at creation, immutable afterwards |
| **UpdateOnly** | Not shown at creation, editable only on update |
| **Searchable** | Used in full-text search queries |
| **Filterable** | Used in filter dropdowns / checkbox groups |
| **Sortable** | Column can be sorted ascending/descending |
| **Hidden** | Exists in data model, never shown in UI |
| **SystemManaged** | Set by backend, never exposed to user input |

## 1.3 Component Mapping Rules

| Data Type | Default UI Component | Notes |
|---|---|---|
| `string` short (≤ 200) | Text Input | Single line |
| `string` medium (≤ 1000) | Text Area | Multi-line |
| `string` long (> 1000) | Rich Text Editor | Novel description, chapter content |
| `string` URL / slug | Text Input | With slug auto-generation option |
| `string` enum | Select Dropdown | Pre-defined options |
| `string` password | Password Input | Masked, show/hide toggle |
| `string` image URL | Upload Component | Preview thumbnail |
| `int` / `byte` | Number Input | With min/max |
| `bool` | Toggle Switch | On/Off |
| `DateTime` | Date Picker | With optional time |
| `DateTime?` | Date Picker (nullable) | Clearable |
| Enum (status) | Select Dropdown (form) / Status Badge (display) | |
| FK reference | Select / Autocomplete Dropdown | Loads options from API |
| FK reference (multi) | Multi-Select | Tags, categories |
| Count / metric | Statistic Card / read-only number | Never editable |
| Collection | List / Table | Child records |
| Computed aggregate | Display-only text / badge | |

---

# 2. Entity Mapping

---

## Entity: User

### Description
Registered platform member. Central entity connected to nearly all other entities. Has role-based access control.

### Relationships
```
User
├─ UserReputation (1:1, Cascade)
├─ RefreshTokens (1:N, Cascade)
├─ UserBadges (1:N, via junction)
├─ Novels (1:N as Author, Restrict)
├─ CommentChapters (1:N, Restrict)
├─ Favorites (1:N, Cascade)
├─ NovelRatings (1:N, Restrict)
├─ ReadingProgresses (1:N, Cascade)
├─ Notifications (1:N, Cascade)
├─ NovelReports (1:N as Reporter, Restrict)
└─ UserReports (1:N as Reporter/Target, Restrict)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-02 | Create (Register) |
| SCR-03 | Authenticate (Login) |
| SCR-06 | Edit own profile |
| SCR-07 | View public profile |
| SCR-08 | Change password |
| SCR-09 | Display authenticated user info in header |
| SCR-48 | Admin — list and manage all users |
| SCR-49 | Admin — view user detail, change role/status |
| SCR-50 | Admin — manage staff users |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `Username` | nvarchar(50) | Text Input | ✓ | — | ✓ | ✓ | — | ✓ |
| `Email` | nvarchar(256) | Email Input | ✓ | — | ✓ | ✓ | — | — |
| `PasswordHash` | nvarchar(max) | Password Input | ✓ | — | Hidden | — | — | — |
| `Avatar` | nvarchar(512) | Upload Component | — | ✓ | ✓ | — | — | — |
| `Bio` | nvarchar(1000) | Text Area | — | ✓ | ✓ | — | — | — |
| `Status` | UserStatus (string) | Select (admin) / Status Badge (display) | SystemManaged | Admin only | ✓ | — | ✓ | — |
| `Role` | UserRole (string) | Select (admin) / Role Badge (display) | SystemManaged | Admin only | ✓ | — | ✓ | — |
| `CreatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | ✓ |
| `UpdatedAt` | datetime2 | Date display | SystemManaged | — | Hidden | — | — | — |

### Forms

#### Register Form (SCR-02)
Fields: `Username`, `Email`, `Password`, `ConfirmPassword`  
Validation:
- `Username`: Required, 3–50 chars, `^[a-zA-Z0-9_]+$`, unique
- `Email`: Required, valid email, max 256 chars, unique
- `Password`: Required, min 8 chars, ≥1 uppercase, ≥1 digit
- `ConfirmPassword`: Must match `Password`

#### Edit Profile Form (SCR-06)
Editable fields: `Avatar`, `Bio`  
Read-only: `Username`, `Email`, `Status`, `Role`, `CreatedAt`

#### Change Password Form (SCR-08)
Fields: `CurrentPassword`, `NewPassword`, `ConfirmNewPassword`

#### Admin Edit Form (SCR-48, SCR-49)
Editable by Admin: `Role`, `Status`

### Table Usage (SCR-48 — User Management)

#### Columns

| Column | Field | Visible | Sortable | Filterable |
|---|---|---|---|---|
| Avatar + Username | `Avatar`, `Username` | ✓ | — | — |
| Email | `Email` | ✓ | — | — |
| Role | `Role` | ✓ | — | ✓ |
| Status | `Status` | ✓ | — | ✓ |
| Joined | `CreatedAt` | ✓ | ✓ | — |
| Novels | count(`Novels`) | ✓ | ✓ | — |
| Actions | — | ✓ | — | — |

#### Row Actions: [View] [Edit Role] [Ban/Unban] [Delete]

### Detail View (SCR-07 — Public Profile)
Sections:
1. **Header**: `Avatar`, `Username`, `Bio`, `CreatedAt`, `UserReputation.Score`
2. **Badges**: `UserBadges` → `Badge.Name`, `Badge.Icon`, `Badge.Color`
3. **Published Novels**: filtered `Novels` list

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| View public profile | ✓ | ✓ | ✓ | ✓ |
| Edit own profile | — | ✓ (own) | ✓ (own) | ✓ (own) |
| View all users list | — | — | — | ✓ |
| Change role | — | — | — | ✓ |
| Ban/Unban user | — | — | ✓ | ✓ |
| Delete user | — | — | — | ✓ |

---

## Entity: UserReputation

### Description
1:1 with User. Tracks community reputation score.

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-07 | Display reputation score on public profile |
| SCR-49 | Admin — view user reputation |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `UserId` | int FK | — | SystemManaged | — | Hidden | — | — | — |
| `Score` | int | Number display | SystemManaged | SystemManaged | ✓ | — | — | ✓ |
| `UpdatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | — |

### Permissions
All roles: read-only. Score managed by system events only.

---

## Entity: RefreshToken

### Description
Stores JWT refresh tokens for session management. Fully system-managed.

### Screen Usage
No direct UI. Managed transparently via auth endpoints.

### Field Mapping

| Field | DB Type | UI Component | View |
|---|---|---|---|
| `Id` | int PK | — | Admin API only |
| `UserId` | int FK | — | Hidden |
| `Token` | nvarchar(512) | — | Hidden |
| `ExpiresAt` | datetime2 | — | Hidden |
| `IsRevoked` | bool | — | Hidden |

### Permissions
All system-managed. No UI exposure.

---

## Entity: Badge

### Description
Achievement badges awarded to users. Admin-managed catalog.

### Relationships
```
Badge
└─ UserBadges (1:N, Cascade)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-07 | Display earned badges on public profile |
| SCR-51 | Admin — CRUD badge catalog |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `Key` | nvarchar(100) | Text Input | ✓ | — | ✓ | ✓ | — | ✓ |
| `Name` | nvarchar(100) | Text Input | ✓ | ✓ | ✓ | ✓ | — | ✓ |
| `Description` | nvarchar(500) | Text Area | ✓ | ✓ | ✓ | — | — | — |
| `Icon` | nvarchar(512) | Upload / URL Input | ✓ | ✓ | ✓ | — | — | — |
| `Color` | nvarchar(20) | Color Picker | ✓ | ✓ | ✓ | — | — | — |

### Forms

#### Create Badge Form (SCR-51)
All fields. `Key` auto-suggested from `Name` (slugified, editable).

#### Edit Badge Form (SCR-51)
All except `Key` (immutable after creation).

### Permissions

| Action | User | Staff | Admin |
|---|---|---|---|
| View badges | ✓ | ✓ | ✓ |
| Create badge | — | — | ✓ |
| Edit badge | — | — | ✓ |
| Delete badge | — | — | ✓ |
| Award badge to user | — | — | ✓ |

---

## Entity: UserBadge

### Description
Junction table — records which badges a user has earned and when.

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-07 | Display badge strip on public profile |
| SCR-51 | Admin — view badge assignment |

### Field Mapping

| Field | DB Type | UI Component | Create | View |
|---|---|---|---|---|
| `UserId` | int FK (PK) | Autocomplete (admin) | ✓ | ✓ |
| `BadgeId` | int FK (PK) | Select | ✓ | ✓ |
| `EarnedAt` | datetime2 | Date display | SystemManaged | ✓ |

---

## Entity: Category

### Description
Novel genre categories. Admin-managed. Each novel belongs to 0 or 1 category.

### Relationships
```
Category
└─ Novels (1:N, SetNull on delete)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-01 | Category strip on landing page |
| SCR-10 | Filter novels by category |
| SCR-12 | Display novel's category |
| SCR-19, SCR-20 | Category selector in novel form |
| SCR-52 | Admin — CRUD categories |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `Name` | nvarchar(100) | Text Input | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| `Slug` | nvarchar(120) | Text Input | Auto-gen | — | ✓ | — | — | — |

### Forms

#### Create / Edit Category Form (SCR-52)
- `Name`: Required, max 100 chars, unique
- `Slug`: Auto-generated from `Name`, URL-safe, max 120 chars, unique, editable

### Table Usage (SCR-52)

| Column | Sortable | Filterable |
|---|---|---|
| Name | ✓ | — |
| Slug | — | — |
| Novel Count | ✓ | — |
| Actions | — | — |

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| View categories | ✓ | ✓ | ✓ | ✓ |
| Create | — | — | — | ✓ |
| Edit | — | — | — | ✓ |
| Delete | — | — | — | ✓ |

---

## Entity: Tag

### Description
Keyword tags applied to novels. Admin-managed. Novels can have multiple tags.

### Relationships
```
Tag
└─ NovelTags (1:N, Cascade)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-10 | Filter novels by tag |
| SCR-12 | Display novel tags |
| SCR-19, SCR-20 | Multi-select tags in novel form |
| SCR-53 | Admin — CRUD tags |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `Name` | nvarchar(100) | Text Input | ✓ | ✓ | ✓ (pill) | ✓ | ✓ | ✓ |
| `Slug` | nvarchar(120) | Text Input | Auto-gen | — | Hidden | — | — | — |

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| View tags | ✓ | ✓ | ✓ | ✓ |
| Create/Edit/Delete | — | — | — | ✓ |

---

## Entity: Novel

### Description
Core content entity. An author creates and manages novels. Goes through a moderation lifecycle.

### Relationships
```
Novel
├─ User (N:1 as Author, Restrict)
├─ Category (N:1 nullable, SetNull)
├─ NovelTags (1:N → Tags)
├─ Volumes (1:N, Cascade)
├─ Favorites (1:N, Cascade)
├─ NovelRatings (1:N, Cascade)
├─ ReadingProgresses (1:N, Cascade)
└─ NovelReports (1:N, Restrict)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-01 | Featured novel cards |
| SCR-09 | Trending / recommended novels |
| SCR-10 | Full catalog browse |
| SCR-11 | Search results |
| SCR-12 | Full novel detail |
| SCR-18 | Author's own novels dashboard |
| SCR-19 | Create novel |
| SCR-20 | Edit novel |
| SCR-21 | Novel management hub |
| SCR-26 | Novel statistics |
| SCR-27 | Moderation status |
| SCR-39 | Staff — pending novels list |
| SCR-40 | Staff — novel review detail |
| SCR-47 | Admin — total novels stat |
| SCR-60 | Admin — novel override |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `Title` | nvarchar(200) | Text Input | ✓ | ✓ | ✓ | ✓ | — | ✓ |
| `Slug` | nvarchar(220) | Text Input | Auto-gen | — | Hidden | — | — | — |
| `Description` | nvarchar(5000) | Rich Text Editor | ✓ | ✓ | ✓ | — | — | — |
| `CoverImage` | nvarchar(512) | Upload Component | ✓ | ✓ | ✓ | — | — | — |
| `AuthorId` | int FK | Display only | SystemManaged | — | ✓ (linked) | — | — | — |
| `CategoryId` | int FK nullable | Select Dropdown | ✓ | ✓ | ✓ (badge) | — | ✓ | — |
| `Status` | NovelStatus | Status Badge / Select (admin) | SystemManaged | Admin/Staff | ✓ | — | ✓ | — |
| `ViewCount` | int | Number display | SystemManaged | SystemManaged | ✓ | — | — | ✓ |
| `LikeCount` | int | Number display | SystemManaged | SystemManaged | ✓ | — | — | ✓ |
| `DislikeCount` | int | Number display | SystemManaged | SystemManaged | Hidden | — | — | — |
| `TotalChapters` | int | Number display | SystemManaged | SystemManaged | ✓ | — | — | ✓ |
| `TotalVolumes` | int | Number display | SystemManaged | SystemManaged | ✓ | — | — | — |
| `CreatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | ✓ |
| `UpdatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | ✓ |

### Forms

#### Create Novel Form (SCR-19)
Required: `Title`  
Optional: `Description`, `CoverImage`, `CategoryId`, `Tags`  
Validation:
- `Title`: Required, max 200 chars
- `Description`: Max 5000 chars
- `CoverImage`: JPG/PNG/WEBP, max 5MB
- `CategoryId`: Valid category ID or null
- Tags: Max 10 tags

#### Edit Novel Form (SCR-20)
Same fields as Create. `Slug` locked. `Status` read-only for authors.

### Table Usage (SCR-18, SCR-39, SCR-48)

#### Columns

| Column | Field | Sortable | Filterable |
|---|---|---|---|
| Cover + Title | `CoverImage`, `Title` | — | — |
| Status | `Status` | — | ✓ |
| Category | `CategoryId` | — | ✓ |
| Chapters | `TotalChapters` | ✓ | — |
| Views | `ViewCount` | ✓ | — |
| Rating | avg(`NovelRatings.Rating`) | ✓ | — |
| Updated | `UpdatedAt` | ✓ | — |

### Detail View (SCR-12)
Sections:
1. **Header**: `CoverImage`, `Title`, `Status`, `Author`, `Category`, tags, `ViewCount`, `LikeCount`, avg rating
2. **Description**: `Description` (rich text rendered)
3. **Volumes & Chapters**: nested list
4. **Reviews**: `NovelRatings` list
5. **Comments**: top-level `CommentChapters` (novel-level)

### Dashboard Usage
- `ViewCount` → Trending widget (SCR-09, SCR-47)
- `Status = Pending` count → Moderation queue stat card (SCR-38)
- Total `Novel` count → Admin stat card (SCR-47)

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| View novel | ✓ | ✓ | ✓ | ✓ |
| Create novel | — | ✓ | ✓ | ✓ |
| Edit novel | — | ✓ (own) | ✓ (own+any) | ✓ |
| Delete novel | — | ✓ (own) | — | ✓ |
| Submit for review | — | ✓ (own) | ✓ (own) | ✓ |
| Approve/Reject | — | — | ✓ | ✓ |
| Override status | — | — | — | ✓ |

---

## Entity: NovelTag

### Description
Junction table — N:M between Novels and Tags.

### Screen Usage
Managed implicitly through Novel create/edit forms.

### Field Mapping

| Field | UI Exposure |
|---|---|
| `NovelId` | Hidden (set from context) |
| `TagId` | Multi-select in Novel form |

---

## Entity: Volume

### Description
Organizes chapters into named volumes within a novel.

### Relationships
```
Volume
├─ Novel (N:1, Cascade)
└─ Chapters (1:N, Cascade)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-12 | Display volume accordion in chapter list |
| SCR-21 | Novel management — volumes overview |
| SCR-22 | Volume CRUD |
| SCR-25 | Chapter list per volume |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `NovelId` | int FK | — | SystemManaged | — | Hidden | — | — | — |
| `VolumeNumber` | int | Number Input | Auto-increment | ✓ | ✓ | — | — | ✓ |
| `Title` | nvarchar(200) | Text Input | ✓ | ✓ | ✓ | — | — | — |

### Forms

#### Create Volume (SCR-22, inline)
- `VolumeNumber`: Auto-increment, editable
- `Title`: Required, max 200 chars

### Permissions

| Action | User | Staff | Admin |
|---|---|---|---|
| Create volume | ✓ (own novel) | ✓ | ✓ |
| Edit volume | ✓ (own) | ✓ | ✓ |
| Delete volume | ✓ (own) | — | ✓ |

---

## Entity: Chapter

### Description
Individual story chapters within a volume. Has its own moderation lifecycle.

### Relationships
```
Chapter
├─ Volume (N:1, Cascade)
├─ ChapterContent (1:1, Cascade)
├─ CommentChapters (1:N, Cascade)
└─ ReadingProgresses (1:N, Restrict)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-12 | List chapters in Novel Detail |
| SCR-13 | Read chapter content |
| SCR-23 | Create chapter |
| SCR-24 | Edit chapter |
| SCR-25 | Chapter list management |
| SCR-27 | Moderation status per chapter |
| SCR-41 | Staff — pending chapters |
| SCR-42 | Staff — chapter review |
| SCR-61 | Admin — chapter override |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `VolumeId` | int FK | — | SystemManaged | — | Hidden | — | — | — |
| `ChapterNumber` | int | Number Input | Auto-inc | ✓ | ✓ | — | — | ✓ |
| `Title` | nvarchar(200) | Text Input | ✓ | ✓ | ✓ | ✓ | — | — |
| `Slug` | nvarchar(220) | Text Input | Auto-gen | — | Hidden | — | — | — |
| `ReleaseDate` | datetime2? | Date-Time Picker | ✓ | ✓ | ✓ | — | ✓ | ✓ |
| `Status` | ChapterStatus | Select / Status Badge | SystemManaged | — | ✓ | — | ✓ | — |
| `CreatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | ✓ |
| `UpdatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | ✓ |

### Forms

#### Create Chapter (SCR-23)
Required: `ChapterNumber`, `Title`, `Content` (from ChapterContent)  
Optional: `ReleaseDate`  
Validation:
- `ChapterNumber`: Positive int, unique per volume
- `Title`: Required, max 200 chars
- `Content`: Required, min 1 char
- `ReleaseDate`: Future date only (for scheduled)

### Table Usage (SCR-25, SCR-41)

| Column | Field | Sortable | Filterable |
|---|---|---|---|
| # | `ChapterNumber` | ✓ | — |
| Title | `Title` | — | — |
| Status | `Status` | — | ✓ |
| Release Date | `ReleaseDate` | ✓ | ✓ |
| Created | `CreatedAt` | ✓ | — |

### Permissions

| Action | User | Staff | Admin |
|---|---|---|---|
| Create chapter | ✓ (own novel) | ✓ | ✓ |
| Edit chapter | ✓ (own) | ✓ | ✓ |
| Delete chapter | ✓ (own) | — | ✓ |
| Submit for review | ✓ (own) | ✓ | ✓ |
| Approve/Reject | — | ✓ | ✓ |

---

## Entity: ChapterContent

### Description
Stores the full text of a chapter (1:1 with Chapter, separated for performance).

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-13 | Display chapter content for reading |
| SCR-23 | Create chapter content (via editor) |
| SCR-24 | Edit chapter content |
| SCR-42 | Staff — read content for review |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View |
|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden |
| `ChapterId` | int FK | — | SystemManaged | — | Hidden |
| `Content` | nvarchar(max) | Rich Text Editor | ✓ | ✓ | Rendered HTML |
| `Version` | int | Number display | SystemManaged | SystemManaged | ✓ |

---

## Entity: CommentChapter

### Description
User comments on a chapter, with optional nesting (1 level reply via `ParentCommentId`).

### Relationships
```
CommentChapter
├─ User (N:1, Restrict)
├─ Chapter (N:1, Cascade)
└─ CommentChapter (self-ref: N:1 nullable, for replies)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-12 | Novel-level comments (no chapter context) |
| SCR-13 | Chapter-level comments + replies |
| SCR-44 | Staff — targeted comment in user report |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `Content` | nvarchar(2000) | Text Area | ✓ | ✓ | ✓ | — | — | — |
| `UserId` | int FK | — | SystemManaged | — | ✓ (avatar + name) | — | — | — |
| `ChapterId` | int FK | — | SystemManaged | — | Hidden | — | — | — |
| `LikeCount` | int | Number + icon | SystemManaged | SystemManaged | ✓ | — | — | ✓ |
| `DislikeCount` | int | Number + icon | SystemManaged | SystemManaged | ✓ | — | — | — |
| `ParentCommentId` | int FK? | — | SystemManaged | — | Hidden | — | — | — |
| `CreatedAt` | datetime2 | Relative time | SystemManaged | — | ✓ | — | — | ✓ |
| `UpdatedAt` | datetime2 | Relative time | SystemManaged | — | Hidden | — | — | — |

### Forms

#### Create Comment / Reply (SCR-13 inline)
- `Content`: Required, max 2000 chars, min 1 char

#### Edit Comment (inline editor)
- `Content`: Same validation

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| View comments | ✓ | ✓ | ✓ | ✓ |
| Create comment | — | ✓ | ✓ | ✓ |
| Edit comment | — | ✓ (own) | ✓ (any) | ✓ |
| Delete comment | — | ✓ (own) | ✓ (any) | ✓ |
| Like/Dislike | — | ✓ | ✓ | ✓ |
| Report | — | ✓ | ✓ | ✓ |

---

## Entity: Favorite

### Description
Records which novels a user has favorited. Composite PK `(UserId, NovelId)`.

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-12 | Favorite toggle button on novel detail |
| SCR-15 | List user's favorited novels |

### Field Mapping

| Field | DB Type | UI Component | Create | View |
|---|---|---|---|---|
| `UserId` | int FK (PK) | — | SystemManaged | Hidden |
| `NovelId` | int FK (PK) | — | SystemManaged | Hidden |
| `CreatedAt` | datetime2 | Date display | SystemManaged | ✓ |

### Permissions

| Action | User | Staff | Admin |
|---|---|---|---|
| Add favorite | ✓ | ✓ | ✓ |
| Remove favorite | ✓ (own) | ✓ (own) | ✓ |
| View own favorites | ✓ | ✓ | ✓ |

---

## Entity: NovelRating

### Description
User rating (1–5 stars) and optional review text for a novel. One rating per user per novel.

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-12 | Display rating summary + individual reviews tab |
| SCR-26 | Novel statistics — rating distribution |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Search | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — | — |
| `NovelId` | int FK | — | SystemManaged | — | Hidden | — | — | — |
| `UserId` | int FK | — | SystemManaged | — | ✓ (avatar, name) | — | — | — |
| `Rating` | byte (1–5) | Star Select | ✓ | ✓ | ✓ (stars) | — | ✓ | ✓ |
| `Review` | nvarchar(3000) | Text Area | ✓ | ✓ | ✓ | — | — | — |
| `CreatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | — | ✓ |
| `UpdatedAt` | datetime2? | Date display | — | SystemManaged | Hidden | — | — | — |

### Forms

#### Create Rating (SCR-12 modal/inline)
- `Rating`: Required, 1–5
- `Review`: Optional, max 3000 chars
- Business rule: One rating per user per novel (409 Conflict if duplicate)

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| View ratings | ✓ | ✓ | ✓ | ✓ |
| Create rating | — | ✓ (once/novel) | ✓ | ✓ |
| Edit own rating | — | ✓ | ✓ | ✓ |
| Delete any rating | — | — | ✓ | ✓ |

---

## Entity: ReadingProgress

### Description
Tracks the user's last-read position per novel. Composite PK `(UserId, NovelId)`.

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-09 | "Continue Reading" cards |
| SCR-13 | Auto-save reading position |
| SCR-14 | Reading history list |

### Field Mapping

| Field | DB Type | UI Component | View | Sort |
|---|---|---|---|---|
| `UserId` | int FK (PK) | — | Hidden | — |
| `NovelId` | int FK (PK) | — | ✓ (novel title + cover) | — |
| `ChapterId` | int FK | — | ✓ (chapter title) | — |
| `ProgressPercentage` | byte (0–100) | Progress Bar | ✓ | ✓ |
| `LastReadAt` | datetime2 | Relative time | ✓ | ✓ |

### Permissions

| Action | User | Staff | Admin |
|---|---|---|---|
| View own progress | ✓ | ✓ | ✓ |
| Update progress | ✓ (auto, SCR-13) | — | — |
| Delete progress | ✓ (own) | — | ✓ |

---

## Entity: Notification

### Description
In-app notifications sent to users for various events.

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-36 | Notification center list |
| SCR-37 | Individual notification detail |
| Header | Bell icon with unread count badge |

### Field Mapping

| Field | DB Type | UI Component | Create | View | Filter | Sort |
|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | — | — |
| `UserId` | int FK | — | SystemManaged | Hidden | — | — |
| `NotificationType` | enum (string) | Type Icon + Label | SystemManaged | ✓ | ✓ | — |
| `EntityType` | nvarchar(100)? | Linked entity label | SystemManaged | ✓ | — | — |
| `EntityId` | int? | — | SystemManaged | Hidden (used for link) | — | — |
| `Message` | nvarchar(1000) | Text display | SystemManaged | ✓ | — | — |
| `IsRead` | bool | Unread dot / Toggle | SystemManaged | ✓ | ✓ | — |
| `CreatedAt` | datetime2 | Relative time | SystemManaged | ✓ | — | ✓ |

### Permissions

| Action | User | Staff | Admin |
|---|---|---|---|
| View own notifications | ✓ | ✓ | ✓ |
| Mark as read | ✓ | ✓ | ✓ |
| Send system notification | — | — | ✓ |
| Delete notification | ✓ (own) | — | ✓ |

---

## Entity: NovelReport

### Description
User-submitted report about a novel (or specific chapter). Staff/Admin processes reports.

### Relationships
```
NovelReport
├─ User (Reporter, N:1 Restrict)
├─ User (ProcessedBy, N:1 SetNull nullable)
├─ Novel (TargetNovel, N:1 Restrict)
└─ Chapter (TargetChapter, N:1 SetNull nullable)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-12 | "Report" button on novel detail |
| SCR-43 | Staff — reports center (novel reports tab) |
| SCR-44 | Staff — process report |
| SCR-57 | Admin — reports overview |

### Field Mapping

| Field | DB Type | UI Component | Create | Edit | View | Filter | Sort |
|---|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — |
| `ReportType` | ReportType (enum) | Select | ✓ | — | ✓ (badge) | ✓ | — |
| `Description` | nvarchar(2000) | Text Area | ✓ | — | ✓ | — | — |
| `Status` | ReportStatus | Status Badge / Select (staff) | SystemManaged | Staff | ✓ | ✓ | — |
| `ActionTaken` | nvarchar(1000) | Text Area | — | Staff | ✓ | — | — |
| `ResolutionNotes` | nvarchar(1000) | Text Area | — | Staff | ✓ | — | — |
| `ReporterId` | int FK | — | SystemManaged | — | ✓ (linked) | — | — |
| `ProcessedById` | int FK? | — | — | SystemManaged | ✓ (linked) | — | — |
| `TargetNovelId` | int FK | Select / Hidden | ✓ | — | ✓ (linked) | — | — |
| `TargetChapterId` | int FK? | Select optional | ✓ | — | ✓ (linked) | ✓ | — |
| `CreatedAt` | datetime2 | Date display | SystemManaged | — | ✓ | — | ✓ |

### Forms

#### Create Novel Report (SCR-12 — Report Modal)
- `TargetNovelId`: Hidden (from context)
- `TargetChapterId`: Optional select "Specific chapter?"
- `ReportType`: Required select (Spam / Inappropriate / Copyright / Harassment / Other)
- `Description`: Optional, max 2000 chars

#### Process Report (SCR-44 — Staff)
- `ActionTaken`: Required on Resolve, max 1000 chars
- `ResolutionNotes`: Optional, max 1000 chars
- `Status`: Resolved / Rejected

### Permissions

| Action | Guest | User | Staff | Admin |
|---|---|---|---|---|
| Create report | — | ✓ | ✓ | ✓ |
| View own reports | — | ✓ | ✓ | ✓ |
| View all reports | — | — | ✓ | ✓ |
| Process report | — | — | ✓ | ✓ |

---

## Entity: UserReport

### Description
User-submitted report about another user (optionally targeting a specific comment).

### Relationships
```
UserReport
├─ User (Reporter, N:1 Restrict)
├─ User (ProcessedBy, N:1 SetNull nullable)
├─ User (TargetUser, N:1 Restrict)
└─ CommentChapter (TargetComment, N:1 SetNull nullable)
```

### Screen Usage

| Screen | Purpose |
|---|---|
| SCR-07 | "Report User" button on public profile |
| SCR-13 | "Report Comment" in comment context menu |
| SCR-43 | Staff — reports center (user reports tab) |
| SCR-44 | Staff — process report |
| SCR-57 | Admin — reports overview |

### Field Mapping
Same structure as `NovelReport`, replacing `TargetNovelId`/`TargetChapterId` with:

| Field | DB Type | UI Component | Create | Edit | View |
|---|---|---|---|---|---|
| `TargetUserId` | int FK | — (from context) | ✓ | — | ✓ (linked) |
| `TargetCommentId` | int FK? | — (optional context) | ✓ | — | ✓ (linked) |

---

# 3. Forum Entities (Planned — Out of Scope v1.0)

> These entities are referenced in `screen.md` (SCR-28 → SCR-35) but not yet in the backend domain. Mapping is included for design purposes.

## Entity: ForumCategory

| Field | Type | UI Component | Create | Edit | View |
|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden |
| `Name` | string | Text Input | ✓ | ✓ | ✓ |
| `Description` | string | Text Area | ✓ | ✓ | ✓ |
| `Slug` | string | Text Input | Auto-gen | — | Hidden |

---

## Entity: ForumThread

| Field | Type | UI Component | Create | Edit | View | Filter | Sort |
|---|---|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden | — | — |
| `CategoryId` | int FK | Select Dropdown | ✓ | — | ✓ | ✓ | — |
| `UserId` | int FK | — | SystemManaged | — | ✓ (linked) | — | — |
| `Title` | string(200) | Text Input | ✓ | ✓ | ✓ | — | ✓ |
| `Content` | rich text | Rich Text Editor | ✓ | ✓ | ✓ | — | — |
| `Status` | enum | Status Badge | SystemManaged | Staff | ✓ | ✓ | — |
| `VoteScore` | int | Number display | SystemManaged | SystemManaged | ✓ | — | ✓ |
| `CreatedAt` | datetime2 | Relative time | SystemManaged | — | ✓ | — | ✓ |

---

## Entity: ForumPost (Reply)

| Field | Type | UI Component | Create | Edit | View |
|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | Hidden | Hidden |
| `ThreadId` | int FK | — | SystemManaged | — | Hidden |
| `UserId` | int FK | — | SystemManaged | — | ✓ (avatar) |
| `ParentPostId` | int FK? | — | SystemManaged | — | Hidden |
| `Content` | string(5000) | Rich Text Editor | ✓ | ✓ | ✓ |
| `VoteScore` | int | Like/Dislike display | SystemManaged | SystemManaged | ✓ |
| `CreatedAt` | datetime2 | Relative time | SystemManaged | — | ✓ |

---

## Entity: AuditLog (Planned)

| Field | Type | UI Component | View | Filter | Sort |
|---|---|---|---|---|---|
| `Id` | int PK | — | Hidden | — | — |
| `ActorId` | int FK | — | ✓ (username) | ✓ | — |
| `Action` | string | Text | ✓ | ✓ | — |
| `EntityType` | string | Badge | ✓ | ✓ | — |
| `EntityId` | int | Number | ✓ | — | — |
| `CreatedAt` | datetime2 | Datetime display | ✓ | — | ✓ |

---

# 4. Screen-Centric Mapping (Key Screens)

---

## SCR-01 — Landing Page

### Entities Used

| Entity | Purpose |
|---|---|
| Novel | Featured, trending, new releases cards |
| Category | Category strip pills |
| User | Header (if logged in) |

### Display Fields
`Novel.CoverImage`, `Novel.Title`, `Novel.Status`, `Novel.ViewCount`, avg(`NovelRatings.Rating`), `Novel.AuthorId` → `User.Username`, `Category.Name`

### Search Fields
`Novel.Title` (via search bar → SCR-11)

### Filter Fields
`Novel.CategoryId` (category pills)

### Sorting Fields
`Novel.ViewCount` (trending), `Novel.CreatedAt` (new releases)

---

## SCR-02 — Register

### Entities Used

| Entity | Purpose |
|---|---|
| User | Create new user record |

### Editable Fields
`User.Username`, `User.Email`, `User.PasswordHash` (via `Password` input)

### Actions

| Action | Affected Entities |
|---|---|
| Register | Create `User` |

---

## SCR-03 — Login

### Entities Used

| Entity | Purpose |
|---|---|
| User | Authenticate |
| RefreshToken | Issue new refresh token |

### Editable Fields
`User.Email` or `User.Username` (login identifier), `User.PasswordHash` (password input)

---

## SCR-06 — User Profile (Edit)

### Entities Used

| Entity | Purpose |
|---|---|
| User | View + edit own profile |
| UserReputation | Display reputation score |
| UserBadge / Badge | Display badges |
| ReadingProgress | Reading stats count |
| Favorite | Favorites count |

### Display Fields
`User.Username`, `User.Email`, `User.Role`, `User.Status`, `User.CreatedAt`, `UserReputation.Score`

### Editable Fields
`User.Avatar`, `User.Bio`

---

## SCR-10 — Novel Catalog

### Entities Used

| Entity | Purpose |
|---|---|
| Novel | Main content grid |
| Category | Filter sidebar |
| Tag | Filter sidebar (multi-select) |
| User | Author display on cards |

### Display Fields
`Novel.CoverImage`, `Novel.Title`, `Novel.Status`, `Novel.ViewCount`, avg rating, `User.Username` (author), `Category.Name`

### Filter Fields
`Novel.CategoryId`, `Novel.Status`, `NovelTag.TagId`

### Sorting Fields
`Novel.ViewCount`, `Novel.UpdatedAt`, avg(`NovelRatings.Rating`)

### Search Fields
`Novel.Title`

---

## SCR-12 — Novel Detail

### Entities Used

| Entity | Purpose |
|---|---|
| Novel | All metadata |
| User | Author info |
| Category | Category display |
| Tag / NovelTag | Tag pills |
| Volume | Volume accordion |
| Chapter | Chapter list per volume |
| NovelRating | Reviews tab + rating summary |
| CommentChapter | Comments tab |
| Favorite | Favorite toggle state |

### Display Fields
All Novel fields, `Volume.Title`, `Volume.VolumeNumber`, `Chapter.ChapterNumber`, `Chapter.Title`, `Chapter.Status`, `Chapter.CreatedAt`, review cards, comment threads

### Editable Fields (inline)
`NovelRating.Rating`, `NovelRating.Review`, `CommentChapter.Content`

### Actions

| Action | Affected Entities |
|---|---|
| Favorite toggle | Create/Delete `Favorite` |
| Like toggle | Increment/decrement `Novel.LikeCount` |
| Submit rating | Create `NovelRating` |
| Submit comment | Create `CommentChapter` |
| Report | Create `NovelReport` |

---

## SCR-13 — Chapter Reader

### Entities Used

| Entity | Purpose |
|---|---|
| Chapter | Metadata (title, number, status) |
| ChapterContent | Full text content |
| CommentChapter | Comment section |
| ReadingProgress | Auto-save position |

### Display Fields
`Chapter.Title`, `Chapter.ChapterNumber`, `ChapterContent.Content`, `CommentChapter.*`

### Actions

| Action | Affected Entities |
|---|---|
| Auto-save progress | Upsert `ReadingProgress` |
| Submit comment | Create `CommentChapter` |
| Like comment | Increment `CommentChapter.LikeCount` |
| Report comment | Create `UserReport` (TargetCommentId) |

---

## SCR-18 — My Novels Dashboard

### Entities Used

| Entity | Purpose |
|---|---|
| Novel | List of author's novels |
| Volume | Count per novel |
| Chapter | Count + pending status |

### Display Fields
`Novel.CoverImage`, `Novel.Title`, `Novel.Status`, `Novel.TotalChapters`, `Novel.ViewCount`, `Novel.UpdatedAt`

### Filter Fields
`Novel.Status`

### Sorting Fields
`Novel.UpdatedAt`, `Novel.ViewCount`, `Novel.TotalChapters`

---

## SCR-38 — Moderation Dashboard

### Entities Used

| Entity | Purpose |
|---|---|
| Novel | Count where Status=Pending |
| Chapter | Count where Status=Pending |
| NovelReport | Count where Status=Pending |
| UserReport | Count where Status=Pending |

### Display Fields
Aggregate counts, recent activity feed

---

## SCR-43 — Reports Center

### Entities Used

| Entity | Purpose |
|---|---|
| NovelReport | Novel reports tab |
| UserReport | User reports tab |
| User | Reporter + target display |
| Novel | Target novel display |

### Display Fields
`ReportType`, `Status`, `CreatedAt`, reporter username, target entity name

### Filter Fields
`ReportType`, `Status`

### Sorting Fields
`CreatedAt`

---

## SCR-47 — Admin Dashboard

### Entities Used

| Entity | Purpose |
|---|---|
| User | Total count, new this week |
| Novel | Total count, pending count |
| Chapter | Total count |
| NovelReport | Open reports count |
| UserReport | Open reports count |

### Display Fields
Aggregate metrics, trend charts

---

## SCR-48 — User Management

### Entities Used

| Entity | Purpose |
|---|---|
| User | Full CRUD table |

### Display Fields
`User.Avatar`, `User.Username`, `User.Email`, `User.Role`, `User.Status`, `User.CreatedAt`, novel count

### Search Fields
`User.Username`, `User.Email`

### Filter Fields
`User.Role`, `User.Status`

### Sorting Fields
`User.CreatedAt`, `User.Username`

### Actions

| Action | Affected Entities |
|---|---|
| Ban user | Update `User.Status` |
| Change role | Update `User.Role` |
| Delete user | Delete `User` |

---

# 5. CRUD Matrix

| Entity | Create | Read | Update | Delete |
|---|---|---|---|---|
| **User** | Register (Guest) | All roles | Owner (bio/avatar), Admin (role/status) | Admin only |
| **UserReputation** | System | All | System | System |
| **RefreshToken** | System (login) | System | System | System (logout/revoke) |
| **Badge** | Admin | All | Admin | Admin |
| **UserBadge** | Admin (award) | All | — | Admin |
| **Category** | Admin | All | Admin | Admin |
| **Tag** | Admin | All | Admin | Admin |
| **Novel** | User/Staff/Admin | All | Owner/Staff/Admin | Owner/Admin |
| **NovelTag** | Owner/Staff/Admin | All | Owner/Staff/Admin | Owner/Admin |
| **Volume** | Owner/Staff/Admin | All | Owner/Staff/Admin | Owner/Admin |
| **Chapter** | Owner/Staff/Admin | All | Owner/Staff/Admin | Owner/Admin |
| **ChapterContent** | Owner/Staff/Admin | All | Owner/Staff/Admin | Cascade (Chapter) |
| **CommentChapter** | User/Staff/Admin | All | Owner/Staff/Admin | Owner/Staff/Admin |
| **Favorite** | User+ | Owner/Admin | — | Owner |
| **NovelRating** | User+ (1/novel) | All | Owner | Owner/Staff/Admin |
| **ReadingProgress** | System/User | Owner/Admin | System/User | Owner/Admin |
| **Notification** | System/Admin | Owner | Owner (IsRead) | Owner/Admin |
| **NovelReport** | User+ | Owner/Staff/Admin | Staff/Admin | — |
| **UserReport** | User+ | Owner/Staff/Admin | Staff/Admin | — |

### Role-Expanded CRUD

| Entity | Guest | User | Staff | Admin |
|---|---|---|---|---|
| User | C (register) | R (own) | R (list) | CRUD |
| Novel | R | CRUD (own) | RU (any) | CRUD |
| Chapter | R (published) | CRUD (own) | RU (any) | CRUD |
| CommentChapter | R | CRU (own) D (own) | CRU (any) D (any) | CRUD |
| NovelRating | R | CRU (own, 1x) | R | CRUD |
| Favorite | — | CRD (own) | — | CRUD |
| Notification | — | R (own), U (IsRead) | R (own) | CRUD |
| NovelReport | — | C | RU (process) | CRUD |
| UserReport | — | C | RU (process) | CRUD |
| Category | R | R | R | CRUD |
| Tag | R | R | R | CRUD |
| Badge | R | R | R | CRUD |

---

# 6. Validation Matrix

| Entity | Field | Required | Min | Max | Format / Regex | Business Rule |
|---|---|---|---|---|---|---|
| **User** | `Username` | ✓ | 3 | 50 | `^[a-zA-Z0-9_]+$` | Unique |
| **User** | `Email` | ✓ | — | 256 | Valid email | Unique |
| **User** | `Password` (input) | ✓ | 8 | 128 | ≥1 uppercase, ≥1 digit | — |
| **User** | `Bio` | — | — | 1000 | — | — |
| **User** | `Avatar` | — | — | 512 (URL len) | JPG/PNG/WEBP, ≤2MB | — |
| **Badge** | `Key` | ✓ | 1 | 100 | `^[a-z0-9_]+$` | Unique |
| **Badge** | `Name` | ✓ | 1 | 100 | — | — |
| **Badge** | `Description` | ✓ | 1 | 500 | — | — |
| **Category** | `Name` | ✓ | 1 | 100 | — | Unique |
| **Category** | `Slug` | ✓ | 1 | 120 | URL-safe slug | Unique |
| **Tag** | `Name` | ✓ | 1 | 100 | — | Unique |
| **Tag** | `Slug` | ✓ | 1 | 120 | URL-safe slug | Unique |
| **Novel** | `Title` | ✓ | 1 | 200 | — | — |
| **Novel** | `Description` | — | — | 5000 | — | — |
| **Novel** | `CoverImage` | — | — | 512 (URL) | JPG/PNG/WEBP, ≤5MB | — |
| **Novel** | `CategoryId` | — | — | — | Valid ID or null | — |
| **Novel** | `Tags` | — | 0 | 10 (count) | — | Valid tag IDs |
| **Volume** | `VolumeNumber` | ✓ | 1 | — | Positive int | Unique per novel |
| **Volume** | `Title` | ✓ | 1 | 200 | — | — |
| **Chapter** | `ChapterNumber` | ✓ | 1 | — | Positive int | Unique per volume |
| **Chapter** | `Title` | ✓ | 1 | 200 | — | — |
| **Chapter** | `Content` | ✓ | 1 | — | — | Via ChapterContent |
| **Chapter** | `ReleaseDate` | — | — | — | Valid datetime | Future date only |
| **CommentChapter** | `Content` | ✓ | 1 | 2000 | — | — |
| **NovelRating** | `Rating` | ✓ | 1 | 5 | Integer (tinyint) | One per user per novel |
| **NovelRating** | `Review` | — | — | 3000 | — | — |
| **NovelReport** | `ReportType` | ✓ | — | — | Valid enum value | — |
| **NovelReport** | `Description` | — | — | 2000 | — | — |
| **NovelReport** | `ActionTaken` | ✓ (on Resolve) | 1 | 1000 | — | Required when resolving |
| **UserReport** | `ReportType` | ✓ | — | — | Valid enum value | — |
| **UserReport** | `Description` | — | — | 2000 | — | — |
| **Notification** | `Message` | ✓ (system) | 1 | 1000 | — | System-generated |
| **ForumThread** | `Title` | ✓ | 5 | 200 | — | — |
| **ForumThread** | `Content` | ✓ | 10 | — | — | — |
| **ForumPost** | `Content` | ✓ | 1 | 5000 | — | — |

---

# 7. UI Generation Constraints

1. **Every editable field maps to exactly one entity attribute.** No composite fields. No fields that span multiple entities in a single input.

2. **No screen contains fields not defined in entities.** All displayed or editable data must trace to a specific `Entity.Field` in this document or `data.md`.

3. **Every table column maps to an entity field** or a computed aggregate:
   - Direct field: `Novel.Title` → "Title" column
   - Computed: `COUNT(Chapters) WHERE NovelId = x` → "Chapters" column
   - Derived: AVG(`NovelRating.Rating`) → "Rating" column

4. **Every filter maps to an entity field:**
   - `Status` filter → `Novel.Status` (enum values)
   - `Category` filter → `Novel.CategoryId` → `Category.Id`
   - `Role` filter → `User.Role` (enum values)

5. **Every search box defines searchable fields:**
   - Global search → `Novel.Title`, `User.Username`
   - Catalog search → `Novel.Title`
   - User management search → `User.Username`, `User.Email`

6. **Every action identifies affected entities:**

| Action | Input Entity | Output / Mutated Entity |
|---|---|---|
| Register | — | Create `User` |
| Login | `User.Email`/`Username` | Create `RefreshToken` |
| Create Novel | `Novel` fields | `Novel` + `NovelTags` |
| Submit Chapter | `Chapter.Id` | Update `Chapter.Status` |
| Approve Novel | `Novel.Id`, reviewer notes | Update `Novel.Status`, Create `Notification` |
| Reject Novel | `Novel.Id`, notes | Update `Novel.Status`, Create `Notification` |
| Save Reading Progress | `ReadingProgress` fields | Upsert `ReadingProgress` |
| Submit Rating | `NovelRating` fields | Create `NovelRating` |
| Like Novel | `Novel.Id` | Increment `Novel.LikeCount` |
| Favorite Novel | `Novel.Id` | Create `Favorite` |
| Submit Comment | `CommentChapter` fields | Create `CommentChapter` |
| Report Novel | `NovelReport` fields | Create `NovelReport` |
| Report User | `UserReport` fields | Create `UserReport` |
| Resolve Report | `ReportId`, action notes | Update `NovelReport.Status`/`UserReport.Status`, Create `Notification` |
| Ban User | `User.Id` | Update `User.Status = Banned` |
| Send Notification | `Notification` fields | Create `Notification` records |
| Award Badge | `UserId`, `BadgeId` | Create `UserBadge`, Create `Notification` |

---

*LitNovel Entity-to-UI Mapping Specification — v1.0*  
*Generated from: `data.md`, `screen.md`, `spec.md`, `screendesign.md`, `component-library.md`*
