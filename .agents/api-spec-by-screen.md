# LitNovel — API Specification by Screen

> Version: 1.0  
> Base URL: `/api`  
> Organized by: Screen → API endpoints  
> Source: `screen.md` (64 screens) + `api-spec.md`

---

# MODULE 1 — AUTHENTICATION & PROFILE

---

## SCR-01 — Landing Page

**Purpose:** Showcase featured/trending novels and provide login/register entry points.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels` | Fetch featured/trending novels |
| `GET` | `/api/categories` | Fetch category list for filter strip |

---

### `GET /api/novels`

**Permission:** Guest+

**Query used on this screen:**
```
GET /api/novels?sort=viewCount&order=desc&status=Ongoing&page=1&size=6
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "The Dragon War Chronicles",
        "slug": "the-dragon-war-chronicles",
        "coverImage": "https://cdn.example.com/covers/1.webp",
        "author": { "id": 5, "username": "author_name", "avatar": "..." },
        "category": { "id": 3, "name": "Fantasy" },
        "tags": [{ "id": 1, "name": "Action" }],
        "status": "Ongoing",
        "viewCount": 128432,
        "ratingAverage": 4.2,
        "updatedAt": "2024-01-12T08:00:00Z"
      }
    ],
    "page": 1,
    "size": 6,
    "totalElements": 156,
    "totalPages": 26
  }
}
```

---

### `GET /api/categories`

**Permission:** Guest+

**Response:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Fantasy", "slug": "fantasy" },
    { "id": 2, "name": "Romance", "slug": "romance" },
    { "id": 3, "name": "Action", "slug": "action" }
  ]
}
```

---

## SCR-02 — Register

**Purpose:** Create a new user account.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Submit registration form |

---

### `POST /api/auth/register`

**Permission:** Guest

**Request:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "Password123"
}
```

**Validation:**

| Field | Rule |
|---|---|
| `username` | Required, 3–50 chars, `^[a-zA-Z0-9_]+$` |
| `email` | Required, valid email, max 256 chars |
| `password` | Required, min 8 chars, ≥1 uppercase, ≥1 digit |

**Success — 201 Created:**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "role": "User",
    "createdAt": "2024-01-12T10:00:00Z"
  }
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Username must be 3–50 characters" | Length validation |
| 400 | "Invalid email format" | Email format |
| 400 | "Password must be at least 8 characters" | Weak password |
| 409 | "Email already exists" | Duplicate email |
| 409 | "Username already taken" | Duplicate username |

---

## SCR-03 — Login

**Purpose:** Authenticate user and issue JWT tokens.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Submit login form |

---

### `POST /api/auth/login`

**Permission:** Guest

**Request:**
```json
{
  "identifier": "john@example.com",
  "password": "Password123"
}
```

> `identifier` accepts email or username.

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "dGhpcyBp...",
    "expiresIn": 900,
    "user": {
      "id": 1,
      "username": "john_doe",
      "email": "john@example.com",
      "avatar": "https://cdn.example.com/avatars/1.webp",
      "role": "User",
      "status": "Online"
    }
  }
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Invalid credentials" | Wrong password |
| 401 | "Invalid credentials" | User not found |
| 403 | "Account has been banned" | Banned user |

---

## SCR-04 — Forgot Password

**Purpose:** Request a password reset email.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/forgot-password` | Send reset email |

---

### `POST /api/auth/forgot-password`

**Permission:** Guest

**Request:**
```json
{
  "email": "john@example.com"
}
```

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "If this email is registered, you will receive a reset link shortly",
  "data": null
}
```

> Always returns 200 to prevent email enumeration.

---

## SCR-05 — Reset Password

**Purpose:** Set a new password via reset token link.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/reset-password` | Submit new password |

---

### `POST /api/auth/reset-password`

**Permission:** Guest (with valid reset token)

**Request:**
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

**Validation:**

| Field | Rule |
|---|---|
| `token` | Required |
| `newPassword` | Required, min 8 chars, ≥1 uppercase, ≥1 digit |
| `confirmPassword` | Must match `newPassword` |

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Password reset successfully. Please login.",
  "data": null
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Passwords do not match" | Mismatch |
| 400 | "Invalid or expired reset token" | Token expired/invalid |

---

## SCR-06 — User Profile (Edit)

**Purpose:** View and update own profile info.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/me` | Load profile data |
| `PUT` | `/api/users/me` | Save profile changes |

---

### `GET /api/users/me`

**Permission:** User+

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "avatar": "https://cdn.example.com/avatars/1.webp",
    "bio": "I love reading fantasy novels.",
    "role": "User",
    "status": "Online",
    "reputation": 120,
    "badges": [
      {
        "key": "first_novel",
        "name": "First Novel",
        "icon": "https://cdn.example.com/badges/fn.svg",
        "color": "#FFD700",
        "earnedAt": "2024-01-15T08:00:00Z"
      }
    ],
    "stats": {
      "novelsCreated": 3,
      "chaptersPublished": 42,
      "favoritesCount": 18,
      "commentsCount": 87
    },
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### `PUT /api/users/me`

**Permission:** User+

**Request:**
```json
{
  "avatar": "https://cdn.example.com/avatars/new.webp",
  "bio": "Updated bio text."
}
```

**Validation:**

| Field | Rule |
|---|---|
| `avatar` | Optional, valid URL, max 512 chars |
| `bio` | Optional, max 1000 chars |

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": 1,
    "username": "john_doe",
    "avatar": "https://cdn.example.com/avatars/new.webp",
    "bio": "Updated bio text.",
    "updatedAt": "2024-01-12T10:30:00Z"
  }
}
```

---

## SCR-07 — Public Profile

**Purpose:** View another user's public profile.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/{id}` | Load user public profile |
| `GET` | `/api/novels?authorId={id}` | Load user's published novels |
| `POST` | `/api/reports/users` | Report this user |

---

### `GET /api/users/{id}`

**Permission:** Guest+

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 5,
    "username": "author_name",
    "avatar": "https://cdn.example.com/avatars/5.webp",
    "bio": "Prolific fantasy writer.",
    "reputation": 350,
    "badges": [],
    "stats": { "novelsPublished": 3 },
    "joinedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Error:**

| Status | Message | Cause |
|---|---|---|
| 404 | "User not found" | Invalid ID |

---

### `POST /api/reports/users`

**Permission:** User+

**Request:**
```json
{
  "targetUserId": 5,
  "targetCommentId": null,
  "reportType": "Harassment",
  "description": "This user is harassing others."
}
```

---

## SCR-08 — Change Password

**Purpose:** Change own account password.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `PUT` | `/api/users/me/password` | Submit password change |

---

### `PUT /api/users/me/password`

**Permission:** User+

**Request:**
```json
{
  "currentPassword": "Password123",
  "newPassword": "NewPassword456",
  "confirmPassword": "NewPassword456"
}
```

**Validation:**

