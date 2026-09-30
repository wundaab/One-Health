# One Health Innovator — Scenario 2 Asset Pack
### "The Fever That Won't Wait"

53 assets extending the Scenario 1 visual system. Same palette, corner
radii, shadow treatment, and icon grammar as the S01 pack — nothing here
introduces a new visual style; it's built with the same style module
used for S01.

---

## 1. What's here, by section (matches the S02 brief numbering)

1. **Characters** — Caregiver (neutral/concerned/reassured) and Child
   (neutral/tired), transparent PNGs, same proportions and construction
   as the S01 cast. See the honest caveat below.
2. **Case Board** — board panel + 3 evidence-card states (not found /
   found / revealed) + 4 evidence-type icons. State is communicated
   entirely through color/outline/badge, not text.
3. **Diagnostic** — panel shell + 3 process-state strips (requested /
   processing / result available) + result card shell + "What Changed?"
   before/after shell. No clinical content of any kind is included —
   that's intentional per the brief.
4. **One Health Board** — circular board background + 3 distinct nodes
   (People/Animals/Environment) + connection line (straight + curved +
   an "active" accent variant) + a small evidence-indicator dot, all as
   separate layers so Unity can reveal connections dynamically instead
   of them being baked into one background.
5. **Alerts** — 3 notification shells (New Lead / Result Available /
   Capacity Low), same navy-panel-plus-accent-bar language as the S01
   notification, just distinguished by accent color and icon.
6. **HUD** — 3 Response Capacity icons (normal/low/critical) plus a
   stat background that matches `S01/01_UI/HUD/stat_background.png`
   exactly, so it drops into the existing HUD row.
7. **Final Allocation** — one reusable action-card template in 4 states
   (normal/hover/selected/disabled). Built as a single template because
   all four action cards (follow-up / coordinate / investigate /
   preserve) share the same shape — Unity places different icon +
   text on the same template per card.
8. **Decision Review** — panel background + one reusable section-card
   shell + 5 distinct section icons (Your Path, What Happened, Why It
   Mattered, What Changed, Try Differently).
9. **Reflection** — panel + a 3-state selectable option button
   (normal/hover/selected) + a replay button.
10. **General UI States** — three small overlay assets (lock, active
    glow ring, completed checkmark) designed to sit on top of *any*
    existing S01 or S02 button/card rather than duplicating full
    button art per color. Normal/Hover/Pressed/Disabled/Selected
    already exist as full button states in the S01 pack
    (`01_UI/Buttons/`) — reuse those directly; these three overlays
    cover the states S01 didn't need (Locked/Active/Completed).

## 2. No baked content — verified

Every text-bearing surface in this pack (evidence cards, diagnostic
result card, alerts, allocation cards, review sections, reflection
options) ships as an empty shell with placeholder icon circles and
placeholder text bars only — no dialogue, names, numbers, medical
content, or button labels are drawn into any file. Drop a
TextMeshPro/Image component on top in Unity for each.

## 3. Honest caveat — Caregiver & Child characters

Same caveat as the S01 pack: these are clean flat-vector stand-ins,
not a match to fully-illustrated character art. They're built with the
same construction and proportions as the S01 cast so they'll sit
correctly next to Amina/Dr. Malik/Samuel in dialogue scenes, and they
follow the "non-graphic, appropriate for an educational game" brief
(no depiction of illness/injury) — but if S01's final characters end
up being commissioned/AI-illustrated art rather than this flat style,
these two should be redone to match at the same time.

## 4. Unity usage notes

- Import everything as **Sprite (2D and UI)**.
- Case Board / Final Allocation / Reflection cards: good 9-slice
  candidates (flat interior, generous corner radius).
- `connection_line_straight.png` / `_curved.png`: designed to be
  individually rotated/scaled/tinted in Unity rather than as one fixed
  background — that's why they ship as separate small assets instead
  of being drawn into `board_background.png`.
- `active_glow_overlay.png` is semi-transparent gold — place behind or
  around a card/button and it won't fight with the card's own shadow.

## 5. Consistency check against S01

Reused directly from the S01 pack (not redrawn): button state system
(`Buttons/`), general panel/shadow language, notification-panel
layout, icon tile size (128px) and corner radius (26px), palette
(`pack_style.py` — same file, same values).
