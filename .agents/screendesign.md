# LitNovel — Screen Design Specification

> Version: 1.0  
> Design System: Vercel-Inspired (see `UI.md`)  
> Coverage: SCR-01 → SCR-64 (64 screens)

---

## Design System Summary

| Token | Value |
|---|---|
| Primary | `#171717` (ink black) |
| Canvas | `#ffffff` |
| Canvas-Soft | `#fafafa` |
| Body text | `#4d4d4d` |
| Muted text | `#888888` |
| Hairline | `#ebebeb` |
| Link | `#0070f3` |
| Error | `#ee0000` |
| Radius (card) | `8px` |
| Radius (button) | `100px` pill |
| Font (display) | Geist / Inter 600 |
| Font (body) | Geist / Inter 400 |
| Font (mono) | Geist Mono |

---

# MODULE 1 — AUTHENTICATION & PROFILE

---

# SCR-01 – Landing Page

## Purpose
Introduce the platform to visitors, showcase featured novels, and provide entry points for registration and login.

## Related Use Cases
- UC-U01 (Register), UC-U02 (Login), UC-U05 (Browse Novels)

## Accessible By
Guest, User, Staff, Admin

## Layout Structure

```
Header
├─ Logo (LitNovel)
├─ Navigation Links (Browse, Rankings, Forum)
├─ CTA Buttons: [Login] [Sign Up]

Hero Section
├─ Headline + Subheadline
├─ Search Bar (full-width)
├─ CTA: [Start Reading] [Become an Author]

Featured Novels Section
├─ Section Title "Trending Now"
├─ Novel Card Grid (3-up desktop, 2-up tablet, 1-up mobile)

New Releases Section
├─ Novel Card Grid (3-up)

Categories Strip
├─ Horizontal scroll of category pills

Footer
├─ Logo + tagline
├─ Navigation links
├─ Copyright
```

## Sections

### Hero Section
**Purpose:** Capture visitor attention and drive to registration or browsing.  
**Components:** Headline (`display-xl`), Sub-headline (`body-lg`), Search Bar (`form-input-lg`), two `button-primary` + `button-secondary` pills.  
**Data Source:** Static content.  
**Actions:** Search → SCR-11, Start Reading → SCR-10, Become an Author → SCR-02.

### Featured Novels Grid
**Purpose:** Showcase trending/featured novels.  
**Components:** `card-marketing` (Novel Card) × 6, Section heading (`display-lg`).  
**Data Source:** `Novels` (top ViewCount, Status = Ongoing/Ended).  
**Actions:** Click card → SCR-12.

### Category Pills Strip
**Purpose:** Quick navigation to category-filtered catalog.  
**Components:** `badge-secondary` pills, horizontal scroll.  
**Data Source:** `Categories` table.  
**Actions:** Click pill → SCR-10 (pre-filtered).

## UI Components
- NavBar, Hero Band, Search Bar, Novel Card, Category Pill, Footer, Button Primary/Secondary

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Search Query | Text Input | No | — | Max 200 chars |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Search | Submit | Guest | Navigate to SCR-11 |
| Login | Button | Guest | Navigate to SCR-03 |
| Sign Up | Button | Guest | Navigate to SCR-02 |
| Click Novel Card | Link | Guest | Navigate to SCR-12 |
| Click Category | Pill Button | Guest | Navigate to SCR-10 (filtered) |

## States

### Loading State
Skeleton cards (3 per row) for Featured and New Releases sections.

### Empty State
Not applicable (static hero always visible).

### Error State
Toast: "Unable to load novels. Please try again."

## Responsive Design

### Desktop
Full layout. Hero full-width. 3-column novel grid. Category strip visible.

### Tablet
Hero stacked. 2-column grid. Nav collapses to hamburger.

### Mobile
1-column grid. Hero stacked, search bar full-width. Hamburger menu.

## Accessibility
- Hero headline `<h1>`. All novel cards have `alt` for cover images.
- Search bar: `aria-label="Search novels"`.
- Nav links keyboard navigable.

---

# SCR-02 – Register

## Purpose
Allow guests to create a new LitNovel account.

## Related Use Cases
- UC-U01 (Register Account)

## Accessible By
Guest only (redirect to SCR-09 if already logged in)

## Layout Structure

```
Page (centered card, canvas-soft background)
├─ Logo
├─ Title: "Create your account"
├─ Registration Form
│   ├─ Username Field
│   ├─ Email Field
│   ├─ Password Field
│   ├─ Confirm Password Field
│   └─ Submit Button
├─ Link: "Already have an account? Login"
└─ Footer
```

## Sections

### Registration Form
**Purpose:** Collect new user credentials.  
**Components:** `ex-auth-form-card`, `form-input` × 4, `button-primary`.  
**Data Source:** Writes to `Users`.  
**Actions:** Submit → create account → redirect to SCR-09.

## UI Components
- AuthFormCard, Input, Button Primary, Link Inline, Logo

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Username | Text Input | Yes | `User.Username` | 3–50 chars, alphanumeric + underscore, unique |
| Email | Email Input | Yes | `User.Email` | Valid email format, max 256 chars, unique |
| Password | Password Input | Yes | `User.PasswordHash` | Min 8 chars, 1 uppercase, 1 number |
| Confirm Password | Password Input | Yes | — | Must match Password |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Register | Primary Button | Guest | Create account → redirect to SCR-09 |
| Go to Login | Link | Guest | Navigate to SCR-03 |

## States

### Loading State
Button shows spinner, fields disabled.

### Success State
Toast: "Account created! Welcome to LitNovel." → Redirect SCR-09.

### Error State
- Inline field errors (red border + message below field).
- Toast for server error: "Registration failed. Please try again."

### Permission State
Logged-in users redirected to SCR-09.

## Responsive Design

### Desktop / Tablet
Centered card `max-width: 480px`, `rounded-lg`, `padding-xl`.

### Mobile
Full-width card, `padding-md`. Fields stack vertically.

## Accessibility
- `<h1>` "Create your account". ARIA labels on all inputs. Error messages linked via `aria-describedby`.

---

# SCR-03 – Login

## Purpose
Authenticate existing users and issue JWT token.

## Related Use Cases
- UC-U02 (Login)

## Accessible By
Guest only

## Layout Structure

