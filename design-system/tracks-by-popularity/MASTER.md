# Design System Master: tracksByPopularity

> Generated from ui-ux-pro-max (entertainment bold) with palette overrides — no purple.

## Product
- **Name:** TracksByPopularity
- **Type:** Music library tool (Spotify-integrated)
- **Style:** Dark Mode OLED + Vibrant/Block accents
- **Mode:** Dark only

## Colors

| Role | Hex | Token / usage |
|------|-----|----------------|
| Background | `#000000` | `bg-black` |
| Surface | `#121212` | `--color-surface` |
| Elevated / Card | `#1A1A1A` | `--color-elevated` |
| Highlight | `#242424` | `--color-highlight` |
| Foreground | `#F8FAFC` | `--color-foreground` |
| Muted | `#A1A1AA` | `--color-muted` |
| Accent / CTA | `#22C55E` | `--color-spotify` |
| CTA hover | `#16A34A` | `--color-spotify-dark` |
| Secondary accent | `#FFCE5C` | `--color-accent` (badges only) |
| Destructive | `#EF4444` | `--color-destructive` |
| Focus ring | `#FFFFFF` | `ring-white` |

Glow: `0 0 24px rgb(34 197 94 / 0.35)` on primary CTAs.

## Typography
- **Display / brand / page titles:** Righteous (`font-display`)
- **Body / UI:** Poppins (`font-sans`)
- **Google Fonts:** `family=Poppins:wght@300;400;500;600;700&family=Righteous`

## Effects & Motion
- Entrance: fade-up 250–400ms, stagger 40ms steps
- Hover: 150–300ms color/shadow transitions
- Subtle green glow on primary buttons and brand mark
- Respect `prefers-reduced-motion: reduce`

## Icons
- Library: Lucide (existing)
- No emoji as structural icons (use `Music2` / `AudioLines` for brand)

## Spacing & Density
- Standard/tool density: 8dp rhythm, cards `rounded-2xl`, touch targets ≥44px (`min-h-11`)

## Anti-patterns (avoid)
- Purple / indigo theme
- Emoji icons
- Removing focus outlines without replacement
- Heavy GSAP on dense tables
- Light mode

## Stack
- React 19 + Vite + Tailwind CSS v4
- Tokens live in `frontend/src/index.css` `@theme`
