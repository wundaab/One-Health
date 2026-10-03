# One Health Innovator — Visual Design System

Flat, clean, friendly, educational style. Rounded shapes, soft drop shadows, no gradients beyond flat fills, thick consistent outlines (4–8px) in dark teal.

## Palette
| Role | Hex | Use |
|---|---|---|
| Teal Dark | #1B423A | outlines, primary text |
| Teal | #2E7D6B | primary brand, health worker/player color |
| Teal Light | #A3D6C8 | soft accents, secondary community members |
| Cream | #F7F2E7 | app/background |
| Sand | #E8DDC4 | ground, neutral progress-bar track |
| Amber | #F2A73B | warmth, warning, resources |
| Coral | #D95A4F | negative / alert / patient accent |
| Leaf Green | #4CAF7D | positive / success |

## Typography
DejaVu Sans Bold for headings/labels, DejaVu Sans (regular) for body copy. Swap for the project's chosen game font later — just keep the same weight contrast (bold headers, regular body).

## Shape language
- Corner radius: 8–14px for small UI elements, 20–28px for cards/panels, full circles for badges/avatars/icons.
- Outlines: 4–8px, always Teal Dark, on characters and interactive elements.
- Shadows: soft, ~15px blur, offset (0, 8), used only on elevated UI (cards, badges, banners) — not on environment art.

## Icon grammar
240x240px rounded-square tile, flat color background, single white glyph, centered. One concept per icon, no text baked in unless unavoidable (AMR/warning use minimal shorthand).

## Files delivered
See folder tree in the top-level README for exact paths.

## Known gaps / next steps for whoever wires this into Unity
- Sprites are plain PNG with alpha where needed (characters, icons, reactions). Import as **Sprite (2D and UI)**, not Texture, so Unity generates the right mesh/atlas behavior.
- Decision card / feedback / unlock / results assets are reference **templates** (flattened PNG mockups), not sliced UI prefabs — turn them into actual Canvas prefabs using Unity's own Image/TextMeshPro components so text stays editable and localizable, using this file's palette/type for consistency. Baking the layout into a single PNG works for a prototype but won't scale to real dynamic content.
- Reaction badges are a single reusable icon set (not per-character), meant to appear next to/above a character sprite when their state changes — cheaper than 4 characters × 8 expressions and keeps the reusability the brief asked for.
- For 9-slice scaling of the card/panel backgrounds in Unity, you'll get a cleaner result exporting a small flat rounded-rect sprite with sliced borders rather than reusing the flattened mockups directly.