```
Page (centered card)
├─ Logo
├─ Title: "Sign in to LitNovel"
├─ Login Form
│   ├─ Email/Username Field
│   ├─ Password Field
│   ├─ Forgot Password Link
│   └─ Submit Button
├─ Link: "Don't have an account? Sign up"
└─ Footer
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Email or Username | Text Input | Yes | `User.Email` / `User.Username` | Non-empty |
| Password | Password Input | Yes | `User.PasswordHash` | Non-empty |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Login | Primary Button | Guest | Authenticate → redirect to SCR-09 |
| Forgot Password | Link | Guest | Navigate to SCR-04 |
| Sign Up | Link | Guest | Navigate to SCR-02 |

## States

### Loading State
Button spinner, fields disabled.

### Error State
Alert banner: "Invalid email or password." (top of form).

### Permission State
If banned: Alert "Your account has been banned. Contact support."

## Responsive Design
Same as SCR-02 — centered card, full-width on mobile.

## Accessibility
- `<h1>` "Sign in". Password field `type="password"`. Toggle show/hide password with `aria-label`.

---

# SCR-04 – Forgot Password

## Purpose
Allow users to request a password reset email.

## Related Use Cases
- Forgot Password flow

## Accessible By
Guest

## Layout Structure

```
Page (centered card)
├─ Title: "Reset your password"
├─ Description text
├─ Email Field
├─ Submit Button
└─ Link: "Back to Login"
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Email | Email Input | Yes | `User.Email` | Valid email format |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Send Reset Email | Primary Button | Guest | Send reset link → success message |
| Back to Login | Link | Guest | Navigate to SCR-03 |

## States

### Success State
Alert: "If this email is registered, you'll receive a reset link shortly."

### Error State
Inline error if email format invalid.

## Responsive Design
Centered card, full-width mobile.

---

# SCR-05 – Reset Password

## Purpose
Allow users to set a new password via reset token link.

## Related Use Cases
- Reset Password flow

## Accessible By
Guest (via email token link)

## Layout Structure

```
Page (centered card)
├─ Title: "Create new password"
├─ New Password Field
├─ Confirm Password Field
├─ Submit Button
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| New Password | Password Input | Yes | `User.PasswordHash` | Min 8 chars, 1 uppercase, 1 number |
| Confirm Password | Password Input | Yes | — | Must match |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Reset Password | Primary Button | Guest (token) | Update password → redirect SCR-03 |

## States

### Error State
- Invalid/expired token: Alert "Reset link has expired. Please request again." + link to SCR-04.
- Password mismatch: Inline error.

---

# SCR-06 – User Profile (Edit)

## Purpose
Allow authenticated users to view and update their own profile information.

## Related Use Cases
- UC-U04 (Update Profile)

## Accessible By
User, Staff, Admin (own profile)

## Layout Structure

```
Page (two-column: sidebar + main)
├─ Sidebar: Avatar, Display Name, Member since
└─ Main Content
    ├─ Tab: Profile Info
    ├─ Tab: Account Settings
    └─ Tab: Reading Stats
```

## Sections

### Profile Info Tab
**Components:** Avatar upload, Username display, Bio textarea, Save button.  
**Data Source:** `User`.

### Account Settings Tab
**Components:** Email display (read-only), Change Password link → SCR-08.

### Reading Stats Tab
**Components:** Stats cards (Total read, Favorites count, Comments count).

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Avatar | Image Upload | No | `User.Avatar` | JPG, PNG, WEBP, max 2MB |
| Bio | Textarea | No | `User.Bio` | Max 1000 chars |
| Username | Text | Read-only | `User.Username` | — |
| Email | Text | Read-only | `User.Email` | — |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Save Profile | Primary Button | Owner | Update User record → success toast |
| Upload Avatar | File Upload | Owner | Update `User.Avatar` |
| Change Password | Secondary Button | Owner | Navigate to SCR-08 |

## States

### Success State
Toast: "Profile updated successfully."

### Error State
Inline validation errors.

## Responsive Design

### Desktop
Two-column sidebar + main content.

### Tablet / Mobile
Single column, sidebar content moves to top.

---

# SCR-07 – Public Profile

## Purpose
Display a user's public profile including their published novels and stats.

## Related Use Cases
- View Public Profile

## Accessible By
Guest, User, Staff, Admin

## Layout Structure

```
Page
├─ Profile Header (Avatar, Username, Bio, Join Date, Stats)
├─ Published Novels Grid
└─ User Badges Section
```

## Sections

### Profile Header
**Components:** Avatar (`rounded-full`), Username (`display-md`), Bio (`body-md`), badge strip.  
**Data Source:** `User`, `UserReputation`, `UserBadges`.

### Published Novels Grid
**Components:** Novel Card × N, Pagination.  
**Data Source:** `Novels` WHERE `AuthorId = userId AND Status IN (Ongoing, Ended)`.

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Click Novel Card | Link | Guest | Navigate to SCR-12 |
| Report User | Danger Button | User | Open Report Modal |

## Responsive Design
Single column on mobile, two-column on desktop.

---

# SCR-08 – Change Password

## Purpose
Allow users to update their account password.

## Accessible By
User, Staff, Admin (own account)

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Current Password | Password Input | Yes | — | Must match stored hash |
| New Password | Password Input | Yes | `User.PasswordHash` | Min 8 chars |
| Confirm New Password | Password Input | Yes | — | Must match New Password |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Change Password | Primary Button | Owner | Update hash → success toast |
| Cancel | Secondary Button | Owner | Return to SCR-06 |

---

# MODULE 2 — READING MODULE

---

# SCR-09 – Home Page (Authenticated)

## Purpose
Personalized homepage with reading progress, recommendations, and new chapters from followed novels.

## Related Use Cases
- UC-U05 (Browse Novels), UC-U09 (Reading History)

## Accessible By
User, Staff, Admin

## Layout Structure

```
Header (sticky)
├─ Logo, Search Bar, Notification Bell, User Avatar Menu

Main Content
├─ Continue Reading Section (ReadingProgress cards)
├─ New Chapters (from followed/favorited novels)
├─ Trending This Week
├─ Recommended For You
├─ Category Quick Access