| Field | Rule |
|---|---|
| `currentPassword` | Required |
| `newPassword` | Required, min 8 chars, ≥1 uppercase, ≥1 digit, different from current |
| `confirmPassword` | Must match `newPassword` |

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Password changed successfully",
  "data": null
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Current password is incorrect" | Wrong current |
| 400 | "New password must differ from current" | Same password |
| 400 | "Passwords do not match" | Confirm mismatch |

---

# MODULE 2 — READING MODULE

---

## SCR-09 — Home Page (Authenticated)

**Purpose:** Personalized homepage with continue reading, trending, and new chapters.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/me/reading-history` | Continue reading section |
| `GET` | `/api/novels?sort=viewCount&order=desc` | Trending novels |
| `GET` | `/api/novels?sort=updatedAt&order=desc` | New releases |
| `GET` | `/api/notifications?isRead=false` | Unread count for bell |

---

### `GET /api/users/me/reading-history`

**Permission:** User+

**Query:** `?filter=in-progress&page=1&size=4`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "novel": {
          "id": 1,
          "title": "The Dragon War Chronicles",
          "coverImage": "https://cdn.example.com/covers/1.webp",
          "status": "Ongoing"
        },
        "lastChapter": { "id": 45, "chapterNumber": 45, "title": "Chapter 45" },
        "progressPercentage": 52,
        "lastReadAt": "2024-01-12T08:00:00Z"
      }
    ],
    "page": 1, "size": 4, "totalElements": 8, "totalPages": 2
  }
}
```

---

## SCR-10 — Novel Catalog

**Purpose:** Browse all published novels with filters and sort.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels` | Main novel grid |
| `GET` | `/api/categories` | Category filter options |
| `GET` | `/api/tags` | Tag filter options |

---

### `GET /api/novels`

**Permission:** Guest+

**Full query parameters for this screen:**
```
GET /api/novels
  ?keyword=dragon
  &categoryId=3
  &tagId=1&tagId=4
  &status=Ongoing
  &sort=viewCount
  &order=desc
  &page=1
  &size=20
```

**Response:** Paginated novel list (see SCR-01 response format)

---

### `GET /api/tags`

**Permission:** Guest+

**Response:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Action", "slug": "action" },
    { "id": 2, "name": "Romance", "slug": "romance" }
  ]
}
```

---

## SCR-11 — Search Results

**Purpose:** Display novels (and authors) matching a search query.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels?keyword={q}` | Novel search results |
| `GET` | `/api/users?keyword={q}` | Author search results |

---

### `GET /api/novels?keyword={query}`

**Permission:** Guest+

**Query:** `?keyword=dragon&page=1&size=20`

**Response:** Same paginated format as SCR-10.

**Empty result response:**
```json
{
  "success": true,
  "data": {
    "items": [],
    "page": 1,
    "size": 20,
    "totalElements": 0,
    "totalPages": 0
  }
}
```

---

### `GET /api/users?keyword={query}`

**Permission:** Guest+

**Query:** `?keyword=john&page=1&size=10`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 5,
        "username": "john_author",
        "avatar": "https://cdn.example.com/avatars/5.webp",
        "novelsPublished": 3
      }
    ],
    "page": 1, "size": 10, "totalElements": 2, "totalPages": 1
  }
}
```

---

## SCR-12 — Novel Detail

**Purpose:** Full novel info, chapter list, reviews, and comments.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{id}` | Load novel detail + volumes + chapters |
| `POST` | `/api/novels/{id}/favorites` | Add to favorites |
| `DELETE` | `/api/novels/{id}/favorites` | Remove from favorites |
| `POST` | `/api/novels/{id}/likes` | Like novel |
| `DELETE` | `/api/novels/{id}/likes` | Unlike novel |
| `GET` | `/api/novels/{id}/reviews` | Load reviews tab |
| `POST` | `/api/novels/{id}/reviews` | Submit rating/review |
| `PUT` | `/api/reviews/{id}` | Edit own review |
| `DELETE` | `/api/reviews/{id}` | Delete review |
| `POST` | `/api/reports/novels` | Report this novel |

---

### `GET /api/novels/{id}`

**Permission:** Guest+ (published only; owner/staff/admin see all statuses)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "The Dragon War Chronicles",
    "slug": "the-dragon-war-chronicles",
    "coverImage": "https://cdn.example.com/covers/1.webp",
    "description": "<p>An epic tale...</p>",
    "author": { "id": 5, "username": "author_name", "avatar": "..." },
    "category": { "id": 3, "name": "Fantasy" },
    "tags": [{ "id": 1, "name": "Action" }],
    "status": "Ongoing",
    "viewCount": 128432,
    "likeCount": 3200,
    "totalChapters": 87,
    "totalVolumes": 3,
    "ratingAverage": 4.2,
    "ratingCount": 1248,
    "isFavorited": false,
    "isLiked": false,
    "userRating": null,
    "volumes": [
      {
        "id": 1,
        "volumeNumber": 1,
        "title": "Volume 1: The Beginning",
        "chapters": [
          {
            "id": 1,
            "chapterNumber": 1,
            "title": "Chapter 1: The Awakening",
            "status": "Published",
            "createdAt": "2024-01-01T00:00:00Z"
          }
        ]
      }
    ],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-12T08:00:00Z"
  }
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 404 | "Novel not found" | Invalid ID |
| 403 | "Novel is not publicly available" | Not published + not owner |

---

### `POST /api/novels/{id}/favorites`

**Permission:** User+

**Response:**
```json
{ "success": true, "message": "Added to favorites", "data": null }
```

**Error:**
```json
{ "success": false, "message": "Novel already in favorites" }
```

---

### `POST /api/novels/{id}/reviews`

**Permission:** User+ (one per novel, cannot review own novel)

**Request:**
```json
{ "rating": 5, "review": "Absolutely captivating!" }
```

**Validation:**

| Field | Rule |
|---|---|
| `rating` | Required, integer 1–5 |
| `review` | Optional, max 3000 chars |

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 409 | "You have already reviewed this novel" | Duplicate |
| 400 | "Cannot review your own novel" | Author restriction |

---

### `POST /api/reports/novels`

**Permission:** User+

**Request:**
```json
{
  "targetNovelId": 1,
  "targetChapterId": null,
  "reportType": "Inappropriate",
  "description": "Contains explicit content."
}
```

---

## SCR-13 — Chapter Reader

**Purpose:** Read chapter content, navigate between chapters, comment.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/chapters/{id}` | Load chapter content |
| `POST` | `/api/chapters/{id}/progress` | Auto-save reading position |
| `GET` | `/api/chapters/{id}/comments` | Load comments section |
| `POST` | `/api/chapters/{id}/comments` | Post a comment |
| `POST` | `/api/comments/{id}/replies` | Reply to a comment |
| `PUT` | `/api/comments/{id}` | Edit own comment |
| `DELETE` | `/api/comments/{id}` | Delete comment |
| `POST` | `/api/comments/{id}/likes` | Like a comment |
| `DELETE` | `/api/comments/{id}/likes` | Unlike a comment |
| `POST` | `/api/reports/users` | Report comment author |

