# One Health Innovator: critique rubric for generated art

Apply this rubric to **every** generated image before it is imported into Unity. It works with `ART_BIBLE.md` (the standard) and `ASSET_LIST.md` (what each asset is for). Record the result in the image's sidecar: `ArtDrop/<asset ID>/<file>.json` (see `HANDOFF.md`).

The rubric has two parts:

- **Part A** is a set of pass/fail gates. A single fail stops the review.
- **Part B** gives 1–5 quality scores.

Part C sets the accept, regenerate and reject thresholds.

---

## 0. Review procedure (about 5 minutes per image)

1. **At 100% zoom.** Check hands, faces, edges, fabric pattern, and any lettering or symbols. Zoom in on every sign, poster, bag, card and cap.
2. **The bible comparison.** Put the image side by side with the golden references:
   - characters: the approved `chr_samuel_full_neutral.png`, plus `Sector_contact.png` until Samuel is approved;
   - backgrounds: the approved `bg_community_meeting.png`.
3. **The 160 px bust test** (characters). Downscale the bust to 160 px inside a circular mask on Cream `#F7F2E7` with a 6 px Navy `#1F2A44` ring, which is what the dialogue box shows. View it at 100% on a 1080p screen.
4. **The 1080p composition test.**
   - Backgrounds: view at 1920×1080 with a layout overlay. Mark the HUD band at y 0–160, and the dialogue box at x 320–1600, y 766–1056, with the 164 px portrait circle centred near (438, 812).
   - Characters: place the figure at about 770 px tall at the left or right third of an approved background. Check that the head clears y 160.
   - Apply the scene's engine grade as a multiply (clinic `#F4EEE6`, overcast `#E3E8EC`, late afternoon `#FFE9CF`).
5. **The expression set test** (characters). Line up neutral, concerned and positive at the same scale, then flick between them as a GIF or by toggling layers. Only the face and arms may change.
6. **Score Parts A and B. Apply Part C.**

---

## Part A: pass/fail gates

Any **FAIL** means the image does not ship, whatever its scores. The *Outcome* column says whether to regenerate (or fix by inpainting) or to reject the approach.

| # | Gate | Fails if… | Outcome on fail |
|---|---|---|---|
| A1 | **Clinical guardrails** (bible §4) | Visible animal disease signs (lesions, discharge, emaciation, flies, collapsed or dead animals); water shown as the confirmed cause (skull, hazard or warning signs, "contaminated" or any label, dead fish, slime, effluent, brown water pouring from a tap); antimicrobials shown as villains or dangerous (monster pills, crossed-out medicine, skulls); a dramatised human illness (rash, vomit, blood, IV, crying child); any visual link implying the farm caused the water change, or the reverse | **Reject.** Rewrite the prompt before trying again. |
| A2 | **Respect and representation** (bible §3) | A stereotyped or "tribal" costume, poverty or "aid poster" staging, lightened or uniform skin tone, a demeaning pose, a saviour framing, the wrong region (savannah, safari animals, Maasai dress), real organisation logos or flags, a red cross emblem | **Reject** |
| A3 | **No text** | Any readable or almost-readable lettering, numbers, pseudo-glyphs, clock numerals, signatures, watermarks or logos containing letters, anywhere in the image | **Regenerate.** If the text is confined to one small area that can be repainted with plain surface, inpaint it and re-check. |
| A4 | **Hands and anatomy** | Any hand with more or fewer than five fingers, fused or bent-back fingers; extra or missing limbs; a broken joint; feet merging with each other or the ground; animal anatomy errors (extra legs, hybrid species) | **Regenerate**, or inpaint a single hand or foot if the rest scores 4 or more |
| A5 | **Identity** | The character does not match their OUTFIT ANCHOR: wrong garment, colour, prop, hairstyle, skin tone or age; or a redraw (Samuel, Amina, Malik, Caregiver, Child) is not recognisable as the original | **Regenerate** |
| A6 | **Technical spec** | Characters: not RGBA, a halo or fringe on the edge, a cast shadow baked in, feet off the y = 1984 baseline, or the wrong figure height (more than ±2% off the table). Busts: not 512×512 or off the shared crop rectangle. Backgrounds: below 1920×1080 or upscaled more than 2×. The wrong aspect ratio. | Fix in post-processing if possible; otherwise **regenerate** |
| A7 | **Player rule** | The Coordinator (player) is drawn speaking or in a dialogue pose | **Reject** |
| A8 | **Staging** (backgrounds) | A person in the foreground or the stage zones, a presenter or "hero" figure, or a crowd with visible repeated faces | **Regenerate** |

---

## Part B: quality scores (1–5)

**Scale:**
- **5:** indistinguishable from good hand-made production art.
- **4:** ship-quality, with at most a trivial nitpick.
- **3:** acceptable for a prototype, but visibly weaker than the reference.
- **2:** clearly flawed; a player would notice.
- **1:** broken or off-brief.

"n/a" is allowed where a criterion does not apply (for example, faces on a map).