Footer
```

## Sections

### Continue Reading
**Purpose:** Resume last-read novels.  
**Components:** `card-soft` with novel cover, progress bar (`ProgressPercentage`), chapter title.  
**Data Source:** `ReadingProgresses` WHERE `UserId = currentUser` ORDER BY `LastReadAt DESC` LIMIT 4.

### Trending This Week
**Components:** Ranked novel list with rank number badge, cover, title, view delta.  
**Data Source:** `Novels` ORDER BY `ViewCount DESC` LIMIT 10.

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Continue Reading | Card Click | User | Navigate to SCR-13 (last chapter) |
| Click Novel | Card Click | User | Navigate to SCR-12 |
| Notification Bell | Icon Button | User | Navigate to SCR-36 |

## Responsive Design
3-column grid → 2-column → 1-column. Continue reading horizontal scroll on mobile.

---

# SCR-10 – Novel Catalog

## Purpose
Browse all published novels with filters and sorting options.

## Related Use Cases
- UC-U05 (Browse Novels)

## Accessible By
Guest, User, Staff, Admin

## Layout Structure

```
Page
├─ Page Title: "Browse Novels"
├─ Filter Sidebar (Desktop) / Filter Drawer (Mobile)
│   ├─ Category filter (checkboxes)
│   ├─ Tags filter (multi-select pills)
│   ├─ Status filter (Ongoing / Ended / Hiatus)
│   └─ Sort By (Views / Rating / Latest Update)
├─ Results Area
│   ├─ Results count + Sort dropdown (mobile)
│   ├─ Novel Grid (4-up desktop, 2-up tablet, 1-up mobile)
│   └─ Pagination
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Category | Checkbox | No | `Novel.CategoryId` → `Category` | — |
| Tags | Multi-select | No | `NovelTags` → `Tag` | — |
| Status | Radio | No | `Novel.Status` | — |
| Sort By | Dropdown | No | — | Views / Rating / UpdatedAt |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Apply Filters | Button | Guest | Reload grid with filters |
| Clear Filters | Link | Guest | Reset all filters |
| Click Novel Card | Link | Guest | Navigate to SCR-12 |
| Page Change | Pagination | Guest | Load next/prev page |

## Table Design

### Columns (list view alternative)
| Column | Sortable | Filterable |
|---|---|---|
| Cover + Title | No | No |
| Category | No | Yes |
| Status | No | Yes |
| Views | Yes | No |
| Rating | Yes | No |
| Last Updated | Yes | No |

## States

### Loading State
Skeleton cards (4 × 2 rows).

### Empty State
Illustration + "No novels found matching your filters." + [Clear Filters] button.

---

# SCR-11 – Search Results

## Purpose
Display novels (and optionally users/chapters) matching a search query.

## Related Use Cases
- UC-U06 (Search Novels)

## Accessible By
Guest, User, Staff, Admin

## Layout Structure

```
Page
├─ Search Bar (pre-filled with query)
├─ Result Tabs: [Novels] [Authors]
├─ Result Grid / List
└─ Pagination
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Search Query | Text Input | Yes | — | Max 200 chars |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Re-search | Submit | Guest | Reload results |
| Click Novel | Link | Guest | SCR-12 |
| Click Author | Link | Guest | SCR-07 |

## States

### Empty State
"No results found for '{query}'. Try different keywords."

---

# SCR-12 – Novel Detail

## Purpose
Display full novel information: synopsis, volumes, chapters list, ratings, reviews, and comments.

## Related Use Cases
- UC-U07 (View Novel), UC-U10 (Favorite), UC-U11 (Like), UC-U12 (Rate), UC-U13 (Comment), UC-U15 (Report)

## Accessible By
Guest, User, Staff, Admin

## Layout Structure

```
Page (two-column: main + sidebar)
├─ Novel Header
│   ├─ Cover Image
│   ├─ Title, Author, Category, Tags, Status
│   ├─ Stats: Views, Likes, Rating (stars)
│   └─ Action Buttons: [Read] [Favorite ♡] [Like 👍] [Report]
├─ Tabs: [Overview] [Chapters] [Reviews] [Comments]
├─ Overview Tab
│   └─ Description / Synopsis
├─ Chapters Tab
│   └─ Volume accordion → Chapter list
├─ Reviews Tab
│   └─ Rating Summary + Review Cards
├─ Comments Tab
│   └─ Comment input + comment thread
└─ Sidebar
    ├─ Same-category novels
    └─ Author info card