---

### `GET /api/chapters/{id}`

**Permission:** Guest (published), Owner/Staff/Admin (all statuses)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 45,
    "chapterNumber": 45,
    "title": "Chapter 45: The Final Battle",
    "slug": "chapter-45-the-final-battle",
    "content": "<p>The story continues...</p>",
    "status": "Published",
    "releaseDate": null,
    "volume": { "id": 2, "volumeNumber": 2, "title": "Volume 2" },
    "novel": { "id": 1, "title": "The Dragon War Chronicles", "slug": "..." },
    "prevChapter": { "id": 44, "chapterNumber": 44, "title": "Chapter 44" },
    "nextChapter": { "id": 46, "chapterNumber": 46, "title": "Chapter 46" },
    "createdAt": "2024-01-10T00:00:00Z"
  }
}
```

---

### `POST /api/chapters/{id}/progress`

**Permission:** User+ (auto-called on scroll)

**Request:**
```json
{ "progressPercentage": 64 }
```

**Validation:**

| Field | Rule |
|---|---|
| `progressPercentage` | Required, integer 0–100 |

**Response:**
```json
{
  "success": true,
  "message": "Reading progress saved",
  "data": {
    "novelId": 1,
    "chapterId": 45,
    "progressPercentage": 64,
    "lastReadAt": "2024-01-12T10:10:00Z"
  }
}
```

---

### `GET /api/chapters/{id}/comments`

**Permission:** Guest+

**Query:** `?page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "user": { "id": 5, "username": "commenter", "avatar": "..." },
        "content": "This chapter was amazing!",
        "likeCount": 12,
        "dislikeCount": 0,
        "parentCommentId": null,
        "replies": [
          {
            "id": 2,
            "user": { "id": 7, "username": "reply_user", "avatar": "..." },
            "content": "I agree!",
            "likeCount": 3,
            "dislikeCount": 0,
            "createdAt": "2024-01-12T09:30:00Z"
          }
        ],
        "createdAt": "2024-01-12T09:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 56, "totalPages": 3
  }
}
```

---

### `POST /api/chapters/{id}/comments`

**Permission:** User+

**Request:**
```json
{ "content": "This chapter was amazing!" }
```

**Validation:** `content` required, 1–2000 chars

---

### `POST /api/comments/{id}/replies`

**Permission:** User+

**Request:**
```json
{ "content": "I completely agree!" }
```

---

## SCR-14 — Reading History

**Purpose:** View all novels user has read with progress.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/me/reading-history` | Load reading history list |
| `DELETE` | `/api/users/me/reading-history/{novelId}` | Remove from history |

---

### `GET /api/users/me/reading-history`

**Permission:** User+

**Query:** `?filter=all&page=1&size=20`

**Filter options:** `all` | `in-progress` | `completed`

**Response:** Same format as SCR-09 (paginated ReadingProgressResponseDto)

---

## SCR-15 — Favorites List

**Purpose:** Browse user's favorited novels.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/me/favorites` | Load favorites list |
| `DELETE` | `/api/novels/{id}/favorites` | Remove from favorites |

---

### `GET /api/users/me/favorites`

**Permission:** User+

**Query:** `?sort=createdAt&order=desc&page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "The Dragon War Chronicles",
        "coverImage": "...",
        "author": { "id": 5, "username": "author_name" },
        "status": "Ongoing",
        "ratingAverage": 4.2,
        "favoritedAt": "2024-01-10T08:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 18, "totalPages": 1
  }
}
```

---

## SCR-16 — Liked Novels

**Purpose:** View novels the user has liked.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/me/likes` | Load liked novels |
| `DELETE` | `/api/novels/{id}/likes` | Unlike novel |

---

### `GET /api/users/me/likes`

**Permission:** User+

**Response:** Same paginated format as favorites list.

---

## SCR-17 — Bookmarks

**Purpose:** View bookmarked chapters.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users/me/bookmarks` | Load bookmarks |
| `DELETE` | `/api/bookmarks/{id}` | Remove bookmark |

---

### `GET /api/users/me/bookmarks`

**Permission:** User+

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "novel": { "id": 1, "title": "The Dragon War Chronicles", "coverImage": "..." },
        "chapter": { "id": 45, "chapterNumber": 45, "title": "Chapter 45" },
        "bookmarkedAt": "2024-01-11T20:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 5, "totalPages": 1
  }
}
```

---

# MODULE 3 — PUBLISHING MODULE

---

## SCR-18 — My Novels Dashboard

**Purpose:** Author's overview of all their novels with stats.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/my` | Load own novels list |
| `DELETE` | `/api/novels/{id}` | Delete a novel |

---

### `GET /api/novels/my`

**Permission:** User+

**Query:** `?status=Draft&page=1&size=20&sort=updatedAt&order=desc`

**Status filter options:** `Draft` | `Pending` | `Ongoing` | `Ended` | `Hiatus` | `Dropped` | `Canceled`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 42,
        "title": "My Novel",
        "slug": "my-novel",
        "coverImage": "...",
        "status": "Draft",
        "totalChapters": 5,
        "totalVolumes": 1,
        "viewCount": 0,
        "ratingAverage": 0,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-10T00:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 3, "totalPages": 1
  }
}
```

---

## SCR-19 — Create Novel

**Purpose:** Form to create a new novel draft.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/categories` | Populate category dropdown |
| `GET` | `/api/tags` | Populate tag multi-select |
| `POST` | `/api/novels` | Submit create form |

---

### `POST /api/novels`

**Permission:** User+

**Request:**
```json
{
  "title": "The Dragon War Chronicles",
  "description": "<p>An epic tale of dragons...</p>",
  "coverImage": "https://cdn.example.com/covers/42.webp",
  "categoryId": 3,
  "tagIds": [1, 4, 7]
}
```

**Validation:**

| Field | Rule |
|---|---|
| `title` | Required, 1–200 chars |
| `description` | Optional, max 5000 chars |
| `coverImage` | Optional, valid URL, max 512 chars |
| `categoryId` | Optional, valid category ID or null |
| `tagIds` | Optional, array of valid tag IDs, max 10 |

**Success — 201 Created:**
```json
{
  "success": true,
  "message": "Novel created successfully",
  "data": {
    "id": 42,
    "title": "The Dragon War Chronicles",
    "slug": "the-dragon-war-chronicles",
    "status": "Draft",
    "createdAt": "2024-01-12T10:00:00Z"
  }
}
```

---

## SCR-20 — Edit Novel

**Purpose:** Update an existing novel's metadata.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{id}` | Load existing novel data |
| `GET` | `/api/categories` | Populate category dropdown |
| `GET` | `/api/tags` | Populate tag multi-select |
| `PUT` | `/api/novels/{id}` | Save changes |

---

### `PUT /api/novels/{id}`

**Permission:** Owner / Staff / Admin

