# LitNovel — UI Component Library & Design System

> Version: 1.0  
> Style: Modern SaaS (Linear / Notion / GitHub / Medium influenced)  
> Base: Vercel-Inspired Design System (see `UI.md`)  
> Application: [spec.md](./spec.md) | Screens: [screen.md](./screen.md)

---

## Table of Contents

1. [Design Principles](#1-design-principles)
2. [Design Tokens](#2-design-tokens)
3. [Layout Components](#3-layout-components)
4. [Navigation Components](#4-navigation-components)
5. [Form Components](#5-form-components)
6. [Button Library](#6-button-library)
7. [Data Display Components](#7-data-display-components)
8. [Feedback Components](#8-feedback-components)
9. [Overlay Components](#9-overlay-components)
10. [Content Components](#10-content-components)
11. [Dashboard Components](#11-dashboard-components)
12. [Empty States](#12-empty-states)
13. [Loading States](#13-loading-states)
14. [Accessibility Standards](#14-accessibility-standards)
15. [Responsive Design Rules](#15-responsive-design-rules)
16. [Component Usage Matrix](#16-component-usage-matrix)
17. [Naming Conventions](#17-naming-conventions)
18. [Implementation Notes](#18-implementation-notes)

---

# 1. Design Principles

## Simplicity
Every component should do one thing well. Remove anything that doesn't serve the user's goal. Prefer plain language labels, single-purpose actions, and uncluttered surfaces. If a design requires explanation, simplify it.

## Consistency
The same patterns must appear in the same way across all screens. A "primary action" always uses the same button style. A "status indicator" always uses the same badge pattern. Predictability reduces cognitive load.

## Accessibility
All components follow WCAG 2.1 AA. Every interactive element is keyboard-navigable, has visible focus indicators, has sufficient color contrast (≥ 4.5:1 for text), and has appropriate ARIA attributes.

## Scalability
The design system supports growth from 8 screens to 64+ without modification. Token-based theming ensures visual changes cascade from a single source of truth.

## Responsiveness
Every component works at three breakpoints: Mobile (< 768px), Tablet (768–1279px), Desktop (≥ 1280px). Components never break at intermediate widths.

## Reusability
Build once, use everywhere. Components are designed in isolation and composed into screens. No one-off styles. Every visual decision maps to a token.

---

# 2. Design Tokens

## 2.1 Colors

### Primary Scale (Ink / Black)

| Token | HEX | Usage |
|---|---|---|
| `primary-50` | `#f2f2f2` | Hover backgrounds on primary elements |
| `primary-100` | `#e6e6e6` | Light primary tints |
| `primary-200` | `#cccccc` | Border on primary surfaces |
| `primary-300` | `#999999` | Disabled primary text |
| `primary-400` | `#666666` | Placeholder text on dark bg |
| `primary-500` | `#404040` | Muted text |
| `primary-600` | `#2e2e2e` | Secondary dark text |
| `primary-700` | `#1f1f1f` | Dark surfaces |
| `primary-800` | `#171717` | **Primary brand color** — buttons, CTAs |
| `primary-900` | `#0a0a0a` | Deep black, code blocks |

### Secondary Scale (Blue)

| Token | HEX | Usage |
|---|---|---|
| `secondary-50` | `#eff6ff` | Info background |
| `secondary-100` | `#dbeafe` | Light info tint |
| `secondary-200` | `#bfdbfe` | Info border |
| `secondary-300` | `#93c5fd` | Info icon |
| `secondary-400` | `#60a5fa` | Info badge |
| `secondary-500` | `#3b82f6` | Info text |
| `secondary-600` | `#2563eb` | Secondary button |
| `secondary-700` | `#1d4ed8` | Link deep |
| `secondary-800` | `#1e40af` | Active nav item |
| `secondary-900` | `#1e3a8a` | Deep info |

### Success Scale (Green)

| Token | HEX | Usage |
|---|---|---|
| `success-50` | `#f0fdf4` | Success background |
| `success-100` | `#dcfce7` | Success banner bg |
| `success-200` | `#bbf7d0` | Success border |
| `success-300` | `#86efac` | Success icon light |
| `success-400` | `#4ade80` | Success progress |
| `success-500` | `#22c55e` | **Success primary** |
| `success-600` | `#16a34a` | Success button |
| `success-700` | `#15803d` | Success deep |
| `success-800` | `#166534` | Success dark |
| `success-900` | `#14532d` | Success text on light bg |

### Warning Scale (Amber)

| Token | HEX | Usage |
|---|---|---|
| `warning-50` | `#fffbeb` | Warning background |
| `warning-100` | `#fef3c7` | Warning banner bg |
| `warning-200` | `#fde68a` | Warning border |
| `warning-300` | `#fcd34d` | Warning icon |
| `warning-400` | `#fbbf24` | Warning badge |
| `warning-500` | `#f59e0b` | **Warning primary** |
| `warning-600` | `#d97706` | Warning button |
| `warning-700` | `#b45309` | Warning text |
| `warning-800` | `#92400e` | Warning dark |
| `warning-900` | `#78350f` | Warning deep |

### Error Scale (Red)

| Token | HEX | Usage |
|---|---|---|
| `error-50` | `#fef2f2` | Error background |
| `error-100` | `#fee2e2` | Error banner bg |
| `error-200` | `#fecaca` | Error border |
| `error-300` | `#fca5a5` | Error icon |
| `error-400` | `#f87171` | Error badge |
| `error-500` | `#ef4444` | **Error primary** |
| `error-600` | `#dc2626` | Error button |
| `error-700` | `#b91c1c` | Error text `#ee0000` |
| `error-800` | `#991b1b` | Error dark |
| `error-900` | `#7f1d1d` | Error deep |

### Info Scale (Cyan)

| Token | HEX | Usage |
|---|---|---|
| `info-50` | `#ecfeff` | Info subtle bg |
| `info-100` | `#cffafe` | Info banner bg |
| `info-500` | `#06b6d4` | Info primary |
| `info-600` | `#0891b2` | Info button |
| `info-700` | `#0e7490` | Info text |

### Neutral / Gray Scale

| Token | HEX | Usage |
|---|---|---|
| `neutral-0` | `#ffffff` | Canvas / card bg |
| `neutral-50` | `#fafafa` | Page background (canvas-soft) |
| `neutral-100` | `#f5f5f5` | Inset bg (canvas-soft-2) |
| `neutral-200` | `#ebebeb` | Hairline / divider |
| `neutral-300` | `#d4d4d4` | Stronger divider |
| `neutral-400` | `#a1a1a1` | Hairline-strong / disabled icon |
| `neutral-500` | `#888888` | Muted text |
| `neutral-600` | `#4d4d4d` | Body text |
| `neutral-700` | `#363636` | Secondary headings |
| `neutral-800` | `#262626` | Primary text |
| `neutral-900` | `#171717` | Ink / headings |

### Semantic Color Aliases

| Alias | Maps to | Usage |
|---|---|---|
| `color-bg` | `neutral-50` | Default page background |
| `color-surface` | `neutral-0` | Card / modal surface |
| `color-surface-raised` | `neutral-100` | Inset / nested surface |
| `color-border` | `neutral-200` | Default border / hairline |
| `color-border-strong` | `neutral-400` | Emphasized border |
| `color-text-primary` | `neutral-900` | Headings, strong labels |
| `color-text-secondary` | `neutral-600` | Body text |
| `color-text-muted` | `neutral-500` | Captions, hints |
| `color-text-disabled` | `neutral-400` | Disabled inputs |
| `color-link` | `secondary-700` `#0070f3` | Inline links |
| `color-action` | `primary-800` `#171717` | Primary CTA |
| `color-action-text` | `neutral-0` | Text on primary CTA |

---

## 2.2 Typography

### Font Families

```
Primary (Sans):   "Inter", "Geist", system-ui, -apple-system, sans-serif
Monospace:        "JetBrains Mono", "Geist Mono", ui-monospace, monospace
```

### Headings

| Style | Size | Weight | Line Height | Letter Spacing | Usage |
|---|---|---|---|---|---|
| H1 / Display XL | 48px | 600 | 48px | -2.4px | Hero headlines |
| H2 / Display LG | 32px | 600 | 40px | -1.28px | Section headings |
| H3 / Display MD | 24px | 600 | 32px | -0.96px | Card group titles |
| H4 / Display SM | 20px | 600 | 28px | -0.6px | Subsection titles |
| H5 | 18px | 600 | 24px | -0.36px | Sidebar group titles |
| H6 | 16px | 600 | 22px | -0.16px | Small section titles |

### Body Text

| Style | Size | Weight | Line Height | Letter Spacing | Usage |
|---|---|---|---|---|---|
| Body LG | 18px | 400 | 28px | 0 | Lead paragraphs |
| Body MD | 16px | 400 | 24px | 0 | Default body |
| Body MD Strong | 16px | 500 | 24px | 0 | Emphasized body |
| Body SM | 14px | 400 | 20px | -0.28px | Secondary text, nav |
| Body SM Strong | 14px | 500 | 20px | -0.28px | Nav CTAs, table emphasis |

### Captions & Labels

| Style | Size | Weight | Line Height | Usage |
|---|---|---|---|---|
| Caption | 12px | 400 | 16px | Footer, badges, hints |
| Caption Mono | 12px | 400 | 16px | Technical labels (mono font) |
| Label | 13px | 500 | 16px | Form labels |
| Code | 13px | 400 | 20px | Inline code, terminal |

### Typography Rules
- Headlines always use **sentence-case** with aggressive negative letter-spacing.
- Never use `font-weight: 700` or heavier for the primary sans. Ceiling is `600`.
- Monospace font is reserved for code blocks, technical labels, status codes.
- Line-height ≥ 1.5 for all body text (accessibility).

---

## 2.3 Spacing System

4px base grid. All spacing values are multiples of 4.

| Token | Value | Usage |
|---|---|---|
| `space-1` | 4px | Tightest gap (icon + text) |
| `space-2` | 8px | Inner component padding |
| `space-3` | 12px | Card inner padding (sm) |
| `space-4` | 16px | Default padding |
| `space-5` | 20px | Button padding |
| `space-6` | 24px | Section internal gap |
| `space-8` | 32px | Card padding |
| `space-10` | 40px | Section padding (small) |
| `space-12` | 48px | Section padding (medium) |
| `space-16` | 64px | Section padding (large) |
| `space-24` | 96px | Hero section padding |
| `space-32` | 128px | Max hero padding |

---

## 2.4 Border Radius

| Token | Value | Usage |
|---|---|---|
| `radius-none` | 0px | Full-bleed elements |
| `radius-xs` | 4px | Tight chips, tiny badges |
| `radius-sm` | 6px | Nav buttons, inputs, dropdowns |
| `radius-md` | 8px | Cards, feature blocks |
| `radius-lg` | 12px | Large cards, modals |
| `radius-xl` | 16px | Image containers |
| `radius-2xl` | 24px | Floating panels |
| `radius-pill` | 9999px | Pill buttons, status badges |
| `radius-full` | 9999px | Avatars, circular icon buttons |

---

## 2.5 Shadows

| Token | CSS Value | Usage |
|---|---|---|
| `shadow-xs` | `0 1px 2px rgba(0,0,0,0.04)` | Subtle lifted surface |
| `shadow-sm` | `0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)` | Default card |
| `shadow-md` | `0 4px 6px rgba(0,0,0,0.04), 0 2px 4px rgba(0,0,0,0.04)` | Hover card, dropdown |
| `shadow-lg` | `0 10px 15px rgba(0,0,0,0.06), 0 4px 6px rgba(0,0,0,0.03)` | Modal, floating panel |
| `shadow-xl` | `0 20px 25px rgba(0,0,0,0.08), 0 10px 10px rgba(0,0,0,0.03)` | Important modals |
| `shadow-inset` | `inset 0 0 0 1px rgba(0,0,0,0.08)` | Card border ring |

### Shadow Usage Pattern
Cards always combine `shadow-sm` + `shadow-inset` (ring border). Never use a single heavy drop shadow.

---

## 2.6 Z-Index Scale

| Token | Value | Usage |
|---|---|---|
| `z-base` | 0 | Default elements |
| `z-raised` | 10 | Sticky headers |
| `z-dropdown` | 100 | Dropdown menus |
| `z-sticky` | 200 | Sticky sidebar |
| `z-overlay` | 300 | Drawer backdrop |
| `z-modal` | 400 | Modals |
| `z-toast` | 500 | Toast notifications |
| `z-tooltip` | 600 | Tooltips (always on top) |

---

# 3. Layout Components

## 3.1 App Shell

The App Shell is the top-level layout container used on all authenticated pages.

### Structure

```
┌─────────────────────────────────────────────────────┐
│                     Header (64px)                    │
├──────────────┬──────────────────────────────────────┤
│              │                                       │
│   Sidebar    │          Content Area                 │
│   (240px)    │          (flex-grow)                  │
│              │                                       │
├──────────────┴──────────────────────────────────────┤
│                     Footer (optional)                │
└─────────────────────────────────────────────────────┘
```

### Responsive Behavior

| Breakpoint | Behavior |
|---|---|
| Desktop ≥ 1280px | Sidebar always visible (240px), content scrollable |
| Tablet 768–1279px | Sidebar collapsible to icon-only (60px), toggle button in header |
| Mobile < 768px | Sidebar becomes bottom nav or off-canvas drawer |

### Variants
- **Public Shell** — Header + Content + Footer, no sidebar
- **Reader Shell** — Minimal header + full-width content (chapter reader)
- **Dashboard Shell** — Header + Sidebar + Content (authenticated)
- **Admin Shell** — Header + Admin Sidebar (dark) + Content

---

## 3.2 Header

**Height:** 64px  
**Background:** `color-surface` (`#ffffff`)  
**Border:** 1px solid `color-border`  
**Position:** Sticky, `z-sticky`

### Variant: Public Header

```
[Logo]    [Browse] [Rankings] [Forum]    [Login] [Sign Up]
```

Components: Logo, NavLinks, Button (Secondary SM), Button (Primary SM)

### Variant: Authenticated Header

```
[Logo]    [Search Bar]    [🔔 Notifications] [Avatar Menu ▾]
```

Components: Logo, SearchInput, IconButton (Bell), Avatar + Dropdown

### Variant: Admin / Staff Header

```
[Logo + "Admin"]    [Search]    [🔔] [Avatar + Role Badge]
```

Components: Logo, AdminBadge, SearchInput, IconButton, Avatar

### Navigation Pattern
- Desktop: Inline nav links
- Tablet: Inline, overflow handled by `...more` dropdown
- Mobile: Hamburger icon → Full-screen drawer menu

---

## 3.3 Sidebar

### Variant: User Sidebar (Dashboard / Publishing)

**Width:** 240px expanded, 60px collapsed  
**Background:** `color-surface`  
**Border-right:** 1px solid `color-border`

```
Navigation Groups:
─ Dashboard
─ My Novels
─ Reading History
─ Favorites
─ Notifications
─ Profile
─ Settings
```

### Variant: Staff Sidebar

```
─ Dashboard
─ Pending Novels
─ Pending Chapters
─ Reports Center
─ Moderation History
─ User Warnings
```

### Variant: Admin Sidebar

```
─ Dashboard
─ Users
─ Staff
─ Novels (Override)
─ Categories & Tags
─ Forum
─ Notifications
─ Reports
─ Audit Logs
─ Statistics
─ System Settings
─ Backup & Restore
```

### Sidebar Item States

| State | Visual |
|---|---|
| Default | Body text, transparent bg |
| Hover | `neutral-100` bg, `radius-sm` |
| Active | `primary-800` left border (3px), bg `neutral-100`, text bold |
| Disabled | Muted text, no interaction |

### Mobile Behavior
Off-canvas drawer, slides in from left. Backdrop overlay `z-overlay`. Close on outside click or ESC key.

---

## 3.4 Page Container

```css
.page-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 24px;       /* Desktop */
}

@media (max-width: 1279px) {
  padding: 0 16px;       /* Tablet */
}

@media (max-width: 767px) {
  padding: 0 12px;       /* Mobile */
}
```

### Content Width Variants

| Variant | Max Width | Usage |
|---|---|---|
| `narrow` | 720px | Auth pages, reader, forms |
| `default` | 1024px | Content pages |
| `wide` | 1280px | Dashboards, tables |
| `full` | 1400px | Admin, overview pages |

---

# 4. Navigation Components

## 4.1 Breadcrumb

**Purpose:** Contextual navigation showing current location in hierarchy.  
**Max depth:** 4 levels (deeper paths truncated with `...`)

### Anatomy
```
My Novels  /  Novel Title  /  Volume 1  /  Chapter 3
```

### States
- Default: `neutral-400` separator `/`, `neutral-600` items, last item `neutral-900` (current)
- Hover: Underline on non-current items
- Overflow: Middle items collapsed to `...` with dropdown on hover

### Accessibility
- `<nav aria-label="Breadcrumb">` wrapper
- `aria-current="page"` on last item

---

## 4.2 Tabs

**Purpose:** Switch between related content panels without page reload.

### Variants

| Variant | Style | Usage |
|---|---|---|
| `tabs-underline` | Bottom border indicator | Profile, Novel Detail |
| `tabs-pill` | Filled pill (ghost button style) | Catalog filters |
| `tabs-card` | Card-style tabs | Admin settings |

### Anatomy
```
[Tab 1]  [Tab 2 ●]  [Tab 3]
         ────────── (active indicator)
```

### States
- Default: `neutral-600` text, no bg
- Hover: `neutral-900` text, `neutral-100` bg
- Active: `neutral-900` text bold, `neutral-900` border-bottom or filled bg
- Disabled: `neutral-400` text, no interaction

### Accessibility
- `role="tablist"`, `role="tab"`, `role="tabpanel"`
- Arrow key navigation between tabs

---

## 4.3 Pagination

**Purpose:** Navigate between pages of list/table data.

### Anatomy
```
[< Prev]  [1] [2] [3] ... [8] [9]  [Next >]
```

### Variants
- `pagination-full`: Numbers + prev/next (desktop)
- `pagination-simple`: Prev / Next only (mobile)
- `pagination-compact`: "Page X of Y" + prev/next

### States
- Default page: `neutral-200` bg, `neutral-900` text
- Active page: `primary-800` bg, `neutral-0` text
- Disabled (first/last): `neutral-400` text, no hover

---

## 4.4 Dropdown

**Purpose:** Select one option from a list or reveal contextual actions.

### Anatomy
```
[Label ▾]
┌─────────────┐
│ Option 1    │
│ Option 2    │  ← active
│ ─────────── │
│ Option 3    │
└─────────────┘
```

### States
- Closed: Standard select/button
- Open: Shadow-md panel, `z-dropdown`
- Item hover: `neutral-100` bg
- Item active/selected: `primary-800` text, checkmark icon
- Item disabled: `neutral-400` text

### Accessibility
- `role="combobox"` / `role="listbox"` / `role="option"`
- ESC to close, Arrow keys to navigate, Enter to select

---

## 4.5 Context Menu

**Purpose:** Right-click or kebab-menu actions on items.

### Anatomy
```
[⋮] click →
┌────────────────┐
│ ✏ Edit         │
│ 📋 Duplicate   │
│ ─────────────  │
│ 🗑 Delete      │ ← danger color
└────────────────┘
```

### Rules
- Max 7 items before scroll
- Danger actions always at bottom, separated by divider, `error-700` text

---

# 5. Form Components

## 5.1 Text Input

### Anatomy
```
[Label]  [Optional badge if not required]
┌─────────────────────────────┐
│  Placeholder text           │
└─────────────────────────────┘
[Helper text or error message]
```

### Sizes

| Size | Height | Font | Padding |
|---|---|---|---|
| SM | 32px | 14px | 0 12px |
| MD | 40px | 14px | 0 12px |
| LG | 48px | 16px | 0 16px |

### States

| State | Border | Background | Text |
|---|---|---|---|
| Default | `neutral-200` | `neutral-0` | `neutral-900` |
| Hover | `neutral-400` | `neutral-0` | `neutral-900` |
| Focus | `primary-800` 2px | `neutral-0` | `neutral-900` |
| Disabled | `neutral-200` | `neutral-100` | `neutral-400` |
| Error | `error-500` | `error-50` | `neutral-900` |
| Success | `success-500` | `neutral-0` | `neutral-900` |
| Read-only | `neutral-200` | `neutral-100` | `neutral-600` |

### Validation Rules
- Required fields: asterisk `*` in label
- Error message appears below field with `error-700` text + `⚠` icon
- Max-length counter shown at `80%` capacity

### Accessibility
- `<label>` always present and linked via `for`/`id`
- `aria-invalid="true"` on error state
- `aria-describedby` links to error/helper text

---

## 5.2 Password Input

Same as Text Input with:
- `type="password"` by default
- Toggle show/hide button (`👁` icon) at right end
- Password strength meter (below field, 4-step bar) on registration forms

---

## 5.3 Text Area

Same styling as Text Input with:
- `min-height: 120px`, resizable vertically
- Character counter shown bottom-right
- Auto-grow option (up to `max-height: 400px`)

---

## 5.4 Search Input

### Anatomy
```
🔍 [Search novels...              ] [✕ clear]
```

- Left icon: `🔍` search icon
- Right icon: `✕` clear button (shown when value present)
- Debounced input (300ms)
- Results dropdown if autocomplete enabled

---

## 5.5 Number Input

- HTML `type="number"` or controlled input
- `[−]` and `[+]` stepper buttons at left/right ends
- Min/max enforced with visual boundary cue

---

## 5.6 Date Picker

### Anatomy
```
[📅 Select date...    ]
```
- Calendar popup panel on focus
- Month/year navigation arrows
- Today highlighted with dot
- Selected date: filled `primary-800` bg

### Variants
- `date-picker`: Single date
- `date-range-picker`: Start + End date range

---

## 5.7 Select

### Anatomy
Single-value dropdown using native or custom overlay.

```
[Select category ▾]
```

Replaces plain `<select>` with keyboard-accessible custom dropdown. Supports:
- Placeholder text
- Clearable (optional `✕`)
- Searchable (filter-as-type for > 7 options)

---

## 5.8 Multi Select

Tag-style multi-select with inline chips:

```
┌──────────────────────────────────┐
│ [Action ✕]  [Romance ✕]   [▾]  │
└──────────────────────────────────┘
```

- Selected values shown as `badge-secondary` pills inside input
- Remove individual via `✕` on each pill
- Max selection enforced with warning toast

---

## 5.9 Checkbox

```
☑  Label text
```

- Size: 16×16px, `radius-xs`
- Checked: `primary-800` bg + white checkmark
- Indeterminate: dash indicator (for "select all" bulk actions)
- Disabled: `neutral-200` bg, `neutral-400` check

---

## 5.10 Radio Group

```
○  Option 1
●  Option 2  (selected)
○  Option 3
```

- Always rendered as a group (`role="radiogroup"`)
- Vertical stacking default, horizontal for ≤ 3 compact options

---

## 5.11 Toggle Switch

```
○────  OFF
────●  ON  (primary-800 bg)
```

- Size: 40×22px track, 18px thumb
- Animated slide transition (150ms ease)
- Labeled with before or after text
- `role="switch"`, `aria-checked`

---

## 5.12 Upload Field

### Anatomy
```
┌─────────────────────────────────────┐
│  🖼  Drag & drop or click to upload │
│  JPG, PNG, WEBP · Max 5MB           │
└─────────────────────────────────────┘
```

### States
- Default: Dashed border `neutral-300`, `neutral-50` bg
- Drag-over: `secondary-200` border, `secondary-50` bg
- Uploading: Progress bar inside zone + spinner
- Uploaded: Thumbnail preview + file name + `[✕ Remove]`
- Error: `error-200` border + error message

---

## 5.13 Rich Text Editor

Used for: Novel descriptions, Chapter content, Forum posts.

### Toolbar
```
[B] [I] [U] [S]  |  [H1] [H2]  |  [• List] [1. List]  |  [Link] [Image] [Code]  |  [Undo] [Redo]
```

### Editor Area
- `min-height: 200px`, auto-grows
- Monospace code blocks with syntax highlight
- Word count display in status bar

### Variants
- `editor-simple`: B/I/U + lists + link (comment boxes)
- `editor-full`: Full toolbar (chapter editor)

---

## 5.14 Form Validation Messages

### Inline Error
```
[Field]
⚠ This field is required.
```
- `error-700` color, 12px, appears on blur or submit
- `aria-live="polite"` for screen reader announcement

### Form-level Error Banner
```
┌────────────────────────────────────────────┐
│ ⚠  Please fix 3 errors before submitting. │
└────────────────────────────────────────────┘
```
- Appears at top of form on failed submit
- Auto-scroll to banner

---

# 6. Button Library

## 6.1 Variants

| Variant | Background | Border | Text | Usage |
|---|---|---|---|---|
| `primary` | `primary-800` | none | `neutral-0` | Main CTA |
| `secondary` | `neutral-0` | `neutral-200` | `neutral-900` | Alternate CTA |
| `outline` | transparent | `primary-800` | `primary-800` | Tertiary action |
| `ghost` | transparent | none | `neutral-600` | Navigation, inline |
| `link` | transparent | none | `secondary-700` | Inline links, text actions |
| `danger` | `error-600` | none | `neutral-0` | Destructive actions |
| `danger-outline` | transparent | `error-600` | `error-600` | Less prominent danger |
| `success` | `success-600` | none | `neutral-0` | Approval / positive action |
| `warning` | `warning-500` | none | `neutral-0` | Caution actions |

## 6.2 Sizes

| Size | Height | Padding | Font | Radius |
|---|---|---|---|---|
| `xs` | 24px | 0 8px | 12px/500 | `radius-sm` |
| `sm` | 32px | 0 12px | 14px/500 | `radius-sm` |
| `md` | 40px | 0 16px | 14px/500 | `radius-sm` |
| `lg` | 48px | 0 20px | 16px/500 | `radius-sm` |
| `pill-sm` | 32px | 0 16px | 14px/500 | `radius-pill` |
| `pill-md` | 40px | 0 20px | 14px/500 | `radius-pill` |
| `pill-lg` | 48px | 0 24px | 16px/500 | `radius-pill` |

Marketing CTAs use `pill-lg`. In-app actions use `sm`, `md`, `lg`.

## 6.3 States

| State | Behavior |
|---|---|
| Default | Base styling |
| Hover | 8% darker bg, cursor pointer |
| Active | 15% darker bg, slight scale down (0.98) |
| Disabled | `neutral-200` bg, `neutral-400` text, `cursor-not-allowed` |
| Loading | Spinner replaces label, button width locked, non-interactive |
| Focus | 3px `primary-300` outline offset 2px |

## 6.4 Button with Icon

```
[← Back]    [Save →]    [⊕ Add Novel]    [🗑]
```

- Icon 16px, gap 8px between icon and label
- Icon-only buttons always have `aria-label`

---

# 7. Data Display Components

## 7.1 Cards

### Novel Card

**Used in:** SCR-01, SCR-09, SCR-10, SCR-11, SCR-12, SCR-15, SCR-16

```
┌──────────────────────┐
│   [Cover Image]      │  ← 3:4 aspect ratio, radius-md top
│   16:9 thumbnail     │
├──────────────────────┤
│  [Status Badge]      │
│  Title (body-md-str) │
│  Author · Category   │
│  ★★★★☆ 4.2  (1.2k)  │
│  [Updated 2h ago]    │
└──────────────────────┘
```

**Hover:** Slight lift (`shadow-md`), cover image scale 1.03, transition 200ms.

### Forum Card

**Used in:** SCR-28, SCR-29, SCR-34, SCR-35

```
┌─────────────────────────────────────┐
│  [💬]  Category Name                │
│  Thread title (display-sm)          │
│  by @username · 2h ago              │
│  💬 32 replies  👁 512 views        │
└─────────────────────────────────────┘
```

### User Card

**Used in:** SCR-07, SCR-48, SCR-49

```
┌─────────────────────────────────────┐
│  [Avatar]  Username                 │
│            Role Badge               │
│            Member since Jan 2024    │
│  Novels: 12   Comments: 89          │
└─────────────────────────────────────┘
```

### Statistic Card

**Used in:** SCR-26, SCR-38, SCR-47, SCR-59

```
┌───────────────────────┐
│  Total Views          │
│  128,432              │  ← display-lg
│  ↑ 12% from last week │  ← success color for positive
└───────────────────────┘
```

---

## 7.2 Table

### Structure

```
┌───┬──────────────┬──────────┬─────────┬──────────┐
│ ☐ │  Title       │ Status   │ Updated │ Actions  │
├───┼──────────────┼──────────┼─────────┼──────────┤
│ ☐ │  Novel Name  │ [Ongoing]│ 2h ago  │ [⋮]      │
│ ☐ │  Novel Name  │ [Pending]│ 1d ago  │ [⋮]      │
└───┴──────────────┴──────────┴─────────┴──────────┘
```

### Sorting
- Clickable column headers with `↑` `↓` `↕` indicators
- Single-column sort at a time
- `neutral-100` bg on sorted column

### Filtering
- Filter icon in header → dropdown filter panel
- Active filters shown as removable pills above table

### Bulk Actions
When ≥ 1 row selected, action bar appears above table:
```
[☑ 3 selected]  [Approve] [Reject] [Delete] [Export]
```

### Row Actions
Context menu or inline action buttons:
```
[View] [Edit] [Delete]
```
or `[⋮]` kebab menu on mobile.

### Empty State
Centered empty state card when no rows match. (See §12)

### Loading State
Skeleton rows: each row shows shimmer bars for each column.

---

## 7.3 Badge

### Status Badges (Novel Status)

| Value | Color | Background |
|---|---|---|
| Draft | `neutral-600` | `neutral-100` |
| Pending | `warning-700` | `warning-100` |
| Ongoing | `success-700` | `success-100` |
| Ended | `secondary-700` | `secondary-100` |
| Hiatus | `warning-700` | `warning-100` |
| Dropped | `error-700` | `error-100` |
| Canceled | `neutral-400` | `neutral-100` |

### Role Badges

| Role | Color | Background |
|---|---|---|
| Guest | `neutral-500` | `neutral-100` |
| User | `secondary-700` | `secondary-100` |
| Staff | `warning-700` | `warning-100` |
| Admin | `error-700` | `error-100` |

### Category / Tag Badges
Neutral `neutral-600` text on `neutral-100` bg, `radius-pill`. Removable variant with `✕`.

### Anatomy
```
[● Status Label]
```
- Padding: `4px 10px`
- Font: `caption` 12px/500
- Radius: `radius-pill`
- Dot indicator optional for status badges

---

## 7.4 Tag

Lighter than badge. Used for categories and novel tags in catalog.

```
[Action] [Romance] [Fantasy] [+3 more]
```

- `neutral-100` bg, `neutral-600` text, `radius-xs`
- Clickable variant navigates to filtered catalog
- Removable variant (form context) has `✕` button

---

## 7.5 Avatar

| Size | Dimensions | Font Size | Usage |
|---|---|---|---|
| XS | 20×20px | 10px | Inline comment attribution |
| SM | 32×32px | 14px | Table rows |
| MD | 40×40px | 16px | Nav dropdown, card attribution |
| LG | 64×64px | 24px | Profile header |
| XL | 96×96px | 36px | User profile page |

- Shape: `radius-full` (circle)
- Fallback: Initials on `primary-800` bg if no image
- Border: `2px solid neutral-0` (stacked avatars)
- Online indicator: 10px green dot bottom-right (SCR-07)

---

## 7.6 Tooltip

```
         ┌──────────────────┐
         │  Tooltip content  │
         └────────┬──────────┘
                  ▼
              [Element]
```

- Trigger: `hover` or `focus`
- Delay: 200ms appear, immediate hide
- Max-width: 220px
- Background: `neutral-900`, text `neutral-0`
- `role="tooltip"`, `aria-describedby` on trigger

---

## 7.7 Divider

### Horizontal
`1px solid neutral-200` full-width

### Vertical
`1px solid neutral-200` in flex rows

### With Label
```
──────────── or ────────────
```
Used in auth forms to separate social login.

---

## 7.8 Progress Bar

Used for: reading progress, backup status, file upload.

```
──────────────────────────────
[██████████████░░░░░░░] 64%
──────────────────────────────
```

- Track: `neutral-200`, height `6px`, `radius-pill`
- Fill: `primary-800` default, `success-500` complete
- Animated: smooth transition on value change
- Label: optional percentage text beside or above

---

## 7.9 Rating Component

5-star rating with half-star support.

```
★★★★☆  4.2  (1,248 ratings)
```

### Interactive Variant (Form)
- Hover: Preview star fill
- Selected: `warning-500` filled stars
- `aria-label="Rate this novel: X out of 5 stars"`

### Display Variant (Read-only)
- Smaller stars (14px), `warning-400` color
- Rating number + count in `neutral-500`

---

# 8. Feedback Components

## 8.1 Alert

### Variants

| Variant | Icon | Border | BG | Text Color |
|---|---|---|---|---|
| Success | ✅ | `success-200` | `success-50` | `success-800` |
| Error | ❌ | `error-200` | `error-50` | `error-800` |
| Warning | ⚠️ | `warning-200` | `warning-50` | `warning-800` |
| Info | ℹ️ | `secondary-200` | `secondary-50` | `secondary-800` |

### Anatomy
```
┌──────────────────────────────────────────────────┐
│ ✅  Title text (optional)                  [✕]  │
│     Description message body text.               │
└──────────────────────────────────────────────────┘
```

- `radius-md`, full-width inside container
- Dismissible variant with `✕` close button
- Non-dismissible for critical errors

---

## 8.2 Toast

Ephemeral notification. Auto-dismisses after `4000ms`.

### Position: Bottom-right (desktop), Bottom-center (mobile)

### Anatomy
```
┌──────────────────────────────────────┐
│ ✅  Profile saved successfully.      │
│                              [✕]    │
└──────────────────────────────────────┘
```

- `shadow-lg`, `radius-md`
- Max-width: 380px
- Stacks vertically (newest on top)
- Pause on hover
- `aria-live="polite"` (success/info), `aria-live="assertive"` (error)

---

## 8.3 Confirmation Dialog

Blocking modal for destructive or irreversible actions.

### Anatomy
```
┌───────────────────────────────────┐
│  🗑 Delete Novel?                 │
│                                   │
│  This action cannot be undone.    │
│  All chapters will be deleted.    │
│                                   │
│            [Cancel] [Delete]      │
└───────────────────────────────────┘
```

- Cancel: `secondary` button
- Confirm: `danger` button
- ESC key closes (cancels)
- Backdrop click closes (cancels)
- Focus trapping inside modal

### High-Risk Variant (Restore/Delete Backup)
User must type a confirmation word (e.g. `RESTORE`) before the confirm button activates.

---

## 8.4 Error Dialog

For non-recoverable errors.

```
┌──────────────────────────────────────┐
│  ⚠ Something went wrong             │
│  Failed to save chapter content.     │
│  Error: 500 Internal Server Error    │
│                              [Retry] │
└──────────────────────────────────────┘
```

---

## 8.5 Success Dialog

For important completed actions.

```
┌──────────────────────────────────────┐
│  ✅ Novel submitted!                 │
│  Your novel is now under review.     │
│  We'll notify you within 48 hours.   │
│                            [Got it]  │
└──────────────────────────────────────┘
```

---

# 9. Overlay Components

## 9.1 Modal

**Max-width:** 520px (default), 720px (large), 900px (preview)  
**Background:** `neutral-0`  
**Radius:** `radius-lg`  
**Shadow:** `shadow-xl`  
**Backdrop:** `rgba(0,0,0,0.5)`, `z-modal`

### Variants

#### Form Modal
```
┌────────────────────────────────────┐
│  Modal Title                  [✕] │
├────────────────────────────────────┤
│  Form fields...                    │
├────────────────────────────────────┤
│                    [Cancel] [Save] │
└────────────────────────────────────┘
```

#### Confirmation Modal
See §8.3 Confirmation Dialog.

#### Preview Modal
Full-width content preview (e.g., novel cover preview, chapter preview).

### Rules
- Focus trapped inside modal while open
- ESC key closes
- `aria-modal="true"`, `role="dialog"`, `aria-labelledby` pointing to title
- Scroll lock on body while modal is open

---

## 9.2 Drawer

**Width:** 360px (right) or 280px (left/nav)  
**From:** Right (forms, filters) or Left (nav on mobile)  
**Backdrop:** Same as modal

### Variants
- `drawer-filter`: Right-side filter panel on mobile catalog
- `drawer-nav`: Left-side navigation on mobile
- `drawer-form`: Right-side form (create/edit in-place)

---

## 9.3 Popover

Small floating panel for additional info or quick actions.

```
[Trigger button]
       |
 ┌─────┴──────┐
 │  Content   │
 └────────────┘
```

- Max-width: 280px
- Auto-position (flips if near viewport edge)
- Dismiss on outside click or ESC

---

## 9.4 Dropdown Overlay

See §4.4. Full-featured overlay dropdown with search and multi-select support.

---

# 10. Content Components

## 10.1 Comment Component

### Anatomy
```
[Avatar] @username · 2h ago          [👍 12] [👎 2] [↩ Reply] [⋮]
         Comment content text goes here. Can be multi-line with
         proper line height for readability.
         
         └─ [Avatar] @reply_user · 1h ago   [👍 3] [↩ Reply]
                      Reply content text here.
```

### Features
- **Nested:** 1 level deep (reply to top-level comment)
- **Like / Dislike:** Toggle buttons with count
- **Reply:** Expands inline reply input below comment
- **Report:** `⋮` menu → "Report Comment" → opens Report Modal
- **Edit:** `⋮` menu → "Edit" → inline textarea replace (owner only)
- **Delete:** `⋮` menu → "Delete" → confirmation toast (owner/staff/admin)

### States
- Default, expanded replies, loading replies, deleted ("Comment removed by moderator")

---

## 10.2 Review Component

```
[Avatar] @username · ★★★★☆ · Jan 12, 2024
         Review title (optional bold)
         Review body text. Lorem ipsum...
         [👍 Helpful (24)]  [Report]
```

- Truncated to 3 lines with "Read more" expand link
- Rating stars with `warning-400` color

---

## 10.3 Novel Information Block

Full novel header used in SCR-12 and moderation preview panels.

```
┌──────────────────────────────────────────────────────────────┐
│  [Cover]  Title (H2)                                         │
│           by @Author · Category · [Ongoing]                  │
│           ★★★★☆ 4.2  ·  👁 128k views  ·  ♡ 3.2k likes      │
│           Tags: [Action] [Fantasy] [Isekai]                  │
│                                                              │
│           [Read Now]  [♡ Favorite]  [👍 Like]  [Report]     │
└──────────────────────────────────────────────────────────────┘
```

---

## 10.4 Chapter Reader

**Used in:** SCR-13

```
[← Back to Novel]      Chapter 12: The Dark Forest      [⚙]
─────────────────────────────────────────────────────────────

                    [Chapter Title]

    Body text in reading-optimized typography. Font size 
    adjustable (16–20px). Line height 1.75. Max width 
    720px centered. Generous vertical whitespace.

─────────────────────────────────────────────────────────────
[← Chapter 11]                               [Chapter 13 →]
```

- Font size control (3 steps: 16/18/20px)
- Theme toggle: Light / Sepia / Dark
- Progress indicator: reading % shown in header

---

## 10.5 Forum Thread Component

Full thread view for SCR-30.

```
[Avatar] @author_name · Forum Category  ·  Jan 5  [Edit] [⋮]
─────────────────────────────────────────────────────────────
Thread Title (H2)
[Flair: Discussion]  [Flair: Question]

Thread content (rich text rendered)...

─────────────────────────────────────────────────────────────
👍 128   💬 45 Replies   🔖 Save   ⚑ Report
```

---

## 10.6 Forum Post (Reply)

```
[Avatar] @replier · 2h ago                     [Edit] [⋮]
         Reply content text here...
         
         👍 12   ↩ Reply to this
```

---

## 10.7 Notification Item

**Used in:** SCR-36, SCR-37, Header bell dropdown

```
[🔔 / Type Icon]  Notification message text.         2h ago
                  Entity context (e.g., "Chapter 5 of Novel X")
```

| Type Icon | Usage |
|---|---|
| 📖 | NewChapter |
| 💬 | NewComment / CommentReply |
| ❤️ | CommentLike |
| 👤 | NewFollower |
| 🏅 | BadgeEarned |
| 📋 | ReportUpdate |
| 📢 | SystemAlert |

- Unread: `secondary-50` bg, bold text, blue left border (3px)
- Read: `neutral-0` bg, normal weight

---

# 11. Dashboard Components

## 11.1 Statistic Card

See §7.1 Statistic Card. Extended version:

```
┌────────────────────────────────┐
│  [Icon]  Metric Label          │
│                                │
│  128,432                       │  ← display-lg
│  ▲ +12.4%  vs last 30 days    │  ← success-600 for positive
│                                │
│  ▓▓▓▓▓▓▓▓░░ (sparkline)       │
└────────────────────────────────┘
```

---

## 11.2 Activity Timeline

Used in Moderation History (SCR-46) and Audit Logs (SCR-58).

```
│  ●  [Avatar] @staff · Approved novel "The Dragon War"  · 2h ago
│  ●  [Avatar] @staff · Rejected chapter · reason: ...   · 4h ago
│  ●  [System] Auto-flagged report #1204                  · 6h ago
│
[Load more]
```

---

## 11.3 Moderation Queue Card

**Used in:** SCR-38, SCR-39, SCR-41

```
┌──────────────────────────────────────────────────────┐
│  [Cover]  Novel Title                                │
│           by @author · Category                      │
│           Submitted: 2024-01-12  (3 days ago)        │
│                                                      │
│           [Review]  [Quick Approve]  [Quick Reject]  │
└──────────────────────────────────────────────────────┘
```

---

## 11.4 Analytics Widget

Wraps chart libraries (Chart.js / Recharts).

```
┌─────────────────────────────────────────────────────┐
│  Widget Title               [7d] [30d] [90d] [1y]  │
│  ─────────────────────────────────────────────────  │
│                                                     │
│        [Chart Area]                                 │
│                                                     │
└─────────────────────────────────────────────────────┘
```

Supports: Line chart, Bar chart, Pie/Donut chart, Sparkline.

---

## 11.5 Report Widget

**Used in:** SCR-43, SCR-57

```
┌──────────────────────────────────────────────────────┐
│  Report #1204  [Spam]  [Pending]                     │
│  "Novel: The War Chronicles"                         │
│  Reported by @user123 · 3 hours ago                  │
│                               [View] [Resolve]       │
└──────────────────────────────────────────────────────┘
```

---

# 12. Empty States

## No Data (Generic)

```
    [📭 Illustration]
    Nothing here yet
    It looks like there's nothing to show.
    [Refresh]
```

## No Search Results

```
    [🔍 Illustration]
    No results found
    Try different keywords or clear your filters.
    [Clear Filters]  [Browse All]
```

## No Notifications

```
    [🔔 Illustration]
    You're all caught up!
    No new notifications at this time.
```

## No Novels (My Novels Dashboard)

```
    [📚 Illustration]
    You haven't written anything yet
    Start your journey as an author today.
    [Create Your First Novel]
```

## No Reports (Staff)

```
    [✅ Illustration]
    No pending reports
    The queue is clear. Great work!
```

## No Forum Posts

```
    [💬 Illustration]
    No posts yet
    Be the first to start a discussion.
    [Create Thread]
```

### Empty State Rules
- Illustration: Line-art SVG, `neutral-300` stroke, 80–120px height
- Title: `display-sm`, `neutral-700`
- Description: `body-sm`, `neutral-500`
- CTA: `button-primary` (main action), `button-ghost` (secondary)
- Centered in available space, `padding-y: space-16`

---

# 13. Loading States

## Card Skeleton

```
┌──────────────────────┐
│  ░░░░░░░░░░░░░░░░░░  │  ← cover image shimmer
├──────────────────────┤
│  ░░░░░░░░░░░░        │  ← title
│  ░░░░░░░░            │  ← meta
│  ░░░░░░              │  ← rating
└──────────────────────┘
```

## Table Skeleton

Each table row shows shimmer bars matching the column widths:
```
│ ░░ │ ░░░░░░░░░░ │ ░░░░░░░  │ ░░░░ │ ░░░░ │
│ ░░ │ ░░░░░░░░   │ ░░░░░░░░ │ ░░░░ │ ░░░░ │
```
Typically 5–8 skeleton rows shown.

## Form Skeleton

Input outlines with shimmer placeholder text.

## Reader Skeleton

Paragraph lines of varying width (`60%`, `100%`, `85%`, `45%`, `100%`...), animating shimmer.

## Dashboard Skeleton

Stat cards: square shimmer blocks. Charts: rectangle shimmer.

### Skeleton Animation Rules
- `background: linear-gradient(90deg, neutral-100, neutral-50, neutral-100)`
- `animation: shimmer 1.5s ease-in-out infinite`
- Always match the shape/layout of the real content

---

# 14. Accessibility Standards

## WCAG 2.1 AA Compliance

### Color Contrast

| Usage | Minimum Ratio |
|---|---|
| Normal text (< 18px) | 4.5:1 |
| Large text (≥ 18px bold / ≥ 24px) | 3:1 |
| UI components & graphical elements | 3:1 |
| Decorative elements | No requirement |

Verified contrast pairs:
- `neutral-900` on `neutral-0`: 17.4:1 ✅
- `neutral-600` on `neutral-0`: 5.7:1 ✅
- `neutral-0` on `primary-800` (`#171717`): 15.5:1 ✅
- `error-700` on `error-50`: 5.2:1 ✅

### Keyboard Navigation

- All interactive elements reachable via `Tab` / `Shift+Tab`
- Logical focus order (left→right, top→bottom)
- Focus never trapped except in modals/dialogs
- Custom components implement `Arrow key` navigation (menus, tabs, radio groups)
- `Enter` / `Space` activate buttons and links

### Focus Indicators

```css
:focus-visible {
  outline: 3px solid #93c5fd;  /* secondary-300 */
  outline-offset: 2px;
  border-radius: inherit;
}
```

No component should remove the focus indicator without providing an equivalent.

### Screen Reader Support

- Semantic HTML elements: `<nav>`, `<main>`, `<article>`, `<aside>`, `<header>`, `<footer>`, `<section>`
- Landmark roles correctly applied
- Images have `alt` text (empty `alt=""` for decorative images)
- Form labels always associated with inputs
- Dynamic content uses `aria-live` regions
- Loading states: `aria-busy="true"` on loading containers
- Modals: `role="dialog"`, `aria-labelledby`, focus management

### ARIA Guidelines

| Pattern | ARIA Usage |
|---|---|
| Modals | `role="dialog"`, `aria-modal="true"`, `aria-labelledby` |
| Tabs | `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-selected` |
| Dropdown menus | `role="listbox"`, `role="option"`, `aria-expanded` |
| Toggle switch | `role="switch"`, `aria-checked` |
| Progress bar | `role="progressbar"`, `aria-valuenow`, `aria-valuemin`, `aria-valuemax` |
| Alerts | `role="alert"`, `aria-live="assertive"` for errors |
| Tooltips | `role="tooltip"`, `aria-describedby` on trigger |
| Breadcrumb | `<nav aria-label="Breadcrumb">` |
| Pagination | `<nav aria-label="Pagination">` |

---

# 15. Responsive Design Rules

## Breakpoints

| Name | Range | Container Max-Width |
|---|---|---|
| Mobile | < 768px | 100% − 24px gutters |
| Tablet | 768px – 1279px | 100% − 32px gutters |
| Desktop | 1280px – 1399px | 1280px |
| Wide | ≥ 1400px | 1400px |

## Grid System

- 12-column grid
- Desktop: 12 cols, 24px gutter
- Tablet: 8 cols, 16px gutter
- Mobile: 4 cols, 12px gutter

## Component Responsive Behaviors

| Component | Desktop | Tablet | Mobile |
|---|---|---|---|
| Header | Full nav inline | Full nav | Hamburger + drawer |
| Sidebar | 240px visible | 60px icon-only | Bottom nav or drawer |
| Novel Grid | 4-column | 2-column | 1-column |
| Table | All columns | Scroll horizontal | Card-per-row view |
| Modal | 520px centered | 90vw | Full-screen |
| Drawer | 360px from right | 360px from right | Full-screen from bottom |
| Tabs | Inline row | Scrollable row | Scrollable or accordion |
| Pagination | Full numbers | Simplified | Prev/Next only |
| Breadcrumb | Full path | Full path | Collapsed (last 2) |
| Filters | Sidebar | Sidebar | Drawer (bottom sheet) |
| Rich Text Editor | Full toolbar | Compact toolbar | Essential toolbar |
| Date Picker | Inline calendar | Inline calendar | Bottom sheet calendar |

---

# 16. Component Usage Matrix

| Component | SCR-01 | SCR-02 | SCR-03 | SCR-06 | SCR-09 | SCR-10 | SCR-12 | SCR-13 | SCR-18 | SCR-19 | SCR-23 | SCR-36 | SCR-38 | SCR-47 | SCR-48 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| NavBar | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Sidebar | — | — | — | ✓ | ✓ | — | — | — | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| NovelCard | ✓ | — | — | — | ✓ | ✓ | — | — | ✓ | — | — | — | — | — | — |
| Table | — | — | — | — | — | ✓ | — | — | ✓ | — | — | — | ✓ | ✓ | ✓ |
| TextInput | — | ✓ | ✓ | ✓ | ✓ | ✓ | — | — | — | ✓ | ✓ | — | — | — | ✓ |
| PasswordInput | — | ✓ | ✓ | — | — | — | — | — | — | — | — | — | — | — | — |
| RichTextEditor | — | — | — | ✓ | — | — | — | — | ✓ | ✓ | ✓ | — | — | — | — |
| Button Primary | ✓ | ✓ | ✓ | ✓ | ✓ | — | ✓ | — | ✓ | ✓ | ✓ | ✓ | ✓ | — | ✓ |
| Button Danger | — | — | — | — | — | — | — | — | ✓ | — | — | — | ✓ | ✓ | ✓ |
| Modal | — | — | — | — | — | — | ✓ | ✓ | ✓ | — | — | — | ✓ | ✓ | ✓ |
| Toast | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Badge (Status) | — | — | — | — | ✓ | ✓ | ✓ | — | ✓ | — | — | — | ✓ | — | ✓ |
| Avatar | ✓ | — | — | ✓ | ✓ | — | ✓ | ✓ | — | — | — | ✓ | — | — | ✓ |
| Pagination | — | — | — | — | ✓ | ✓ | ✓ | — | ✓ | — | — | ✓ | ✓ | — | ✓ |
| Tabs | — | — | — | ✓ | — | ✓ | ✓ | — | ✓ | — | — | ✓ | ✓ | ✓ | — |
| Breadcrumb | — | — | — | — | — | — | ✓ | ✓ | ✓ | ✓ | ✓ | — | ✓ | — | — |
| Rating | — | — | — | — | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — |
| CommentComponent | — | — | — | — | — | — | ✓ | ✓ | — | — | — | — | — | — | — |
| StatisticCard | — | — | — | — | — | — | — | — | ✓ | — | — | — | ✓ | ✓ | — |
| Alert | — | ✓ | ✓ | ✓ | — | — | — | — | — | ✓ | ✓ | — | — | — | — |
| Skeleton | ✓ | — | — | — | ✓ | ✓ | ✓ | ✓ | ✓ | — | — | ✓ | ✓ | ✓ | ✓ |
| EmptyState | — | — | — | — | ✓ | ✓ | — | — | ✓ | — | — | ✓ | ✓ | — | ✓ |
| SearchInput | ✓ | — | — | — | ✓ | ✓ | — | — | — | — | — | — | — | — | ✓ |
| Dropdown | — | — | — | — | — | ✓ | — | — | — | ✓ | — | ✓ | ✓ | ✓ | ✓ |
| ProgressBar | — | — | — | — | ✓ | — | — | ✓ | — | — | — | — | — | — | — |
| Tooltip | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| ConfirmDialog | — | — | — | — | — | — | — | — | ✓ | — | — | — | ✓ | ✓ | ✓ |
| NotificationItem | — | — | — | — | ✓ | — | — | — | — | — | — | ✓ | — | — | — |

> ✓ = Used on this screen. — = Not used. Matrix covers primary screens; all 64 screens inherit global components (NavBar, Toast, Tooltip, Skeleton).

---

# 17. Naming Conventions

## Components

PascalCase noun describing what it is.

| Type | Pattern | Examples |
|---|---|---|
| Entity Card | `{Entity}Card` | `NovelCard`, `UserCard`, `ForumCard` |
| Entity Table | `{Entity}Table` | `NovelTable`, `UserTable`, `ReportTable` |
| Entity Form | `{Entity}Form` | `NovelForm`, `ChapterForm`, `ProfileForm` |
| Entity List | `{Entity}List` | `NovelList`, `NotificationList` |
| Modal | `{Action}{Entity}Modal` | `CreateNovelModal`, `DeleteChapterModal`, `ReportModal` |
| Dialog | `{Purpose}Dialog` | `ConfirmDialog`, `SuccessDialog`, `ErrorDialog` |
| Page | `{Entity}{Action}Page` | `NovelDetailPage`, `UserProfilePage` |
| Section | `{Purpose}Section` | `HeroSection`, `FeaturedSection` |
| Widget | `{Purpose}Widget` | `StatWidget`, `ChartWidget` |
| Layout | `{Type}Layout` | `DashboardLayout`, `ReaderLayout` |

## CSS Classes / Tokens

kebab-case:
```
.btn-primary
.card-novel
.badge-status-ongoing
.form-input--error
```

## API-related UI

```
NovelApiCard         → Card that displays API-fetched novel data
NovelApiTable        → Table fed by novel API endpoint
ChapterApiReader     → Chapter content fetched from /api/chapters/{slug}
```

## File Structure Convention (Next.js / Razor Pages)

```
components/
  ui/              ← Primitives (Button, Input, Badge)
  layout/          ← AppShell, Header, Sidebar
  novel/           ← NovelCard, NovelForm, NovelTable
  chapter/         ← ChapterReader, ChapterForm
  forum/           ← ForumCard, ThreadView, PostItem
  moderation/      ← ModerationQueue, ReviewPanel
  admin/           ← StatCard, AuditLog, UserTable
  common/          ← EmptyState, Skeleton, Pagination
```

---

# 18. Implementation Notes

## Razor Pages (ASP.NET Core — LitNovel Frontend)

```
litnovel-frontend/
  Pages/
    Index.cshtml            → SCR-01 Landing
    Auth/
      Register.cshtml       → SCR-02
      Login.cshtml          → SCR-03
    Novel/
      Index.cshtml          → SCR-10 Catalog
      Detail.cshtml         → SCR-12
    Chapter/
      Reader.cshtml         → SCR-13
    Dashboard/
      Index.cshtml          → SCR-09 / SCR-18
  Shared/
    _Layout.cshtml          → App Shell
    _Header.cshtml
    _Sidebar.cshtml
    Components/
      NovelCard.cshtml
      Pagination.cshtml
      Alert.cshtml
```

### CSS Approach
- Vanilla CSS with custom properties (tokens):

```css
:root {
  --color-primary: #171717;
  --color-surface: #ffffff;
  --color-bg: #fafafa;
  --color-border: #ebebeb;
  --color-text-primary: #171717;
  --color-text-secondary: #4d4d4d;
  --color-text-muted: #888888;
  --color-link: #0070f3;
  
  --radius-sm: 6px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-pill: 9999px;
  
  --shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
  --shadow-md: 0 4px 6px rgba(0,0,0,0.04), 0 2px 4px rgba(0,0,0,0.04);
  
  --font-sans: 'Inter', system-ui, -apple-system, sans-serif;
  --font-mono: 'JetBrains Mono', ui-monospace, monospace;
}
```

### JavaScript
- Vanilla JS for interactive UI (dropdowns, modals, toasts)
- AJAX via `fetch()` for partial page updates
- No heavy framework required for v1.0

## If Using React / Next.js

```tsx
// Design token usage
const theme = {
  colors: { primary: '#171717', surface: '#ffffff' },
  radii: { sm: '6px', md: '8px', lg: '12px' },
};

// Component example
<Button variant="primary" size="md" loading={isSubmitting}>
  Save Changes
</Button>

// With TailwindCSS
<button className="bg-[#171717] text-white text-sm font-medium px-4 py-2 rounded-full hover:bg-[#2e2e2e] transition-colors">
  Sign Up
</button>
```

## With Ant Design (Admin Sections)
```tsx
import { Table, Button, Badge, Modal, Form } from 'antd';

// Override Ant Design tokens
const theme = {
  token: {
    colorPrimary: '#171717',
    borderRadius: 6,
    fontFamily: 'Inter, system-ui',
  },
};
```

## With Shadcn UI
```tsx
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";

// Shadcn components follow the same token system
```

## Performance Guidelines
- All images: `loading="lazy"`, WebP format, `srcset` for responsive
- Icons: SVG sprite or icon font (Lucide / Heroicons)
- Fonts: `font-display: swap`, preload critical fonts
- CSS: Critical CSS inlined, rest deferred
- JS: Progressive enhancement — core functionality without JS

---

*LitNovel UI Component Library — v1.0*  
*Generated from: `UI.md`, `screen.md`, `spec.md`, `screendesign.md`*