```

## Sections

### Novel Header
**Data Source:** `Novel`, `User (Author)`, `Category`, `NovelTags`, aggregate `NovelRatings`.

### Chapters Tab
**Components:** Volume accordion (`VolumeNumber`, `Title`), Chapter rows (`ChapterNumber`, `Title`, `Status`, `CreatedAt`).  
**Data Source:** `Volumes`, `Chapters`.

### Reviews Tab
**Components:** Star rating summary bar, `NovelRating` cards (User avatar, rating stars, review text, date).  
**Data Source:** `NovelRatings`.

### Comments Tab
**Components:** Comment box (`form-input` large), nested `CommentChapter` threads.  
**Data Source:** `CommentChapters`.

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Rating | Star select (1–5) | Yes | `NovelRating.Rating` | 1–5, once per user |
| Review Text | Textarea | No | `NovelRating.Review` | Max 3000 chars |
| Comment | Textarea | Yes | `CommentChapter.Content` | Max 2000 chars |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Read First Chapter | Primary Button | Guest | SCR-13 (chapter 1) |
| Continue Reading | Primary Button | User | SCR-13 (last progress) |
| Favorite | Icon Button (toggle) | User | Toggle `Favorites` record |
| Like | Icon Button (toggle) | User | Increment `Novel.LikeCount` |
| Submit Rating | Submit | User | Create `NovelRating` |
| Submit Comment | Submit | User | Create `CommentChapter` |
| Report Novel | Danger Link | User | Open Report Modal |

## Responsive Design

### Desktop
Two-column layout (main 70%, sidebar 30%).

### Tablet / Mobile
Single column, sidebar below main.

---

# SCR-13 – Chapter Reader

## Purpose
Display chapter content for reading. Allow navigation between chapters and commenting.

## Related Use Cases
- UC-U08 (Read Chapter), UC-U14 (Comment on Chapter)

## Accessible By
Guest (public chapters), User (with progress tracking)

## Layout Structure

```
Page
├─ Top Navigation Bar
│   ├─ Back to Novel link
│   ├─ Chapter Title
│   └─ Settings (font size, theme)
├─ Chapter Content Area (centered, max-width reading column)
├─ Chapter Navigation Footer
│   ├─ [← Previous Chapter] [Next Chapter →]
│   └─ Chapter progress indicator
├─ Comments Section (below content)
```

## Sections

### Content Area
**Components:** `body-lg` typography, wide margins, `canvas` background, reading-optimized line-height.  
**Data Source:** `ChapterContent.Content`.

### Comments Section
**Components:** Comment input box, nested `CommentChapter` thread.  
**Data Source:** `CommentChapters` WHERE `ChapterId = currentChapter`.

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Previous Chapter | Ghost Button | Guest | Load prev chapter |
| Next Chapter | Ghost Button | Guest | Load next chapter |
| Submit Comment | Submit | User | Create `CommentChapter` |
| Like Comment | Icon Button | User | Increment `LikeCount` |
| Reply Comment | Text Link | User | Expand reply input |
| Report Comment | Icon | User | Open Report Modal |

## States

### Loading State
Skeleton content block (paragraph lines).

### Empty State (No Content)
"This chapter has no content yet."

## Responsive Design

### Desktop
Reading column max-width 720px, centered, generous top/bottom padding.

### Mobile
Full-width, larger font size (18px body), swipe gesture for prev/next chapter.

## Accessibility
- Font size adjustment via `aria-label="Increase font size"` controls.
- Chapter content in `<article>` landmark.

---

# SCR-14 – Reading History

## Purpose
Show all novels the user has previously read with their progress.

## Related Use Cases
- UC-U09 (View Reading History)

## Accessible By
User, Staff, Admin

## Layout Structure

```
Page
├─ Title: "Reading History"
├─ Filter: [All] [In Progress] [Completed]
├─ Novel History List
│   └─ Row: Cover, Title, Last Chapter, Progress Bar, Last Read Date, [Continue]
└─ Pagination
```

## Fields (display only)

| Field | Source Entity |
|---|---|
| Novel Cover | `Novel.CoverImage` |
| Novel Title | `Novel.Title` |
| Last Chapter | `ReadingProgress.ChapterId` → `Chapter.Title` |
| Progress % | `ReadingProgress.ProgressPercentage` |
| Last Read | `ReadingProgress.LastReadAt` |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Continue | Primary Button | User | SCR-13 (last chapter) |
| Remove from History | Icon Button | User | Delete `ReadingProgress` record |

---

# SCR-15 – Favorites List

## Purpose
Browse all novels the user has favorited/followed.

## Related Use Cases
- UC-U10 (Add to Favorites), UC-U11 (Remove from Favorites)

## Accessible By
User

## Layout Structure

```
Page
├─ Title: "My Favorites"
├─ Sort: Latest / Title / Rating
├─ Novel Grid (3-up desktop)
└─ Pagination
```

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Click Novel | Link | User | SCR-12 |
| Remove Favorite | Icon Button (♡ toggle) | User | Delete `Favorites` record |

---

# SCR-16 – Liked Novels

## Purpose
Display novels the user has liked.

## Accessible By
User

## Layout Structure
Same structure as SCR-15 but sourced from `Novel.LikeCount` user associations.

---

# SCR-17 – Bookmarks

## Purpose
View bookmarked chapters for quick access.

## Accessible By
User

## Layout Structure

```
Page
├─ Title: "Bookmarks"
├─ Bookmark List
│   └─ Row: Novel Cover, Novel Title, Chapter Title, Bookmarked At, [Read]
└─ Pagination
```

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Read | Button | User | SCR-13 |
| Remove Bookmark | Icon | User | Delete bookmark record |

---

# MODULE 3 — PUBLISHING MODULE

---

# SCR-18 – My Novels Dashboard

## Purpose
Overview dashboard for authors showing all their novels and quick stats.

## Related Use Cases
- UC-U29 (View My Novels), UC-U30 (View Moderation Status)

## Accessible By
User, Staff, Admin

## Layout Structure

```
Page
├─ Header: "My Novels" + [Create Novel] button
├─ Stats Row: [Total Novels] [Total Chapters] [Total Views] [Total Ratings]
├─ Filter Tabs: [All] [Draft] [Pending] [Ongoing] [Ended]
├─ Novel Table / Card Grid
└─ Pagination
```

## Sections

### Novel Table
**Columns:**

| Column | Sortable | Filterable |
|---|---|---|
| Cover + Title | No | No |
| Status | No | Yes |
| Chapters | No | No |
| Views | Yes | No |
| Rating | Yes | No |
| Updated | Yes | No |
| Actions | No | No |

**Row Actions:** [View] [Edit] [Manage Volumes/Chapters] [Delete]

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Create Novel | Primary Button | User | SCR-19 |
| Edit | Icon Button | Owner | SCR-20 |
| Manage | Icon Button | Owner | SCR-21 |
| Delete | Danger Icon | Owner | Confirm modal → delete |

---

# SCR-19 – Create Novel

## Purpose
Form to create a new novel draft.

## Related Use Cases
- UC-U20 (Create Novel)

## Accessible By
User, Staff, Admin

## Layout Structure

```
Page (single-column form card)
├─ Title: "Create New Novel"
├─ Form
│   ├─ Title
│   ├─ Cover Image Upload
│   ├─ Description (Rich Text Editor)
│   ├─ Category (Dropdown)
│   ├─ Tags (Multi-select)
│   └─ Action Buttons: [Save as Draft] [Submit for Review]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Title | Text Input | Yes | `Novel.Title` | Max 200 chars |
| Cover Image | Image Upload | No | `Novel.CoverImage` | JPG/PNG/WEBP, max 5MB |
| Description | Rich Text | No | `Novel.Description` | Max 5000 chars |
| Category | Dropdown | No | `Novel.CategoryId` | Valid category ID |
| Tags | Multi-select | No | `NovelTags` | Max 10 tags |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Save as Draft | Secondary Button | User | Create Novel (Status=Pending → Draft) |
| Submit for Review | Primary Button | User | Create + set Status=Pending |
| Cancel | Link | User | SCR-18 |

---

# SCR-20 – Edit Novel

## Purpose
Update an existing novel's metadata.

## Related Use Cases
- UC-U21 (Edit Novel)

## Accessible By
Owner (Author), Staff, Admin

## Layout Structure
Same form as SCR-19, pre-filled with existing data.

## Fields
Same as SCR-19, all pre-populated.

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Save Changes | Primary Button | Owner | Update `Novel` record |
| Cancel | Link | Owner | SCR-21 |

---

# SCR-21 – Novel Management Detail

## Purpose
Central management hub for a specific novel — overview, volumes, chapters, settings.

## Related Use Cases
- UC-U21, UC-U22

## Accessible By
Owner, Staff, Admin

## Layout Structure

```
Page
├─ Novel Header (cover, title, status badge)
├─ Tabs: [Overview] [Volumes & Chapters] [Statistics] [Settings]
├─ Overview Tab: novel meta summary
├─ Volumes & Chapters Tab → Link to SCR-22
├─ Statistics Tab → Link to SCR-26
└─ Settings Tab: submit/publish controls
```

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Edit Novel Info | Button | Owner | SCR-20 |
| Manage Volumes | Button | Owner | SCR-22 |
| Submit for Review | Primary Button | Owner | Set Status=Pending |
| Delete Novel | Danger Button | Owner | Confirm → delete |

---