**Request:**
```json
{
  "title": "Updated Title",
  "description": "<p>Updated description.</p>",
  "coverImage": "https://cdn.example.com/covers/new.webp",
  "categoryId": 4,
  "tagIds": [1, 2, 5]
}
```

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Novel updated successfully",
  "data": {
    "id": 42,
    "title": "Updated Title",
    "slug": "updated-title",
    "updatedAt": "2024-01-12T11:00:00Z"
  }
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 403 | "You do not have permission to edit this novel" | Not owner/staff |
| 404 | "Novel not found" | Invalid ID |

---

## SCR-21 — Novel Management Detail

**Purpose:** Central hub to manage a specific novel.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{id}` | Load novel detail |
| `POST` | `/api/novels/{id}/submit` | Submit for moderation |
| `DELETE` | `/api/novels/{id}` | Delete novel |

---

### `POST /api/novels/{id}/submit`

**Permission:** Owner / Staff / Admin

**Request body:** None required

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Novel submitted for review",
  "data": {
    "id": 42,
    "status": "Pending",
    "updatedAt": "2024-01-12T10:05:00Z"
  }
}
```

**Error:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Novel must be in Draft status to submit" | Wrong status |

---

## SCR-22 — Volume Management

**Purpose:** Create, reorder, and manage volumes within a novel.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{novelId}/volumes` | Load volume list |
| `POST` | `/api/novels/{novelId}/volumes` | Create volume |
| `PUT` | `/api/volumes/{id}` | Edit volume |
| `DELETE` | `/api/volumes/{id}` | Delete volume (cascade chapters) |

---

### `GET /api/novels/{novelId}/volumes`

**Permission:** Owner / Staff / Admin

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "volumeNumber": 1,
      "title": "Volume 1: The Beginning",
      "chapterCount": 24,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

---

### `POST /api/novels/{novelId}/volumes`

**Permission:** Owner / Staff / Admin

**Request:**
```json
{ "volumeNumber": 2, "title": "Volume 2: The Rising Storm" }
```

**Validation:**

| Field | Rule |
|---|---|
| `volumeNumber` | Required, positive int, unique per novel |
| `title` | Required, 1–200 chars |

**Success — 201 Created:**
```json
{
  "success": true,
  "message": "Volume created successfully",
  "data": { "id": 5, "volumeNumber": 2, "title": "Volume 2: The Rising Storm", "createdAt": "..." }
}
```

**Error:**
```json
{ "success": false, "message": "Volume number already exists in this novel" }
```

---

## SCR-23 — Create Chapter

**Purpose:** Write and save a new chapter.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/volumes/{volumeId}/chapters` | Submit new chapter |
| `POST` | `/api/chapters/{id}/submit` | Submit for review (optional after save) |

---

### `POST /api/volumes/{volumeId}/chapters`

**Permission:** Owner / Staff / Admin

**Request:**
```json
{
  "chapterNumber": 1,
  "title": "Chapter 1: The Awakening",
  "content": "<p>The story begins here...</p>",
  "releaseDate": null
}
```

**Validation:**

| Field | Rule |
|---|---|
| `chapterNumber` | Required, positive int, unique per volume |
| `title` | Required, 1–200 chars |
| `content` | Required, non-empty |
| `releaseDate` | Optional, must be future datetime |

**Success — 201 Created:**
```json
{
  "success": true,
  "message": "Chapter created successfully",
  "data": {
    "id": 87,
    "chapterNumber": 1,
    "title": "Chapter 1: The Awakening",
    "status": "Draft",
    "volumeId": 1,
    "createdAt": "2024-01-12T10:00:00Z"
  }
}
```

---

## SCR-24 — Edit Chapter

**Purpose:** Edit an existing chapter's content.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/chapters/{id}` | Load chapter data |
| `PUT` | `/api/chapters/{id}` | Save changes |

---

### `PUT /api/chapters/{id}`

**Permission:** Owner / Staff / Admin

**Request:**
```json
{
  "chapterNumber": 1,
  "title": "Chapter 1: Updated Title",
  "content": "<p>Updated content.</p>",
  "releaseDate": "2024-02-01T00:00:00Z"
}
```

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Chapter updated successfully",
  "data": {
    "id": 87,
    "title": "Chapter 1: Updated Title",
    "status": "Draft",
    "updatedAt": "2024-01-12T11:00:00Z"
  }
}
```

---

## SCR-25 — Chapter Management

**Purpose:** List and manage all chapters within a volume.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/volumes/{volumeId}/chapters` | Load chapter list |
| `POST` | `/api/chapters/{id}/submit` | Submit chapter for review |
| `DELETE` | `/api/chapters/{id}` | Delete chapter |

---

### `GET /api/volumes/{volumeId}/chapters`

**Permission:** Owner / Staff / Admin

**Query:** `?page=1&size=50&sort=chapterNumber&order=asc`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "chapterNumber": 1,
        "title": "Chapter 1: The Awakening",
        "status": "Published",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "page": 1, "size": 50, "totalElements": 24, "totalPages": 1
  }
}
```

---

### `POST /api/chapters/{id}/submit`

**Permission:** Owner / Staff / Admin

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Chapter submitted for review",
  "data": { "id": 87, "status": "Pending", "updatedAt": "..." }
}
```

---

## SCR-26 — Novel Statistics

**Purpose:** Analytics for the author's novel.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{id}/analytics` | Load analytics data |

---

### `GET /api/novels/{id}/analytics`

**Permission:** Owner / Staff / Admin

**Response:**
```json
{
  "success": true,
  "data": {
    "novelId": 1,
    "viewCount": 128432,
    "likeCount": 3200,
    "favoritesCount": 5800,
    "ratingAverage": 4.2,
    "ratingCount": 1248,
    "commentCount": 342,
    "ratingDistribution": { "1": 12, "2": 34, "3": 98, "4": 456, "5": 648 },
    "topChapters": [
      { "chapterId": 1, "title": "Chapter 1", "commentCount": 89 }
    ]
  }
}
```

---

## SCR-27 — Moderation Status

**Purpose:** Track moderation status of own novels and chapters.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/my?status=Pending` | Pending novels |
| `GET` | `/api/volumes/{novelId}/chapters?status=Pending` | Pending chapters |

---

# MODULE 4 — FORUM MODULE

---

## SCR-28 — Forum Home

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/categories` | Load forum category list |

### `GET /api/forum/categories`

**Permission:** Guest+

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "General Discussion",
      "description": "Talk about anything.",
      "threadCount": 234,
      "latestThread": {
        "id": 99,
        "title": "What's your favorite novel?",
        "createdAt": "2024-01-12T08:00:00Z"
      }
    }
  ]
}
```

---

## SCR-29 — Forum Category Detail

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/threads?categoryId={id}` | Load threads in category |

### `GET /api/forum/threads`

**Permission:** Guest+

**Query:** `?categoryId=1&sort=createdAt&order=desc&page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "What's your favorite fantasy novel?",
        "author": { "id": 5, "username": "user_name", "avatar": "..." },
        "replyCount": 32,
        "viewCount": 512,
        "voteScore": 24,
        "lastReplyAt": "2024-01-12T07:00:00Z",
        "createdAt": "2024-01-10T00:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 89, "totalPages": 5
  }
}
```

