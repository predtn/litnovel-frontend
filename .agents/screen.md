# Screen Catalog - LitNovel System

## 1. Authentication & Profile

| Screen ID | Screen Name     | Description                                                   | Related Use Cases   |
| --------- | --------------- | ------------------------------------------------------------- | ------------------- |
| SCR-01    | Landing Page    | Homepage introducing the platform with Login/Register options | UC-U01, UC-U02      |
| SCR-02    | Register        | User registration form                                        | UC-U01              |
| SCR-03    | Login           | User login form                                               | UC-U02              |
| SCR-04    | Forgot Password | Request password reset email                                  | Forgot Password     |
| SCR-05    | Reset Password  | Create a new password                                         | Reset Password      |
| SCR-06    | User Profile    | Manage personal information and avatar                        | UC-U04              |
| SCR-07    | Public Profile  | View public profile of a user                                 | View Public Profile |
| SCR-08    | Change Password | Change account password                                       | Change Password     |

---

## 2. Reading Module

| Screen ID | Screen Name     | Description                                        | Related Use Cases                              |
| --------- | --------------- | -------------------------------------------------- | ---------------------------------------------- |
| SCR-09    | Home Page       | Featured novels, rankings, recommendations         | UC-U05                                         |
| SCR-10    | Novel Catalog   | Browse novels with filters and sorting             | UC-U05                                         |
| SCR-11    | Search Results  | Display search results                             | UC-U06                                         |
| SCR-12    | Novel Detail    | View novel information, ratings, reviews, comments | UC-U07, UC-U10, UC-U11, UC-U12, UC-U13, UC-U15 |
| SCR-13    | Chapter Reader  | Read chapter content and interact with comments    | UC-U08, UC-U14                                 |
| SCR-14    | Reading History | Continue previously read novels                    | UC-U09                                         |
| SCR-15    | Favorites List  | View followed/favorited novels                     | UC-U10, UC-U11                                 |
| SCR-16    | Liked Novels    | View liked novels history                          | UC-U16                                         |
| SCR-17    | Bookmarks       | View bookmarked chapters                           | Bookmark Chapter                               |

---

## 3. Publishing Module

| Screen ID | Screen Name             | Description                             | Related Use Cases      |
| --------- | ----------------------- | --------------------------------------- | ---------------------- |
| SCR-18    | My Novels Dashboard     | Dashboard of novels created by the user | UC-U29, UC-U30         |
| SCR-19    | Create Novel            | Create a new novel                      | UC-U20                 |
| SCR-20    | Edit Novel              | Update novel information                | UC-U21                 |
| SCR-21    | Novel Management Detail | Manage a specific novel                 | UC-U21, UC-U22         |
| SCR-22    | Volume Management       | Manage volumes within a novel           | UC-U23, UC-U24, UC-U25 |
| SCR-23    | Create Chapter          | Create a new chapter                    | UC-U26                 |
| SCR-24    | Edit Chapter            | Edit chapter title and content          | UC-U27                 |
| SCR-25    | Chapter Management      | Manage chapter list                     | UC-U28                 |
| SCR-26    | Novel Statistics        | View views, ratings, followers          | Analytics              |
| SCR-27    | Moderation Status       | Track moderation results                | UC-U30                 |

---

## 4. Forum Module

| Screen ID | Screen Name           | Description                    | Related Use Cases      |
| --------- | --------------------- | ------------------------------ | ---------------------- |
| SCR-28    | Forum Home            | Forum category overview        | UC-U31                 |
| SCR-29    | Forum Category Detail | Browse threads in a category   | UC-U32                 |
| SCR-30    | Thread Detail         | View thread and replies        | UC-U32, UC-U35, UC-U38 |
| SCR-31    | Create Thread         | Create a new discussion thread | UC-U33                 |
| SCR-32    | Edit Thread           | Edit owned thread              | UC-U34                 |
| SCR-33    | Edit Post             | Edit owned forum post          | UC-U36                 |
| SCR-34    | My Threads            | View user-created threads      | UC-U37                 |
| SCR-35    | Saved Threads         | View saved threads             | Save Thread            |

---

## 5. Notification Module

| Screen ID | Screen Name         | Description               | Related Use Cases  |
| --------- | ------------------- | ------------------------- | ------------------ |
| SCR-36    | Notification Center | View notifications        | View Notifications |
| SCR-37    | Notification Detail | View notification content | View Notification  |

---

## 6. Staff Moderation Module

| Screen ID | Screen Name           | Description                   | Related Use Cases      |
| --------- | --------------------- | ----------------------------- | ---------------------- |
| SCR-38    | Moderation Dashboard  | Moderation overview dashboard | UC-S01                 |
| SCR-39    | Pending Novels        | List pending novels           | UC-S02, UC-S03         |
| SCR-40    | Novel Review Detail   | Review novel content          | UC-S02, UC-S03, UC-S04 |
| SCR-41    | Pending Chapters      | List pending chapters         | UC-S05, UC-S06         |
| SCR-42    | Chapter Review Detail | Review chapter content        | UC-S05, UC-S06, UC-S07 |
| SCR-43    | Reports Center        | View and manage reports       | UC-S08 → UC-S12        |
| SCR-44    | Report Detail         | Process a specific report     | UC-S08 → UC-S12        |
| SCR-45    | User Warning          | Issue warning to user         | UC-S16                 |
| SCR-46    | Moderation History    | View moderation logs          | UC-S17                 |

---

## 7. Administration Module

| Screen ID | Screen Name             | Description                     | Related Use Cases |
| --------- | ----------------------- | ------------------------------- | ----------------- |
| SCR-47    | Admin Dashboard         | System overview dashboard       | UC-A13            |
| SCR-48    | User Management         | Manage users                    | UC-A01, UC-A02    |
| SCR-49    | User Detail             | View user profile and status    | UC-A01, UC-A02    |
| SCR-50    | Staff Management        | Manage staff accounts           | UC-A03, UC-A04    |
| SCR-51    | Badge Management        | Manage user badges              | UC-A05            |
| SCR-52    | Novel Categories        | Manage novel categories         | UC-A06            |
| SCR-53    | Tag Management          | Manage tags                     | UC-A07            |
| SCR-54    | Forum Categories        | Manage forum categories         | UC-A08            |
| SCR-55    | Forum Flairs            | Manage forum flairs             | UC-A09            |
| SCR-56    | Notification Management | Manage system notifications     | UC-A10            |
| SCR-57    | Reports Overview        | View all reports                | UC-A11            |
| SCR-58    | Audit Logs              | View system logs                | UC-A12            |
| SCR-59    | Statistics Dashboard    | System statistics and analytics | UC-A13            |
| SCR-60    | Novel Override          | Force-manage novels             | UC-A14            |
| SCR-61    | Chapter Override        | Force-manage chapters           | UC-A15            |
| SCR-62    | System Settings         | Configure system settings       | UC-A16, UC-A17    |
| SCR-63    | Announcement Management | Manage homepage announcements   | UC-A18            |
| SCR-64    | Backup & Restore        | Backup and restore system data  | UC-A19, UC-A20    |

---

# Summary

| Module                   | Number of Screens |
| ------------------------ | ----------------- |
| Authentication & Profile | 8                 |
| Reading                  | 9                 |
| Publishing               | 10                |
| Forum                    | 8                 |
| Notifications            | 2                 |
| Staff Moderation         | 9                 |
| Administration           | 18                |
| **Total**                | **64 Screens**    |