# SCR-22 – Volume Management

## Purpose
Create, reorder, and manage volumes within a novel.

## Related Use Cases
- UC-U23 (Add Volume), UC-U24 (Edit Volume), UC-U25 (Delete Volume)

## Accessible By
Owner, Staff, Admin

## Layout Structure

```
Page
├─ Breadcrumb: My Novels > Novel Title > Volumes
├─ [Add Volume] Button
├─ Volume List (drag-to-reorder)
│   └─ Volume Card: VolumeNumber, Title, Chapter Count, [Manage Chapters] [Edit] [Delete]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Volume Title | Text Input | Yes | `Volume.Title` | Max 200 chars |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Add Volume | Primary Button | Owner | Inline form → create Volume |
| Edit Volume | Icon | Owner | Inline edit |
| Delete Volume | Danger Icon | Owner | Confirm → delete (cascade chapters) |
| Manage Chapters | Button | Owner | SCR-25 |

---

# SCR-23 – Create Chapter

## Purpose
Form to write and save a new chapter.

## Related Use Cases
- UC-U26 (Create Chapter)

## Accessible By
Owner, Staff, Admin

## Layout Structure

```
Page (full-width editor)
├─ Top Bar: Novel Title > Volume > [Chapter Number] [Chapter Title field]
├─ Rich Text Editor (full-width)
├─ Bottom Bar: [Word Count] [Save Draft] [Schedule] [Submit for Review]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Chapter Number | Number | Yes | `Chapter.ChapterNumber` | Auto-incremented, editable |
| Chapter Title | Text | Yes | `Chapter.Title` | Max 200 chars |
| Content | Rich Text | Yes | `ChapterContent.Content` | Max length: nvarchar(max) |
| Release Date | DateTime picker | No | `Chapter.ReleaseDate` | Future date only |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Save Draft | Secondary | Owner | Create Chapter (Status=Draft) |
| Schedule | Ghost Button | Owner | Set ReleaseDate + Status=Scheduled |
| Submit for Review | Primary | Owner | Status=Pending |

---

# SCR-24 – Edit Chapter

## Purpose
Edit title and content of an existing chapter.

## Related Use Cases
- UC-U27 (Edit Chapter)

## Accessible By
Owner, Staff, Admin

## Layout Structure
Same as SCR-23, pre-filled with existing chapter data.

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Save Changes | Primary | Owner | Update Chapter + ChapterContent |
| Cancel | Link | Owner | SCR-25 |

---

# SCR-25 – Chapter Management

## Purpose
View and manage all chapters within a volume.

## Related Use Cases
- UC-U28 (Manage Chapters)

## Layout Structure

```
Page
├─ Breadcrumb: My Novels > Novel > Volume
├─ [Add Chapter] Button
├─ Chapter List Table
```

## Table Design

### Columns

| Column | Sortable | Filterable |
|---|---|---|
| # | Yes | No |
| Title | No | No |
| Status | No | Yes |
| Words | No | No |
| Created | Yes | No |
| Actions | No | No |

### Row Actions
[Edit] [View] [Submit] [Delete]

---

# SCR-26 – Novel Statistics

## Purpose
Analytics dashboard for an author's novel.

## Accessible By
Owner, Staff, Admin

## Sections

- **Views Over Time** — line chart (`Novel.ViewCount` trends)
- **Ratings Distribution** — bar chart (1–5 star breakdown)
- **Comment Activity** — count per chapter
- **Favorites Count** — total `Favorites` records

---

# SCR-27 – Moderation Status

## Purpose
Track submission and moderation status of the author's own novels and chapters.

## Related Use Cases
- UC-U30 (View Moderation Status)

## Layout Structure

```
Page
├─ Title: "Moderation Status"
├─ Tabs: [Novels] [Chapters]
├─ Status Table
```

## Table Design

| Column | Description |
|---|---|
| Title | Novel or Chapter title |
| Submitted At | `UpdatedAt` when submitted |
| Status | `Pending` / `Published` / `Rejected` badge |
| Reviewer Notes | Rejection reason (if any) |

---

# MODULE 4 — FORUM MODULE

---

# SCR-28 – Forum Home

## Purpose
Overview of all forum categories.

## Related Use Cases
- UC-U31

## Layout Structure

```
Page
├─ Title: "Community Forum"
├─ [Create Thread] Button
├─ Category Cards Grid
│   └─ Category Card: Name, Description, Thread count, Latest post
```

---

# SCR-29 – Forum Category Detail

## Purpose
Browse threads within a specific forum category.

## Related Use Cases
- UC-U32

## Layout Structure

```
Page
├─ Breadcrumb: Forum > Category Name
├─ [Create Thread] Button
├─ Thread List Table
│   └─ Row: Title, Author, Replies, Views, Last Reply
└─ Pagination
```

---

# SCR-30 – Thread Detail

## Purpose
Read a discussion thread and all its replies.

## Related Use Cases
- UC-U32, UC-U35, UC-U38

## Layout Structure

```
Page
├─ Thread Header: Title, Author, Posted At, Tags
├─ Original Post Content
├─ Reply List (paginated)
├─ Reply Editor (authenticated users)
└─ Report Thread Link
```

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Post Reply | Primary Button | User | Submit reply |
| Like Post | Icon | User | Increment like |
| Edit Reply | Icon | Owner | Inline editor |
| Delete Reply | Danger | Owner | Delete post |
| Report Thread | Link | User | Open Report Modal |

---

# SCR-31 – Create Thread

## Purpose
Create a new forum discussion thread.

## Related Use Cases
- UC-U33

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Title | Text | Yes | `Thread.Title` | Max 200 chars |
| Category | Dropdown | Yes | `Thread.CategoryId` | Valid category |
| Content | Rich Text | Yes | `Thread.Content` | Min 10 chars |
| Flair | Dropdown | No | `Thread.FlairId` | Optional |

---

# SCR-32 – Edit Thread

## Purpose
Edit owned thread title and content.

## Related Use Cases
- UC-U34

## Layout Structure
Same as SCR-31, pre-filled.

---

# SCR-33 – Edit Post

## Purpose
Edit owned forum reply content.

## Related Use Cases
- UC-U36

## Layout Structure
Inline editor within SCR-30, or dedicated page for long edits.

---

# SCR-34 – My Threads

## Purpose
View all threads created by the current user.

## Layout Structure

```
Page
├─ Title: "My Threads"
├─ Thread Table: Title, Category, Replies, Created, Status, [Actions]
└─ Pagination
```

---

# SCR-35 – Saved Threads

## Purpose
View threads the user has saved/bookmarked.