---

## SCR-30 — Thread Detail

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/threads/{id}` | Load thread + original post |
| `GET` | `/api/forum/posts?threadId={id}` | Load replies |
| `POST` | `/api/forum/posts` | Post a reply |
| `PUT` | `/api/forum/posts/{id}` | Edit own reply |
| `DELETE` | `/api/forum/posts/{id}` | Delete own reply |
| `POST` | `/api/forum/threads/{id}/vote` | Upvote / downvote thread |
| `POST` | `/api/forum/posts/{id}/vote` | Vote on a reply |
| `POST` | `/api/reports/novels` | Report thread content |

### `POST /api/forum/posts`

**Permission:** User+

**Request:**
```json
{
  "threadId": 1,
  "content": "<p>My reply here.</p>",
  "parentPostId": null
}
```

---

## SCR-31 — Create Thread

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/categories` | Populate category dropdown |
| `POST` | `/api/forum/threads` | Submit new thread |

### `POST /api/forum/threads`

**Permission:** User+

**Request:**
```json
{
  "categoryId": 1,
  "title": "What's your favorite fantasy novel?",
  "content": "<p>Let's discuss!</p>",
  "flairId": null
}
```

**Validation:**

| Field | Rule |
|---|---|
| `categoryId` | Required, valid category ID |
| `title` | Required, 5–200 chars |
| `content` | Required, min 10 chars |

---

## SCR-32 — Edit Thread

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/threads/{id}` | Load thread data |
| `PUT` | `/api/forum/threads/{id}` | Save changes |

### `PUT /api/forum/threads/{id}`

**Permission:** Owner / Staff / Admin

**Request:**
```json
{
  "title": "Updated Thread Title",
  "content": "<p>Updated content.</p>"
}
```

---

## SCR-33 — Edit Post

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `PUT` | `/api/forum/posts/{id}` | Save edited reply |

### `PUT /api/forum/posts/{id}`

**Permission:** Owner / Staff / Admin

**Request:**
```json
{ "content": "<p>Edited reply content.</p>" }
```

---

## SCR-34 — My Threads

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/threads?authorId=me` | Load own threads |
| `DELETE` | `/api/forum/threads/{id}` | Delete own thread |

---

## SCR-35 — Saved Threads

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/forum/saved-threads` | Load saved threads |
| `DELETE` | `/api/forum/saved-threads/{threadId}` | Unsave thread |

---

# MODULE 5 — NOTIFICATIONS

---

## SCR-36 — Notification Center

**Purpose:** View all notifications.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/notifications` | Load notification list |
| `PUT` | `/api/notifications/read-all` | Mark all as read |
| `DELETE` | `/api/notifications/{id}` | Delete notification |

---

### `GET /api/notifications`

**Permission:** User+

**Query:** `?isRead=false&type=NewComment&page=1&size=20`

**Type options:** `NewChapter` | `NewComment` | `CommentReply` | `CommentLike` | `NewFollower` | `BadgeEarned` | `ReportUpdate` | `SystemAlert`

**Response:**
```json
{
  "success": true,
  "data": {
    "unreadCount": 3,
    "items": [
      {
        "id": 1,
        "notificationType": "NewComment",
        "entityType": "Chapter",
        "entityId": 87,
        "message": "john_doe commented on your chapter.",
        "isRead": false,
        "createdAt": "2024-01-12T09:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 14, "totalPages": 1
  }
}
```

---

### `PUT /api/notifications/read-all`

**Permission:** User+

**Response:**
```json
{ "success": true, "message": "All notifications marked as read", "data": null }
```

---

## SCR-37 — Notification Detail

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/notifications/{id}` | Load notification detail |
| `PUT` | `/api/notifications/{id}/read` | Mark as read |
| `DELETE` | `/api/notifications/{id}` | Delete |

### `PUT /api/notifications/{id}/read`

**Permission:** Owner

**Response:**
```json
{ "success": true, "message": "Notification marked as read", "data": null }
```

---

# MODULE 6 — STAFF MODERATION MODULE

---

## SCR-38 — Moderation Dashboard

**Purpose:** Overview with pending counts and quick links.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/staff/dashboard` | Load all moderation stats |

---

### `GET /api/staff/dashboard`

**Permission:** Staff / Admin

**Response:**
```json
{
  "success": true,
  "data": {
    "pendingNovels": 12,
    "pendingChapters": 8,
    "openReports": 23,
    "recentActivity": [
      {
        "action": "ApproveNovel",
        "staff": { "id": 2, "username": "staff_user" },
        "target": "Novel: The Dragon War Chronicles",
        "performedAt": "2024-01-12T09:00:00Z"
      }
    ]
  }
}
```

---

## SCR-39 — Pending Novels

**Purpose:** List novels awaiting review.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/staff/novels/pending` | Load pending novels list |
| `POST` | `/api/staff/novels/{id}/approve` | Quick approve |
| `POST` | `/api/staff/novels/{id}/reject` | Quick reject |

---

### `GET /api/staff/novels/pending`

**Permission:** Staff / Admin

**Query:** `?fromDate=2024-01-01&toDate=2024-01-31&page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 42,
        "title": "The Dragon War Chronicles",
        "coverImage": "...",
        "author": { "id": 5, "username": "author_name" },
        "category": { "id": 3, "name": "Fantasy" },
        "submittedAt": "2024-01-11T14:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 12, "totalPages": 1
  }
}
```

---

### `POST /api/staff/novels/{id}/approve`

**Permission:** Staff / Admin

**Request body:** None

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Novel approved",
  "data": { "id": 42, "status": "Ongoing", "processedAt": "2024-01-12T10:00:00Z" }
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Novel is not in Pending status" | Wrong status |
| 404 | "Novel not found" | Invalid ID |

---

### `POST /api/staff/novels/{id}/reject`

**Permission:** Staff / Admin

**Request:**
```json
{ "reason": "Content violates community guidelines." }
```

**Validation:** `reason` required, 1–1000 chars

---

## SCR-40 — Novel Review Detail

**Purpose:** Full review of a pending novel before approving/rejecting.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{id}` | Load full novel preview |
| `POST` | `/api/staff/novels/{id}/approve` | Approve novel |
| `POST` | `/api/staff/novels/{id}/reject` | Reject with reason |
| `POST` | `/api/staff/novels/{id}/lock` | Lock novel |

---

### `POST /api/staff/novels/{id}/lock`

**Permission:** Staff / Admin

**Request:**
```json
{ "reason": "Under investigation for copyright violation." }
```

---

## SCR-41 — Pending Chapters

