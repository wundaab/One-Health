# One Health Innovator: art delivery handoff

This page tells the art team how to deliver generated or drawn art. Generate in any tool you like, using the prompts in `PROMPTS.md`. Deliver only as described here. **Do not put anything in `Assets/`, and do not open the Unity project.** The engineer imports accepted files.

## 1. Where to put the files

```
ArtDrop\<asset ID>\
```

- Make **one folder per asset ID** from `ASSET_LIST.md`, named exactly as the ID: `CHR-01`, `BG-03`, `MAP-01`, `GFX-05` and so on.
- Inside each folder:
  - candidate images and their `.json` sidecars go in the folder itself;
  - layered or vector sources go in `source\` (PSD, KRA, SVG, AI or Figma export);
  - anything you generated but don't recommend goes in `rejects\` (optional, but it helps the critic see what failed).
- Never overwrite a file that is already in the drop. Add a new version number instead.

## 2. File naming

```
<ASSET-ID>_<bible file name stem>_v<NN>.<ext>
```

| Asset type | Example |
|---|---|
| Character full body | `CHR-04_chr_farmer_full_neutral_v01.png` |
| Character bust | `CHR-04_chr_farmer_bust_neutral_v01.png` |
| Background | `BG-03_bg_farm_smallholding_v02.png` |
| Map base, markers | `MAP-01_map_s3_field_base_v01.png`, `MAP-02_map_marker_site_accessible_v01.svg` |
| Vignettes | `GFX-01_oh_vignette_animals_v01.png` |
| Medal, lock, logo | `GFX-02_badge_medal_base_v01.svg` plus `GFX-02_badge_medal_base_v01.png` |
| Sidecar | the same name as its image with `.json`: `BG-03_bg_farm_smallholding_v02.json` |

- Use lower case after the ID, underscores, no spaces and no accents.
- The expression words are exactly `neutral`, `concerned` and `positive`.
- The bible name stems are listed in `ART_BIBLE.md` §5.5.
- `vNN` counts up per file: `v01`, `v02` and so on.
- **All three expressions and all three busts in a character set share one version number.** For example, `v03` means neutral, concerned and positive all from the same approved neutral.

## 3. Formats, sizes and transparency

| Asset | Format | Size | Transparency | Notes |
|---|---|---|---|---|
| Character full body (CHR-*) | PNG, 8-bit RGBA | **1024×2048** | **Yes.** Straight alpha with clean edges: no white halo, no grey fringe, no baked-in shadow. | Feet on y = 1984, centred at x = 512, figure height from the bible table (§5.3). |
| Character bust | PNG, RGBA | **512×512** | Yes | **Crop it from the full body** using the recipe in `PROMPTS.md` §1.5, with the same rectangle for all 3 expressions. Never generate a bust separately. |
| Background (BG-*) | PNG, RGB | **2560×1440 master plus a 1920×1080 export** (deliver both) | No | Upscale by no more than 2× from the native output. No text anywhere. |
| Map base (MAP-01) | PNG, RGB | **1920×1200** | No | No text or markers on the base. |
| Map markers and routes (MAP-02) | **SVG** plus a PNG | 256×256 per marker; routes at map scale (1920×1200 canvas) | Yes | Flat vector. |
| One Health vignettes (GFX-01) | PNG, RGBA | **1024×1024** each | Yes, outside the circle | No ring, arrows or labels. |
| Medal, locks, emblems, icons (GFX-02, 03, 04, 06, 07, 08) | **SVG** plus a PNG | Medal 512×512; icons and emblems 256×256 | Yes | Built in vector. AI output is concept only. |
| Logo mark (GFX-05) | **SVG** plus a PNG | 1024×1024 | Yes | The emblem only. Leave out the wordmark text. |

- **Colour:** sRGB. Strip any embedded non-sRGB profile.
- **Never deliver JPEG or WebP.** If your tool only exports them, convert to PNG before anything else, and keep the original in `source\`.
- **No transparent output in your tool?** Generate on flat `#D8D8D8` grey, remove the background, then check the edge over both black and white before delivering.

## 4. How many variants to deliver

The goal is a small, curated choice, not a dump. Before delivering, check every candidate against Part A of `CRITIQUE_RUBRIC.md` (the gates) yourself.

| Asset type | Round 1 | Round 2 (after the art director picks) |
|---|---|---|
| **Characters** | **3 neutral full-body candidates** (`v01`–`v03`). No expressions or busts yet. | For the chosen neutral: **2 candidates each** for concerned and positive. Then deliver the final set: 3 full bodies plus 3 busts, all under **one** version number. |
| **CHR-01 Samuel** (the golden reference) | 3 candidates from **each** model you are considering (the bake-off in `PROMPTS.md` §1.6), using the same prompt and references | As for the other characters. This model is then locked for the whole cast. |
| **Backgrounds** | **3 candidates** | Up to 2 revisions of the chosen one (inpaint fixes, a crop, an upscale) |
| **BG-07** (a variant of the approved BG-02) | 2 candidates | 1 revision |
| **Map base, vignettes** | 2–3 candidates each (for vignettes, per vignette) | 1 revision |
| **Vector items** (medal, locks, logo, markers, emblems) | 2–3 concepts as PNG | 1 final as SVG plus PNG |

- At most **5 candidates** per file in any round. Put extras in `rejects\`.
- Work in priority order: P1 first (`ASSET_LIST.md` §4). **Deliver CHR-01 and BG-02 first and wait for approval before starting the rest**, because they become the style references for everything else.

## 5. Sidecar (`.json`, one per delivered image)

```json
{
  "asset_id": "BG-03",
  "file": "BG-03_bg_farm_smallholding_v02.png",
  "tool_and_model": "",
  "date": "2026-11-",
  "seed": "",
  "prompt": "the exact text sent",
  "negative_or_avoid": "",
  "reference_images": ["paths of any references used"],
  "post_processing": ["background removal: …", "upscale 2x: …", "inpaint: hand, left"],
  "self_check_gates_passed": true,
  "notes": ""
}
```

The critic adds the rubric scores to this same file during review.

## 6. Hard rules (an immediate reject, whatever the quality)

- Any readable or garbled text, letters, logos or watermarks.
- Any breach of the clinical guardrails: visible animal disease signs; water shown as the confirmed cause (no skulls, warning signs or "contaminated" labels); medicines shown as villains.
- Stereotyped or disrespectful depiction (`ART_BIBLE.md` §3).
- The player character shown speaking.
- Outfit or prop drift between a character's expressions.

## 7. When you're done with a round

Add a line to `ArtDrop\DELIVERY_LOG.txt`:

```
2026-11-05  CHR-01  round 1  v01-v03  ready for review
```

The art director reviews the round with `CRITIQUE_RUBRIC.md` and replies with one of: **ACCEPT**, **ACCEPT WITH FIX**, **REGENERATE** or **REJECT** for each file.