## Layout Structure
Same as SCR-34 but sourced from saved thread records.

---

# MODULE 5 — NOTIFICATIONS

---

# SCR-36 – Notification Center

## Purpose
View all notifications for the current user.

## Accessible By
User, Staff, Admin

## Layout Structure

```
Page
├─ Title: "Notifications"
├─ Filter Tabs: [All] [Unread] [System] [Comments] [Moderation]
├─ Notification List
│   └─ Row: Icon (type), Message, Timestamp, [Mark as Read]
├─ [Mark All as Read] Button
└─ Pagination
```

## Sections

### Notification Row
**Data Source:** `Notifications` WHERE `UserId = currentUser`.  
**Components:** Type icon (`NotificationType`), message text, relative timestamp, unread dot indicator.

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Mark as Read | Icon | User | Set `IsRead = true` |
| Mark All as Read | Button | User | Bulk update |
| Click Notification | Link | User | Navigate to `EntityType`/`EntityId` |

## States

### Empty State
Illustration + "You have no notifications."

---

# SCR-37 – Notification Detail

## Purpose
View full notification content and navigate to related entity.

## Layout Structure

```
Page / Modal
├─ Notification Type + Icon
├─ Message (full)
├─ Related Entity Link
└─ [Mark as Read] [Dismiss]
```

---

# MODULE 6 — STAFF MODERATION MODULE

---

# SCR-38 – Moderation Dashboard

## Purpose
Overview dashboard for staff showing pending content counts and quick actions.

## Related Use Cases
- UC-S01

## Accessible By
Staff, Admin

## Layout Structure

```
Page
├─ Header: "Moderation Dashboard"
├─ Stats Cards Row
│   ├─ Pending Novels (count)
│   ├─ Pending Chapters (count)
│   ├─ Open Reports (count)
│   └─ Active Warnings (count)
├─ Recent Activity Feed
├─ Quick Links: [Review Novels] [Review Chapters] [Reports Center]
```

## Sections

### Stats Cards
**Data Source:** COUNT queries on `Novels` (Status=Pending), `Chapters` (Status=Pending), `NovelReports`/`UserReports` (Status=Pending).  
**Components:** `card-soft` with large number (`display-lg`), label (`body-sm`), trend delta.

---

# SCR-39 – Pending Novels

## Purpose
List all novels awaiting moderation review.

## Related Use Cases
- UC-S02, UC-S03

## Accessible By
Staff, Admin

## Layout Structure

```
Page
├─ Title: "Pending Novels"
├─ Filter: Submitted Date range
├─ Novels Table
└─ Pagination
```

## Table Design

### Columns

| Column | Sortable | Filterable |
|---|---|---|
| Cover + Title | No | No |
| Author | No | No |
| Category | No | Yes |
| Submitted | Yes | No |
| Actions | No | No |

### Row Actions
[Review] [Quick Approve] [Quick Reject]

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Review | Button | Staff | SCR-40 |
| Quick Approve | Primary Button | Staff | Set Novel.Status=Ongoing → notify author |
| Quick Reject | Danger Button | Staff | Open rejection reason modal |

---

# SCR-40 – Novel Review Detail

## Purpose
Review full novel content (synopsis, cover, metadata) to approve or reject.

## Related Use Cases
- UC-S02, UC-S03, UC-S04

## Accessible By
Staff, Admin

## Layout Structure

```
Page (two-column)
├─ Left: Novel Preview (same as SCR-12 view)
└─ Right: Review Panel
    ├─ Author Info
    ├─ Submission Date
    ├─ Reviewer Notes (Textarea)
    └─ Action Buttons: [Approve] [Reject] [Lock]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Reviewer Notes | Textarea | Yes (on reject) | Rejection reason | Max 1000 chars |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Approve | Primary Button | Staff | Novel.Status=Ongoing → notify author |
| Reject | Danger Button | Staff | Novel.Status=Rejected → notify with notes |
| Lock | Warning Button | Staff | Lock novel from editing |

---

# SCR-41 – Pending Chapters

## Purpose
List all chapters awaiting moderation.

## Related Use Cases
- UC-S05, UC-S06

## Accessible By
Staff, Admin

## Layout Structure
Same as SCR-39 but for `Chapters` (Status=Pending).

## Table Design

### Columns

| Column | Sortable | Filterable |
|---|---|---|
| Novel Title | No | No |
| Chapter # + Title | No | No |
| Author | No | No |
| Word Count | No | No |
| Submitted | Yes | No |
| Actions | No | No |

---

# SCR-42 – Chapter Review Detail

## Purpose
Read chapter content to approve or reject.

## Related Use Cases
- UC-S05, UC-S06, UC-S07

## Accessible By
Staff, Admin

## Layout Structure

```
Page (two-column)
├─ Left: Chapter Content (reading view)
└─ Right: Review Panel
    ├─ Novel + Volume context
    ├─ Reviewer Notes
    └─ [Approve] [Reject] [Lock]
```

---

# SCR-43 – Reports Center

## Purpose
View and manage all incoming reports (novel and user reports).

## Related Use Cases
- UC-S08 → UC-S12

## Accessible By
Staff, Admin

## Layout Structure

```
Page
├─ Title: "Reports Center"
├─ Tabs: [Novel Reports] [User Reports]
├─ Filter: [All] [Pending] [Resolved] [Rejected] + Date Range
├─ Reports Table
└─ Pagination
```

## Table Design

### Columns (Novel Reports)

| Column | Sortable | Filterable |
|---|---|---|
| Target Novel | No | No |
| Report Type | No | Yes |
| Reporter | No | No |
| Submitted | Yes | No |
| Status | No | Yes |
| Actions | No | No |

### Row Actions
[View] [Resolve] [Reject]

---

# SCR-44 – Report Detail

## Purpose
View full report details and take moderation action.

## Related Use Cases
- UC-S08 → UC-S12

## Accessible By
Staff, Admin

## Layout Structure

```
Page
├─ Report Header: Type, Status badge, Submitted Date
├─ Reporter Info
├─ Target Content Preview (novel/chapter/user/comment)
├─ Report Description
├─ Resolution Panel
│   ├─ Action Taken (Textarea)
│   ├─ Resolution Notes (Textarea)
│   └─ [Resolve] [Reject Report]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Action Taken | Textarea | Yes (on resolve) | `BaseReport.ActionTaken` | Max 1000 chars |
| Resolution Notes | Textarea | No | `BaseReport.ResolutionNotes` | Max 1000 chars |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Resolve Report | Primary Button | Staff | Status=Resolved → notify reporter |
| Reject Report | Secondary Button | Staff | Status=Rejected |
| Ban User | Danger Button | Staff | Set `User.Status=Banned` |