**Purpose:** List chapters awaiting review.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/staff/chapters/pending` | Load pending chapters |
| `POST` | `/api/staff/chapters/{id}/approve` | Quick approve |
| `POST` | `/api/staff/chapters/{id}/reject` | Quick reject |

---

### `GET /api/staff/chapters/pending`

**Permission:** Staff / Admin

**Query:** `?page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 87,
        "chapterNumber": 12,
        "title": "Chapter 12",
        "novel": { "id": 1, "title": "The Dragon War Chronicles" },
        "author": { "id": 5, "username": "author_name" },
        "wordCount": 2400,
        "submittedAt": "2024-01-11T15:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 8, "totalPages": 1
  }
}
```

---

## SCR-42 — Chapter Review Detail

**Purpose:** Read full chapter content before approving/rejecting.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/chapters/{id}` | Load chapter content |
| `POST` | `/api/staff/chapters/{id}/approve` | Approve chapter |
| `POST` | `/api/staff/chapters/{id}/reject` | Reject chapter |
| `POST` | `/api/staff/chapters/{id}/lock` | Lock chapter |

---

### `POST /api/staff/chapters/{id}/approve`

**Permission:** Staff / Admin

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Chapter approved",
  "data": { "id": 87, "status": "Published", "processedAt": "2024-01-12T10:05:00Z" }
}
```

---

## SCR-43 — Reports Center

**Purpose:** View and manage all incoming reports.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/staff/reports` | Load reports list (both tabs) |
| `POST` | `/api/staff/reports/{id}/resolve` | Quick resolve |
| `POST` | `/api/staff/reports/{id}/reject-report` | Quick reject report |

---

### `GET /api/staff/reports`

**Permission:** Staff / Admin

**Query:** `?type=novel&status=Pending&page=1&size=20`

**Type options:** `novel` | `user` | omit for all

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 201,
        "reportType": "Inappropriate",
        "targetType": "Novel",
        "targetId": 1,
        "targetTitle": "The Dragon War Chronicles",
        "reporter": { "id": 9, "username": "reporter_user" },
        "status": "Pending",
        "createdAt": "2024-01-12T08:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 23, "totalPages": 2
  }
}
```

---

## SCR-44 — Report Detail

**Purpose:** Full report detail and processing panel.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/staff/reports/{id}` | Load full report detail |
| `POST` | `/api/staff/reports/{id}/resolve` | Resolve report |
| `POST` | `/api/staff/reports/{id}/reject-report` | Reject report |
| `POST` | `/api/admin/users/{id}/ban` | Ban target user (Admin) |

---

### `GET /api/staff/reports/{id}`

**Permission:** Staff / Admin

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 201,
    "reportType": "Inappropriate",
    "description": "Contains explicit content in chapter 5.",
    "status": "Pending",
    "reporter": { "id": 9, "username": "reporter_user" },
    "targetNovel": {
      "id": 1,
      "title": "The Dragon War Chronicles",
      "author": { "id": 5, "username": "author_name" }
    },
    "targetChapter": { "id": 87, "chapterNumber": 5, "title": "Chapter 5" },
    "actionTaken": null,
    "resolutionNotes": null,
    "processedBy": null,
    "createdAt": "2024-01-12T08:00:00Z"
  }
}
```

---

### `POST /api/staff/reports/{id}/resolve`

**Permission:** Staff / Admin

**Request:**
```json
{
  "actionTaken": "Removed offending chapter. Warned author.",
  "resolutionNotes": "Chapter 5 violated Section 3 of community guidelines."
}
```

**Validation:** `actionTaken` required, 1–1000 chars

---

### `POST /api/staff/reports/{id}/reject-report`

**Permission:** Staff / Admin

**Request:**
```json
{ "resolutionNotes": "Content reviewed and found compliant with guidelines." }
```

---

## SCR-45 — User Warning

**Purpose:** Issue a formal warning to a user.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/users/{id}` | Load user info |
| `POST` | `/api/staff/users/{id}/warn` | Issue warning |

---

### `POST /api/staff/users/{id}/warn`

**Permission:** Staff / Admin

**Request:**
```json
{
  "reason": "Repeated posting of off-topic spam comments.",
  "severity": "Minor"
}
```

**Validation:**

| Field | Rule |
|---|---|
| `reason` | Required, 1–1000 chars |
| `severity` | Required: `Minor` or `Major` |

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "Warning issued successfully",
  "data": {
    "userId": 7,
    "reason": "Repeated posting of off-topic spam comments.",
    "severity": "Minor",
    "issuedAt": "2024-01-12T10:00:00Z"
  }
}
```

---

## SCR-46 — Moderation History

**Purpose:** View log of all past moderation actions.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/staff/moderation/history` | Load moderation history |

---

### `GET /api/staff/moderation/history`

**Permission:** Staff / Admin

**Query:** `?staffId=2&actionType=ApproveNovel&fromDate=2024-01-01&toDate=2024-01-31&page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "staff": { "id": 2, "username": "staff_user" },
        "action": "ApproveNovel",
        "targetType": "Novel",
        "targetId": 42,
        "targetTitle": "The Dragon War Chronicles",
        "notes": null,
        "performedAt": "2024-01-12T10:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 145, "totalPages": 8
  }
}
```

---

# MODULE 7 — ADMINISTRATION MODULE

---

## SCR-47 — Admin Dashboard

**Purpose:** System-wide overview for administrators.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/statistics` | Load all platform stats |
| `GET` | `/api/staff/dashboard` | Load moderation stats |

---

### `GET /api/admin/statistics`

**Permission:** Admin

**Response:**
```json
{
  "success": true,
  "data": {
    "users": { "total": 12450, "newThisWeek": 234, "banned": 12 },
    "novels": { "total": 3200, "ongoing": 2100, "pending": 45, "newThisMonth": 120 },
    "chapters": { "total": 48000, "publishedThisWeek": 340 },
    "reports": { "total": 890, "open": 23, "resolvedThisMonth": 67 },
    "engagement": {
      "totalComments": 125000,
      "totalRatings": 45000,
      "totalFavorites": 89000
    }
  }
}
```

---

## SCR-48 — User Management

**Purpose:** Search, filter, and manage all platform users.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/users` | Load users table |
| `PUT` | `/api/admin/users/{id}` | Edit role/status |
| `POST` | `/api/admin/users/{id}/ban` | Ban user |
| `POST` | `/api/admin/users/{id}/unban` | Unban user |
| `DELETE` | `/api/admin/users/{id}` | Delete user |

---

### `GET /api/admin/users`

**Permission:** Admin

**Query:** `?keyword=john&role=User&status=Online&sort=createdAt&order=desc&page=1&size=20`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "username": "john_doe",
        "email": "john@example.com",
        "avatar": "...",
        "role": "User",
        "status": "Online",
        "novelsCount": 3,
        "joinedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "page": 1, "size": 20, "totalElements": 1500, "totalPages": 75
  }
}
```

---

### `POST /api/admin/users/{id}/ban`

**Permission:** Admin

**Request:**
```json
{ "reason": "Repeated violations of community guidelines." }
```

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "User has been banned",
  "data": { "userId": 7, "status": "Banned", "bannedAt": "2024-01-12T10:00:00Z" }
}
```

---