| # | Criterion | Applies to | 5 looks like | 1 looks like |
|---|---|---|---|---|
| B1 | **Style match to the bible** (§1.1, §2.1–2.2) | All | Line weight, cel shadow and palette indistinguishable from the golden reference; it would pass as the same artist | A different style: painterly, anime, 3D, photoreal, or the glossy 2010s app look |
| B2 | **Anatomy and proportions** | Characters, animals | Believable 7–7.5 head proportions (child 5.5); correct joints; a natural weight shift; animals correct for their breed | Distorted limbs, a wrong head size, rubbery bodies |
| B3 | **Face quality** | Characters, vignettes with people | A specific, natural face with symmetric eyes, a clear expression and a distinct identity from the other cast members | Warped, waxy or "same face"; drifting eyes; smeared mouth |
| B4 | **Hands** (once A4 passes) | Characters | Clean, well-constructed hands, gripping props convincingly | Passes the finger count but the hands are mushy, too small, or holding props unconvincingly |
| B5 | **Text-artifact cleanliness** (once A3 passes) | All | Surfaces where text might appear (posters, cards, bags, packaging) are deliberately blank or abstract | Faint scribbles or texture that suggests writing |
| B6 | **Cultural accuracy and specificity** | All | Reads unmistakably as the Lake Kivu highlands: the right housing, crops, water point, livestock breeds and contemporary clothing; dignified | Generic "Africa", or details from another region |
| B7 | **Clinical tone** (once A1 passes) | Scenes, vignettes, characters with props | Calm, factual, non-alarming; medicines ordinary; the "after rain" observation subtle | Technically within the rules but alarming in mood (ominous lighting, menacing water) |
| B8 | **Readability at 160 px** | Character busts | Eyes, brows and mouth read clearly; the expression is identifiable without the name; the silhouette is distinct in the circle | A muddy face; the expression unreadable; the hat or hair dominates |
| B9 | **Readability at 1080p** | Backgrounds, full bodies, map, vignettes | Crisp at full screen; the focal point obvious in under 2 s; clear depth layers | Soft or mushy; cluttered; nothing reads |
| B10 | **Composition with the HUD and dialogue box** | Backgrounds, full bodies | The HUD band and dialogue band are calm, the stage zones open, and the focal content sits in y 160–720; a figure's head clears the HUD; nothing important is hidden under the box | Key content under the HUD or box, or busy detail fighting the UI |
| B11 | **Palette** (§2.3) | All | Hues sit within or harmonise with the bible palette; medium or medium-low saturation; the light direction is upper-left | Over-saturated, neon, off-palette, or the wrong light direction |
| B12 | **Consistency across a character's expressions** | Character sets (score once per set) | Outfit, props, colours, skin, scale, legs and feet identical; only the face and arms change; busts align | Any drift: a colour shift, a changed prop (tablet to folder), a different bag, a scale jump |
| B13 | **Consistency with the cast or scene set** | All | Sits naturally beside the already-approved assets (line weight, light, level of detail) | Clearly from a different batch |

---

## Part C: decision thresholds

Work out the **average** of the applicable B-scores, and note the **minimum**.

| Decision | Rule | Action |
|---|---|---|
| **ACCEPT** | All Part A gates pass, **and** the average is 4.0 or more, **and** no applicable criterion is below 3, **and** B1 (style) is 4 or more. For characters, B3 (face), B8 (160 px) and B12 (set consistency) are also each 4 or more. For backgrounds, B10 (composition) is 4 or more. | Import. Note any 3-scores as known nitpicks in the sidecar. |
| **ACCEPT WITH FIX** | Meets ACCEPT except for one localised defect that can be repaired in under 15 minutes: one hand inpaint, one text patch, an edge clean-up. | Fix, then **re-score A3, A4 and A6**, then import. |
| **REGENERATE** | Gates A3, A4, A5, A6 or A8 fail and can't be fixed locally; **or** the average is 3.0–3.9; **or** any applicable criterion scores 2. | Re-run with the same prompt and a new seed (for up to 4 attempts). Then adjust the prompt (add the failed item to the negatives or the Avoid line, strengthen the anchor, or swap the reference). After 8 failed attempts in total, escalate to the art director. |
| **REJECT** | Gate A1, A2 or A7 fails; **or** the average is below 3.0; **or** B1 (style) is 2 or less; **or** any applicable criterion scores 1. | Discard the result, revise the prompt or approach (§6 of the bible), and log the reason so the same failure is not repeated. If the same asset fails the same way three times, flag it for a human illustrator. |

**Set rule (characters).** A character ships only when **all three** expressions and all three busts are accepted. If one expression cannot reach ACCEPT within its attempt budget, regenerate the set from the approved neutral rather than mixing generations.

**P1 sign-off.** Every P1 asset needs a second reviewer, or the art director, to confirm the ACCEPT decision.

---

## Score sheet template (copy into the sidecar)

```json
{
  "asset_id": "CHR-04",
  "file": "chr_farmer_full_neutral.png",
  "reviewer": "",
  "date": "",
  "gates": { "A1": "pass", "A2": "pass", "A3": "pass", "A4": "pass", "A5": "pass", "A6": "pass", "A7": "n/a", "A8": "n/a" },
  "scores": { "B1": 0, "B2": 0, "B3": 0, "B4": 0, "B5": 0, "B6": 0, "B7": 0, "B8": 0, "B9": 0, "B10": 0, "B11": 0, "B12": 0, "B13": 0 },
  "average": 0.0,
  "minimum": 0,
  "decision": "ACCEPT | ACCEPT WITH FIX | REGENERATE | REJECT",
  "notes": "",
  "fix_applied": ""
}
```

## Quick reference: criteria by asset type

| Asset type | Gates | Scores |
|---|---|---|
| Character full body | A1–A7 | B1–B7, B9–B13 |
| Character bust | A3, A4, A6 | B3, B8, B11, B12 |
| Background | A1–A3, A6, A8 | B1, B2 (if animals), B5–B7, B9–B11, B13 |
| Map base and vignettes | A1–A4, A6 | B1, B2 (animals), B3 (people), B5, B6, B9, B11, B13 |
| Vector items (medal, lock, logo, markers) | A1–A3, A6, plus a construction check (symmetry, closed shapes, centring) | B1 (matches the tile-icon grammar), B9, B11 |