---

# SCR-45 – User Warning

## Purpose
Issue a formal warning to a user regarding their content or behavior.

## Related Use Cases
- UC-S16

## Accessible By
Staff, Admin

## Layout Structure

```
Page / Modal
├─ User Info (avatar, username, current warnings)
├─ Warning Reason (Textarea)
├─ Warning Severity (Dropdown: Minor / Major)
└─ [Issue Warning] [Cancel]
```

---

# SCR-46 – Moderation History

## Purpose
View historical log of all moderation actions taken.

## Related Use Cases
- UC-S17

## Accessible By
Staff, Admin

## Layout Structure

```
Page
├─ Title: "Moderation History"
├─ Filter: Date Range, Staff Member, Action Type
├─ History Table
└─ Pagination
```

## Table Design

| Column | Sortable | Filterable |
|---|---|---|
| Date | Yes | No |
| Staff | No | Yes |
| Action Type | No | Yes |
| Target | No | No |
| Notes | No | No |

---

# MODULE 7 — ADMINISTRATION MODULE

---

# SCR-47 – Admin Dashboard

## Purpose
System-wide overview dashboard for administrators.

## Related Use Cases
- UC-A13

## Accessible By
Admin

## Layout Structure

```
Page
├─ Header: "Admin Dashboard"
├─ Stats Cards Row (6-up)
│   ├─ Total Users
│   ├─ Total Novels
│   ├─ Total Chapters
│   ├─ Pending Moderation
│   ├─ Open Reports
│   └─ New Users (this week)
├─ Charts Section
│   ├─ User growth (line chart)
│   └─ Novel publish rate (bar chart)
├─ Recent Activity Feed
└─ Quick Nav Links
```

---

# SCR-48 – User Management

## Purpose
Search, filter, and manage all platform users.

## Related Use Cases
- UC-A01, UC-A02

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "User Management"
├─ Search Bar + Filter: [All Roles] [Active / Banned]
├─ Users Table
└─ Pagination
```

## Table Design

### Columns

| Column | Sortable | Filterable |
|---|---|---|
| Avatar + Username | No | No |
| Email | No | No |
| Role | No | Yes |
| Status | No | Yes |
| Joined | Yes | No |
| Novels | No | No |
| Actions | No | No |

### Row Actions
[View] [Edit Role] [Ban] [Unban] [Delete]

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Search | Input | Admin | Filter table |
| View | Button | Admin | SCR-49 |
| Ban | Danger Button | Admin | Set `User.Status=Banned` |
| Unban | Success Button | Admin | Set `User.Status=Offline` |
| Edit Role | Dropdown | Admin | Update `User.Role` |

---

# SCR-49 – User Detail (Admin)

## Purpose
View detailed user profile, activity, warnings, and manage status/role.

## Related Use Cases
- UC-A01, UC-A02

## Accessible By
Admin

## Layout Structure

```
Page (two-column)
├─ Left: User Profile (same as SCR-07)
└─ Right: Admin Panel
    ├─ Role Control (Dropdown)
    ├─ Status Control (Active / Banned)
    ├─ Warning History
    ├─ Report History (as reporter / target)
    └─ Action Buttons: [Save Changes] [Issue Warning] [Delete Account]
```

---

# SCR-50 – Staff Management

## Purpose
Manage staff accounts — assign or remove Staff role.

## Related Use Cases
- UC-A03, UC-A04

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Staff Management"
├─ [Add Staff] Button (promote existing user)
├─ Staff Table: Username, Email, Joined As Staff, Moderation Count, [Remove Staff]
```

---

# SCR-51 – Badge Management

## Purpose
Create and manage user achievement badges.

## Related Use Cases
- UC-A05

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Badge Management"
├─ [Create Badge] Button
├─ Badge Cards Grid
│   └─ Badge Card: Icon, Name, Description, Awarded Count, [Edit] [Delete]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Key | Text | Yes | `Badge.Key` | Unique, max 100 chars |
| Name | Text | Yes | `Badge.Name` | Max 100 chars |
| Description | Text | Yes | `Badge.Description` | Max 500 chars |
| Icon | Image Upload | No | `Badge.Icon` | Max 512px URL |
| Color | Color Picker | No | `Badge.Color` | Hex color code |

---

# SCR-52 – Novel Categories

## Purpose
Manage novel categories (create, edit, delete).

## Related Use Cases
- UC-A06

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Novel Categories"
├─ [Add Category] Button
├─ Category Table: Name, Slug, Novel Count, [Edit] [Delete]
```

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Name | Text | Yes | `Category.Name` | Max 100 chars, unique |
| Slug | Text | Yes | `Category.Slug` | Max 120 chars, unique, URL-safe |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Add Category | Primary Button | Admin | Inline form → create Category |
| Edit | Icon | Admin | Inline edit |
| Delete | Danger Icon | Admin | Confirm → delete (SetNull on Novels) |

---

# SCR-53 – Tag Management

## Purpose
Manage tags for novels.

## Related Use Cases
- UC-A07

## Accessible By
Admin

## Layout Structure
Same as SCR-52 but for `Tags` entity.

## Fields

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Name | Text | Yes | `Tag.Name` | Max 100 chars, unique |
| Slug | Text | Yes | `Tag.Slug` | Max 120 chars, unique, URL-safe |

---

# SCR-54 – Forum Categories

## Purpose
Manage forum categories.

## Related Use Cases
- UC-A08

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Forum Categories"
├─ [Add Category] Button
├─ Category Table: Name, Description, Thread Count, [Edit] [Delete]
```

---

# SCR-55 – Forum Flairs

## Purpose
Manage thread flairs/labels for forum categories.

## Related Use Cases
- UC-A09

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Forum Flairs"
├─ Category selector
├─ Flair list: Name, Color, [Edit] [Delete]
├─ [Add Flair] Button
```

---

# SCR-56 – Notification Management

## Purpose
Create and send system-wide notifications to users or groups.

## Related Use Cases
- UC-A10

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Notification Management"
├─ [Send Notification] Button
├─ Sent Notifications Table: Message, Type, Target, Sent At, Read Count
```

## Fields (Create Notification Modal)

| Field | Type | Required | Source Entity | Validation |
|---|---|---|---|---|
| Type | Dropdown | Yes | `Notification.NotificationType` | SystemAlert, etc. |
| Message | Textarea | Yes | `Notification.Message` | Max 1000 chars |
| Target | Radio | Yes | — | All Users / Specific User |
| User (if specific) | Autocomplete | Conditional | `Notification.UserId` | Valid user |