## SCR-49 — User Detail (Admin)

**Purpose:** Detailed user info with admin controls.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/users/{id}` | Load full user detail |
| `PUT` | `/api/admin/users/{id}` | Update role/status |
| `POST` | `/api/admin/users/{id}/ban` | Ban user |
| `POST` | `/api/staff/users/{id}/warn` | Issue warning |

---

### `GET /api/admin/users/{id}`

**Permission:** Admin

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 7,
    "username": "problem_user",
    "email": "user@example.com",
    "avatar": "...",
    "bio": "...",
    "role": "User",
    "status": "Online",
    "reputation": 45,
    "badges": [],
    "stats": {
      "novelsCreated": 1,
      "chaptersPublished": 5,
      "commentsCount": 89,
      "reportsReceived": 3,
      "warningsCount": 1
    },
    "warnings": [
      {
        "reason": "Posting spam comments",
        "severity": "Minor",
        "issuedBy": { "id": 2, "username": "staff_user" },
        "issuedAt": "2024-01-10T09:00:00Z"
      }
    ],
    "joinedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### `PUT /api/admin/users/{id}`

**Permission:** Admin

**Request:**
```json
{ "role": "Staff", "status": "Online" }
```

**Validation:**

| Field | Rule |
|---|---|
| `role` | Optional, valid `UserRole` enum: `User` \| `Staff` \| `Admin` |
| `status` | Optional, valid `UserStatus` enum: `Offline` \| `Online` \| `Banned` |

---

## SCR-50 — Staff Management

**Purpose:** Manage staff accounts.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/users?role=Staff` | Load staff list |
| `POST` | `/api/admin/users/{id}/assign-staff` | Promote to Staff |
| `POST` | `/api/admin/users/{id}/revoke-staff` | Demote to User |

---

### `POST /api/admin/users/{id}/assign-staff`

**Permission:** Admin

**Request body:** None

**Success — 200 OK:**
```json
{
  "success": true,
  "message": "User promoted to Staff",
  "data": { "userId": 5, "role": "Staff", "updatedAt": "2024-01-12T10:00:00Z" }
}
```

---

## SCR-51 — Badge Management

**Purpose:** Create and manage user achievement badges.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/badges` | Load badge list |
| `POST` | `/api/admin/badges` | Create badge |
| `PUT` | `/api/admin/badges/{id}` | Edit badge |
| `DELETE` | `/api/admin/badges/{id}` | Delete badge |
| `POST` | `/api/admin/badges/{id}/award/{userId}` | Award to user |

---

### `POST /api/admin/badges`

**Permission:** Admin

**Request:**
```json
{
  "key": "first_novel",
  "name": "First Novel",
  "description": "Published your first novel.",
  "icon": "https://cdn.example.com/badges/first_novel.svg",
  "color": "#FFD700"
}
```

**Validation:**

| Field | Rule |
|---|---|
| `key` | Required, 1–100 chars, `^[a-z0-9_]+$`, unique |
| `name` | Required, 1–100 chars |
| `description` | Required, 1–500 chars |
| `icon` | Optional, valid URL |
| `color` | Optional, valid hex color |

---

## SCR-52 — Novel Categories

**Purpose:** Manage novel categories.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/categories` | Load categories |
| `POST` | `/api/admin/categories` | Create category |
| `PUT` | `/api/admin/categories/{id}` | Edit category |
| `DELETE` | `/api/admin/categories/{id}` | Delete category |

---

### `POST /api/admin/categories`

**Permission:** Admin

**Request:**
```json
{ "name": "Science Fiction", "slug": "science-fiction" }
```

**Validation:**

| Field | Rule |
|---|---|
| `name` | Required, 1–100 chars, unique |
| `slug` | Required, 1–120 chars, URL-safe, unique |

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 409 | "Category name already exists" | Duplicate name |
| 409 | "Slug already exists" | Duplicate slug |

---

## SCR-53 — Tag Management

**Purpose:** Manage novel tags.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/tags` | Load tags |
| `POST` | `/api/admin/tags` | Create tag |
| `PUT` | `/api/admin/tags/{id}` | Edit tag |
| `DELETE` | `/api/admin/tags/{id}` | Delete tag |

---

### `POST /api/admin/tags`

**Permission:** Admin

**Request:**
```json
{ "name": "Isekai", "slug": "isekai" }
```

**Validation:** Same as category.

---

## SCR-54 — Forum Categories

**Purpose:** Manage forum categories.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/forum-categories` | Load list |
| `POST` | `/api/admin/forum-categories` | Create |
| `PUT` | `/api/admin/forum-categories/{id}` | Edit |
| `DELETE` | `/api/admin/forum-categories/{id}` | Delete |

---

### `POST /api/admin/forum-categories`

**Permission:** Admin

**Request:**
```json
{
  "name": "General Discussion",
  "description": "Talk about anything related to LitNovel.",
  "slug": "general-discussion"
}
```

---

## SCR-55 — Forum Flairs

**Purpose:** Manage thread flairs/labels per forum category.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/forum-flairs?categoryId={id}` | Load flairs |
| `POST` | `/api/admin/forum-flairs` | Create flair |
| `PUT` | `/api/admin/forum-flairs/{id}` | Edit flair |
| `DELETE` | `/api/admin/forum-flairs/{id}` | Delete flair |

---

### `POST /api/admin/forum-flairs`

**Permission:** Admin

**Request:**
```json
{ "categoryId": 1, "name": "Question", "color": "#3B82F6" }
```

---

## SCR-56 — Notification Management

**Purpose:** Send system notifications to users.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/notifications/sent` | Load sent notifications history |
| `POST` | `/api/admin/notifications` | Send notification |

---

### `POST /api/admin/notifications`

**Permission:** Admin

**Request:**
```json
{
  "notificationType": "SystemAlert",
  "message": "The platform will undergo maintenance on Jan 20th.",
  "targetAll": true,
  "targetUserId": null
}
```

**Validation:**

| Field | Rule |
|---|---|
| `notificationType` | Required, valid `NotificationType` enum |
| `message` | Required, 1–1000 chars |
| `targetAll` | Required boolean |
| `targetUserId` | Required if `targetAll = false`, valid user ID |

**Success — 201 Created:**
```json
{
  "success": true,
  "message": "Notification sent to all users",
  "data": { "sentCount": 12450, "sentAt": "2024-01-12T10:00:00Z" }
}
```

---

## SCR-57 — Reports Overview (Admin)

**Purpose:** High-level view of all platform reports.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/reports` | Load all reports |
| `GET` | `/api/admin/reports?type=novel` | Novel reports tab |
| `GET` | `/api/admin/reports?type=user` | User reports tab |

---

### `GET /api/admin/reports`

**Permission:** Admin

**Query:** `?type=novel&status=Pending&fromDate=2024-01-01&page=1&size=20`

**Response:** Same format as `GET /api/staff/reports` with additional `processedById` filter support.

---

## SCR-58 — Audit Logs

**Purpose:** View system audit logs of all significant actions.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/audit-logs` | Load audit log |

---

### `GET /api/admin/audit-logs`

**Permission:** Admin

**Query:** `?actorId=2&entityType=Novel&fromDate=2024-01-01&toDate=2024-01-31&page=1&size=50`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "actor": { "id": 2, "username": "admin_user" },
        "action": "ApproveNovel",
        "entityType": "Novel",
        "entityId": 42,
        "ipAddress": "192.168.1.1",
        "createdAt": "2024-01-12T10:00:00Z"
      }
    ],
    "page": 1, "size": 50, "totalElements": 4200, "totalPages": 84
  }
}
```

---

## SCR-59 — Statistics Dashboard

**Purpose:** Comprehensive platform analytics.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/statistics` | Load statistics |
| `GET` | `/api/admin/statistics/chart?metric=userGrowth&from=...&to=...` | Time-series chart data |

---

### `GET /api/admin/statistics/chart`

**Permission:** Admin

**Query:** `?metric=userGrowth&from=2024-01-01&to=2024-01-31&granularity=day`

**Response:**
```json
{
  "success": true,
  "data": {
    "metric": "userGrowth",
    "points": [
      { "date": "2024-01-01", "value": 45 },
      { "date": "2024-01-02", "value": 38 }
    ]
  }
}
```

---

## SCR-60 — Novel Override

**Purpose:** Force-manage any novel regardless of ownership.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/novels/{id}` | Load any novel |
| `PUT` | `/api/admin/novels/{id}/status` | Force-change status |
| `PUT` | `/api/admin/novels/{id}/author` | Force-change author |
| `DELETE` | `/api/novels/{id}` | Force delete |

---

### `PUT /api/admin/novels/{id}/status`

**Permission:** Admin

**Request:**
```json
{ "status": "Ended", "reason": "Author account deleted." }
```

**Valid status values:** `Ongoing` | `Ended` | `Hiatus` | `Dropped` | `Canceled`

---

## SCR-61 — Chapter Override

**Purpose:** Force-manage any chapter regardless of ownership.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/chapters/{id}` | Load any chapter |
| `PUT` | `/api/chapters/{id}` | Force-edit content |
| `PUT` | `/api/admin/chapters/{id}/status` | Force-change status |
| `DELETE` | `/api/chapters/{id}` | Force delete |

---

## SCR-62 — System Settings

**Purpose:** Configure platform-wide settings.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/settings` | Load settings |
| `PUT` | `/api/admin/settings` | Save settings |

---

### `GET /api/admin/settings`

**Permission:** Admin

**Response:**
```json
{
  "success": true,
  "data": {
    "general": {
      "siteName": "LitNovel",
      "tagline": "Read, Write, Discover",
      "maintenanceMode": false
    },
    "content": {
      "maxNovelDescriptionLength": 5000,
      "maxChapterLength": 50000,
      "maxTagsPerNovel": 10
    },
    "moderation": {
      "reviewSLAHours": 48,
      "autoFlagKeywords": ["keyword1", "keyword2"]
    }
  }
}
```

---

## SCR-63 — Announcement Management

**Purpose:** Manage homepage banners and announcements.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/announcements` | Load announcements list |
| `POST` | `/api/admin/announcements` | Create announcement |
| `PUT` | `/api/admin/announcements/{id}` | Edit announcement |
| `DELETE` | `/api/admin/announcements/{id}` | Delete announcement |
| `PUT` | `/api/admin/announcements/{id}/toggle` | Toggle active/inactive |

---

### `POST /api/admin/announcements`

**Permission:** Admin

**Request:**
```json
{
  "title": "Scheduled Maintenance Notice",
  "content": "<p>The platform will be down for maintenance on Jan 20th from 2–4 AM UTC.</p>",
  "startDate": "2024-01-18T00:00:00Z",
  "endDate": "2024-01-21T00:00:00Z",
  "isActive": true
}
```

**Validation:**

| Field | Rule |
|---|---|
| `title` | Required, max 200 chars |
| `content` | Required, max 2000 chars |
| `startDate` | Required, valid datetime |
| `endDate` | Optional, must be after `startDate` |
| `isActive` | Required boolean |

**Success — 201 Created:**
```json
{
  "success": true,
  "message": "Announcement created",
  "data": {
    "id": 5,
    "title": "Scheduled Maintenance Notice",
    "isActive": true,
    "startDate": "2024-01-18T00:00:00Z",
    "createdAt": "2024-01-12T10:00:00Z"
  }
}
```

---

## SCR-64 — Backup & Restore

**Purpose:** Manage system data backups.

### APIs Used

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/backups` | Load backup history |
| `POST` | `/api/admin/backups` | Trigger new backup |
| `GET` | `/api/admin/backups/{id}/download` | Download backup file |
| `POST` | `/api/admin/backups/{id}/restore` | Restore from backup |
| `DELETE` | `/api/admin/backups/{id}` | Delete backup |

---

### `GET /api/admin/backups`

**Permission:** Admin

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "backup_20240112_100000",
      "sizeBytes": 524288000,
      "sizeFormatted": "500 MB",
      "status": "Completed",
      "createdAt": "2024-01-12T10:00:00Z",
      "downloadUrl": "/api/admin/backups/backup_20240112_100000/download"
    }
  ]
}
```

---

### `POST /api/admin/backups`

**Permission:** Admin

**Request body:** None

**Success — 202 Accepted:**
```json
{
  "success": true,
  "message": "Backup job started",
  "data": {
    "jobId": "backup_20240112_110000",
    "status": "InProgress",
    "startedAt": "2024-01-12T11:00:00Z"
  }
}
```

---

### `POST /api/admin/backups/{id}/restore`

**Permission:** Admin

**Request:**
```json
{ "confirmationText": "RESTORE" }
```

**Validation:** `confirmationText` must equal `"RESTORE"` exactly.

**Success — 202 Accepted:**
```json
{
  "success": true,
  "message": "Restore job started. System will be unavailable during restore.",
  "data": {
    "jobId": "restore_20240112_110000",
    "status": "InProgress"
  }
}
```

**Errors:**

| Status | Message | Cause |
|---|---|---|
| 400 | "Confirmation text must be 'RESTORE'" | Wrong confirmation |
| 404 | "Backup not found" | Invalid backup ID |

---

# Appendix — Global Shared Endpoints (All Screens)

The following endpoints are used across multiple screens:

| Method | Endpoint | Used In |
|---|---|---|
| `POST` | `/api/auth/refresh` | All screens (token auto-refresh) |
| `GET` | `/api/notifications?isRead=false&size=1` | Header bell badge (all auth screens) |
| `GET` | `/api/categories` | SCR-01, SCR-10, SCR-19, SCR-20 |
| `GET` | `/api/tags` | SCR-10, SCR-19, SCR-20 |

---

*LitNovel API Specification by Screen — v1.0*  
*Covers: 64 screens across 7 modules*  
*Generated from: `screen.md`, `api-spec.md`, `spec.md`*