---

# SCR-57 – Reports Overview (Admin)

## Purpose
High-level view of all reports across the platform.

## Related Use Cases
- UC-A11

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Reports Overview"
├─ Stats Cards: [Open] [Resolved] [Rejected]
├─ Tabs: [Novel Reports] [User Reports]
├─ Reports Table (same as SCR-43)
└─ Export Button
```

---

# SCR-58 – Audit Logs

## Purpose
View system audit log of all significant actions.

## Related Use Cases
- UC-A12

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Audit Logs"
├─ Filter: Date Range, Actor, Action Type, Entity Type
├─ Audit Log Table
└─ Export CSV Button
```

## Table Design

| Column | Sortable | Filterable |
|---|---|---|
| Timestamp | Yes | No |
| Actor (User) | No | Yes |
| Action | No | Yes |
| Entity Type | No | Yes |
| Entity ID | No | No |
| IP Address | No | No |

---

# SCR-59 – Statistics Dashboard

## Purpose
Comprehensive analytics dashboard for platform statistics.

## Related Use Cases
- UC-A13

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Platform Statistics"
├─ Date Range Picker
├─ Stat Cards Row: [DAU] [MAU] [New Novels] [New Chapters] [Comments] [Reports]
├─ Charts Section
│   ├─ User Growth (line chart, 30-day)
│   ├─ Novel Publish Rate (bar chart)
│   ├─ Top Novels (table)
│   └─ Category Distribution (pie chart)
```

---

# SCR-60 – Novel Override

## Purpose
Force-manage any novel regardless of ownership.

## Related Use Cases
- UC-A14

## Accessible By
Admin

## Layout Structure

```
Page
├─ Search Novel (by title or ID)
├─ Novel Preview (same as SCR-12)
└─ Admin Override Panel
    ├─ Change Status (Dropdown)
    ├─ Change Author (Autocomplete)
    ├─ Lock / Unlock
    ├─ Force Delete
    └─ Action Log
```

---

# SCR-61 – Chapter Override

## Purpose
Force-manage any chapter regardless of ownership.

## Related Use Cases
- UC-A15

## Accessible By
Admin

## Layout Structure

```
Page
├─ Search Chapter (by novel + chapter number)
├─ Chapter Content Preview
└─ Admin Override Panel
    ├─ Change Status
    ├─ Edit Content
    ├─ Force Delete
    └─ Action Log
```

---

# SCR-62 – System Settings

## Purpose
Configure platform-wide settings.

## Related Use Cases
- UC-A16, UC-A17

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "System Settings"
├─ Settings Sections (accordion or tabs)
│   ├─ General: Site name, tagline, logo
│   ├─ Security: Password policy, session duration
│   ├─ Content: Max novel description length, chapter limits
│   ├─ Moderation: Auto-reject keywords, review SLA
│   └─ Email: SMTP settings, templates
```

---

# SCR-63 – Announcement Management

## Purpose
Create and manage homepage banners and announcements.

## Related Use Cases
- UC-A18

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Announcements"
├─ [Create Announcement] Button
├─ Announcement List
│   └─ Row: Title, Status (Active/Inactive), Start Date, End Date, [Edit] [Delete] [Toggle]
```

## Fields

| Field | Type | Required | Validation |
|---|---|---|---|
| Title | Text | Yes | Max 200 chars |
| Content | Rich Text | Yes | Max 2000 chars |
| Start Date | Date Picker | Yes | Today or future |
| End Date | Date Picker | No | After Start Date |
| Active | Toggle | Yes | — |

---

# SCR-64 – Backup & Restore

## Purpose
Manage system data backups and restore operations.

## Related Use Cases
- UC-A19, UC-A20

## Accessible By
Admin

## Layout Structure

```
Page
├─ Title: "Backup & Restore"
├─ Backup Section
│   ├─ [Create Backup Now] Button
│   ├─ Backup History Table
│   └─ Scheduled Backup Settings
├─ Restore Section
│   ├─ Warning Banner
│   ├─ Select Backup Dropdown
│   └─ [Restore] Button (requires confirmation)
```

## Table Design (Backup History)

| Column | Description |
|---|---|
| Backup ID | Unique identifier |
| Created At | Timestamp |
| Size | File size |
| Status | Completed / Failed |
| Actions | [Download] [Restore] [Delete] |

## Actions

| Action | Type | Permission | Result |
|---|---|---|---|
| Create Backup | Primary Button | Admin | Trigger backup job → toast progress |
| Download | Icon Button | Admin | Download backup file |
| Restore | Danger Button | Admin | Confirm modal → restore system |

## States

### Loading State (Backup in Progress)
Progress bar + "Backup in progress... Please do not navigate away."

### Error State
Alert: "Backup failed. Check system logs."

### Restore Confirmation Modal
⚠️ Warning: "Restoring will overwrite current data. This action cannot be undone." + typed confirmation ("RESTORE") required.

---

# Global Components Reference

## Shared Components Used Across All Screens

| Component | Usage |
|---|---|
| `NavBar` | All authenticated pages |
| `ExceptionHandlingMiddleware` toast | All pages (error feedback) |
| `Pagination` | All list/table screens |
| `Modal (confirm)` | Delete, approve, reject actions |
| `Report Modal` | SCR-12, SCR-13, SCR-30, SCR-44 |
| `Avatar` | Profile, comment, author areas |
| `Badge (status)` | Novel status, notification type |
| `Search Bar` | SCR-01, SCR-09, SCR-10, SCR-48 |
| `Skeleton loader` | All async data screens |
| `Empty state card` | All list screens |
| `Toast notification` | All write operations |

## Global State Rules

| State | Behavior |
|---|---|
| **Unauthenticated** | Redirect to SCR-03 for protected pages |
| **Banned** | Show "Account banned" page, logout |
| **Insufficient Role** | Show 403 page: "You don't have permission to view this page." |
| **Not Found** | Show 404 page with [Go Home] button |
| **Server Error** | Show 500 page with [Retry] button |

## Responsive Breakpoints

| Breakpoint | Width | Key Changes |
|---|---|---|
| Mobile | < 600px | 1-column grids, hamburger nav, full-width inputs |
| Tablet | 600–959px | 2-column grids, sidebar collapses |
| Desktop | 960–1199px | Full multi-column, sidebar visible |
| Wide | ≥ 1200px | Max-width 1400px container |

---

*Generated from: `screen.md`, `spec.md`, `Agents.md`, `UI.md`, `data.md`*  
*LitNovel Platform — Screen Design Specification v1.0*
