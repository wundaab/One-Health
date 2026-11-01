# One Health Innovator: art bible

Version 1, 1 November 2026. Planning document for the character and scene art in Scenarios 1–4. Companion files in this folder: `ASSET_LIST.md`, `PROMPTS.md`, `CRITIQUE_RUBRIC.md` and `HANDOFF.md`, which covers delivery to `ArtDrop\`.

This bible is the reference for anyone who generates, paints or critiques art for the game. When a generated image and this document disagree, this document wins. When this document and the clinical guardrails (section 4) disagree, the guardrails win.

---

## 1. What exists and what we are unifying

I reviewed every character, background and illustration in `Assets/Art/`. The screen currently mixes five illustration styles:

| # | Style | Where | Quality | Verdict |
|---|---|---|---|---|
| 1 | Lush anime landscape with a glossy, bevelled "app icon" logo | `Assets/Backgrounds/mainMenuBackgroun.jpeg` (1600×731) | Attractive, but over-saturated, the logo is baked in, and the look is generic | Retire. Redo in the target style (P2). |
| 2 | Semi-realistic painted scenes with garbled signage | `02_Environments/Kivu_Clinic_Interior.png` (756×484), `Kivu_Community_Outdoor_Meeting_Area.png` (749×484) | Low resolution, garbled text, malformed hands on a poster, a sheep-goat hybrid, repeated crowd faces | Redo both in the target style. |
| 3 | **Clean-outline cel-shaded cast** | Amina, Dr. Malik, Samuel, Player, Caregiver, Child (all about 290 px tall) | Consistent designs, respectful and specific, readable silhouettes. However, they are crops from one AI reference sheet: low resolution, props change between expressions, some keep a white halo | **Base style. Keep the designs and redraw at production resolution.** |
| 4 | The same cel style with heavier ink, at high resolution | `02_Scenario_2/sector_contact/Sector_contact.png` (1121×1267) | The best-executed asset in the project: crisp, confident lines, correct hands, a natural face, a muted palette close to the UI | **Quality bar and style reference for rendering.** Its line weight is pulled back slightly to meet the cast. |
| 5 | Flat vector UI tiles and placeholders | HUD and `04_UI/Icons`, the field map placeholder, the medal and lock | Clean and on-palette, with a few construction errors | Keep for UI and icons. It is the UI layer, not an illustration style. |

### 1.1 Chosen style: "Kivu clean-line cel"

**One illustration style for characters and scenes. It takes the cast's designs and proportions (style 3) and renders them at the fidelity and line discipline of `Sector_contact.png` (style 4). The palette is pulled toward the UI tokens in `UITheme.cs`. Flat vector (style 5) remains the separate UI and icon layer, as it is now.**

Why this style:
- **It is already the majority.** Seven of the eight character sprites use it, as does the art team's own reference sheet (`07_Documentation/S01_Complete_Visual_Reference_Sheet.png`). Choosing anything else would mean redrawing the entire cast and would add a sixth style.
- **It has a proven high-quality example.** `Sector_contact.png` shows that this style can be generated cleanly at high resolution: five-finger hands, a believable face, no plastic skin. It also gives the critic a concrete benchmark.
- **It fits the UI.** The design system (`Assets/Art/01_Scenario_1/DESIGN_SYSTEM.md`) already asks for "thick consistent outlines in dark teal", flat fills and soft shapes. A cel illustration with a dark outline sits naturally next to the flat cards and Nunito type. A painted style does not.
- **It survives small sizes.** Outlined, two-tone figures stay readable in a 160 px circular bust. Painterly rendering turns to mush at that size.
- **It hides the AI tells.** Flat colour and clean line remove the airbrushed "plastic" skin and the micro-detail where AI images break down (signage, crowd faces, clutter).

The difference between styles 3 and 4 is mostly line weight and saturation. The bible sets one value for each (section 2), so redrawn cast members and new characters converge on a single look.

---

## 2. Style specification

### 2.1 Line

| Element | Characters (on a 1024×2048 canvas) | Backgrounds (at 1920×1080) |
|---|---|---|
| Outer silhouette | 7–9 px, Outline Ink `#2B2420` (warm near-black), slight taper at the ends | 2–3 px, a darker shade of the local colour (never black). Foreground objects only. |
| Interior lines (folds, features, pockets) | 3–4 px, the same ink, tapered | 1–2 px or none. Distant shapes have no line at all. |
| Facial features | 3–5 px. The eyes have a clear upper lash line. The nose is a minimal shadow shape, not a full outline. | n/a |
| Hair | Silhouette line plus 3–6 interior strand groups, never individual strands | n/a |

- One weight family across the cast. A character whose outline looks visibly heavier than Samuel's or Amina's redraw at the same display size fails the style check.
- Backgrounds use lighter, coloured lines and less contrast than characters. That separation ("characters pop, scenes sit back") is intentional, and it replaces the shader blur we are currently using to hide low-resolution backgrounds.

### 2.2 Shading and rendering

- **Cel shading: a flat base colour plus one hard-edged shadow tone.** The shadow is 15–22% darker than the base and shifted slightly cool (toward teal or violet), not grey.
- An optional third tone: a small, hard-edged highlight on hair, eyes, rubber boots and glass. No more than 5% of the figure.
- **No airbrushed gradients on skin or clothes.** A single very soft ambient-occlusion pass under the chin and arms is acceptable only if it is subtle at 1080p.
- Fabric pattern (kitenge, kikwembe) is drawn flat and follows the folds loosely. It does not need photographic accuracy, but the motif must be a coherent, repeating geometric or botanical pattern, not noise.
- **Backgrounds:** the same flat-plus-shadow logic, with simplified shapes. Skies are allowed one smooth gradient. Use atmospheric perspective: distant hills and the lake fade toward `#9DBBC8` and lose saturation.
- No visible paper texture, brush texture, film grain, bloom, lens flare or chromatic aberration.

### 2.3 Palette

The palette starts from the UI tokens in `Assets/Scripts/UI/UITheme.cs`, so illustration and UI share hues. The illustration-only colours extend it. They are not replacements.

**UI tokens (from `UITheme.cs`)**

| Token | Hex | Use in illustration |
|---|---|---|
| TealDark / Ink | `#1B423A` | Deep foliage shadow, line colour for dark-green objects, darkest background values |
| Teal | `#2E7D6B` | Brand accent. Painted wall bands in the clinic, team-base tarpaulin. |
| TealLight | `#A3D6C8` | Soft accents, reflected light on water |
| Cream | `#F7F2E7` | Light walls, paper, sky haze near the horizon |
| Sand | `#E8DDC4` | Light ground, khaki fabric, concrete in sun |
| Amber | `#F2A73B` | Warm accents: sunlight, jerrycans (shifted toward yellow, see below), headwrap motifs |
| Coral | `#D95A4F` | Sparingly: a garment accent or a flower. **Never** a warning colour in scenes (no red X, no red signs). |
| LeafGreen | `#4CAF7D` | Fresh crops and grass in sun |
| Navy | `#1F2A44` | The radio operator's polo. Deep shadow in interiors. |

**Illustration extension**

| Name | Hex | Use |
|---|---|---|
| Outline Ink | `#2B2420` | Character outlines |
| Laterite | `#B5653A` | Red-brown earth roads and paths, mud-brick |
| Laterite shadow | `#8A4A2B` | Shadowed earth, wet mud |
| Clay plaster | `#D9A877` | Plastered house walls |
| Iron roof | `#9AA3A8` / rust `#A0522D` | Corrugated roofs: mostly grey, some rusted |
| Banana green | `#5E8C3A` | Banana leaves, maize |
| Eucalyptus sage | `#8FA58A` | Eucalyptus and distant trees |
| Hill green, mid / far | `#6F9A5A` / `#A9C2A0` | Terraced hills by depth |
| Lake blue / far | `#4F86A6` / `#9DBBC8` | Lake Kivu near and far |
| Sky / overcast sky | `#D6E8EE` / `#C9CFD3` | Clear and rainy skies |
| Volcanic slate | `#5C6670` | Distant volcano silhouettes, wet rock |
| Jerrycan yellow | `#E8B634` | 20 L jerrycans (a slightly muted yellow) |
| Rubber-boot green / black | `#2F5E3E` / `#2A2A2A` | Gumboots |

**Skin tones (base / shadow).** Use a range across the cast. Never one shared tone, and never lightened.

| Ref | Base | Shadow |
|---|---|---|
| S1 deep | `#4A2E1F` | `#35201A` |
| S2 deep-warm | `#5A3825` | `#41281C` |
| S3 medium-deep | `#6B4429` | `#4F311F` |
| S4 medium | `#7D5235` | `#5D3C27` |
| S5 medium-warm | `#8F6142` | `#6B4730` |

**Saturation rule.** Base colours should sit at or below about 65% HSV saturation. The current Amina vest (a saturated royal blue) and the main-menu sky are the upper limit, not the norm. The UI should be the most saturated layer on screen, followed by the characters, with backgrounds the least saturated.

**Character identity colours.** Each speaker's costume carries the colour used for their name-tab accent in `ScenarioKit.NameColor`, so that colour, costume and name agree:

| Character | Identity colour | Carried by |
|---|---|---|
| Samuel | Amber `#F2A73B` | Hat band and belt buckle. His shirt stays green, as in the original design. |
| Farmer | Brown `#8A5A2B` | Work shirt and basket |
| Animal-health worker | LeafGreen family, deepened to `#3F6B4A` | Field vest and cap |
| Environmental officer | `#2F6FB0` | Rain jacket |
| Water collector | `#2F8FB0` | T-shirt |
| Radio operator | Navy `#1F2A44` | Polo shirt |
| Amina | Mid-blue `#3A78C2` | Health-worker vest. Kept from the original design, slightly desaturated. |
| Dr. Malik | White coat and blue shirt `#4A7FB5` | Coat |

### 2.4 Proportions and design

- **Grounded semi-realistic proportions.** Adults are 7–7.5 heads tall and the child (about 8 years old) is about 5.5 heads. No chibi, no anime eyes, no exaggerated hero anatomy.
- Faces: natural features with a recognisable bone structure and a varied nose, lip and jaw shape per character. **No "same face"**: put any two cast members side by side and each must be identifiable with the hair and clothes covered.
- Hands: simplified but always five fingers. A hand may be partly hidden by a prop or a pocket. Prefer poses where one hand holds something.
- Silhouette test: every character must be identifiable from a solid-black silhouette. Hats, headwraps, props and outfit shape do this work (see the outfit anchors in `PROMPTS.md`).
- Clothing is everyday and contemporary, clean and in good repair. Workwear looks used, not ragged.

### 2.5 Camera and framing

**Characters**
- Eye-level camera, straight-on to a slight three-quarter view (turned 10–25° toward screen centre). No up or down shots and no foreshortening.
- Full body: standing, relaxed weight shift, feet fully visible and flat on one ground line, nothing cropped.
- **Facing.** The master is drawn turned slightly toward screen right. The engineer can mirror sprites for stage-right placement **only if** the outfit is symmetrical. Characters with asymmetric details (lanyards, badges, bags) are mirrored only after the art director checks that no text or logo reads backwards.
- Busts are crops of the full body, never separate generations (section 6.3).

**Backgrounds**
- Eye level at standing adult height. The horizon or eye line sits at 40–48% of the image height (y ≈ 430–520 at 1080p).
- The camera is static and wide, with a 24–35 mm equivalent field of view. No fisheye, tilted horizons or extreme depth of field.
- Backgrounds are **stages, not scenes**: the area where characters stand is open and uncluttered. **No foreground people in backgrounds that also show speaking characters.** Crowds, where needed (meeting area, briefing), are mid-ground, small, seen from behind or in profile, with simplified faces.

### 2.6 Lighting

- One consistent key light: **daylight from the upper left**, warm-neutral (about 5200 K). Shadows fall to the lower right. Characters and backgrounds use the same direction, or characters look pasted on.
- **Characters are drawn once, under neutral daylight.** Mood (overcast Scenario 3, late-afternoon Scenario 4) comes from the engine colour grade already in `ArtPipeline.cs` (`OvercastGrade #E3E8EC`, `LateAfternoonGrade #FFE9CF`). Do not generate rain-lit or sunset-lit versions of characters.
- Backgrounds carry their own time of day (morning clinic, overcast farm, rain, late-afternoon briefing) but keep the light direction upper-left. In overcast scenes the light is soft but still comes from the left.
- No rim lights, god rays, dramatic chiaroscuro or coloured gel lighting.
- Contact shadows are added in the engine (`GeneratedSprites.SoftShadow`), so character PNGs have **no cast shadow** baked in.

### 2.7 Backgrounds versus characters

| Property | Characters | Backgrounds |
|---|---|---|
| Line | Dark, 7–9 px outer | Coloured, 2–3 px, foreground only |
| Contrast | Full value range | Compressed: no pure black, darkest value about `#1B423A`/`#35302B` |
| Saturation | Medium | Medium-low, falling with distance |
| Detail | Full detail on face and hands | Simplified. Detail falls off toward the edges and into the HUD and dialogue bands. |
| Text | None (blank badges and bags) | None at all (section 5.3) |

---

## 3. Representation guidance

The game is set in Kivu, a lakeside farming community in the Great Lakes highlands (the Lake Kivu basin on the Democratic Republic of Congo–Rwanda border). The art should feel **specific to this place, contemporary and dignified**. It should not read as generic "Africa", poverty imagery or a safari postcard.

### 3.1 People

- **Variety of skin tone, face shape, hair texture, age and build.** Use the skin table in 2.3. Hair: short-cropped, coily natural hair, braids, locs, twists, neat buns, headwraps. Ages from 8 to about 60. Several body types.
- **Gender balance across roles, against the stereotype.** In the planned cast the farmer is a woman who runs her own smallholding, the water collector is a young man, the environmental officer and radio operator are women, and the animal-health worker is a man. No role is gendered by default.
- **Professionals are shown competent and active.** They hold their tools (tablet, clipboard, radio, sample case), stand upright and gesture as people explaining something. They are colleagues, not helpers.
- **Community members are knowledgeable partners, not victims.** The farmer, the water collector and Samuel speak from expertise about their land, animals and water. Concern reads as attentive worry, never helplessness, fear or begging.
- **The child (Scenario 2) is tired but responsive.** No crying, visible illness signs or distress staging.
- **No outsider-saviour framing.** The player (Coordinator) is a young local professional, not a foreign expert. No foreign-aid branding, NGO logos or white vehicles with logos.
- **The player** (Coordinator) has no voiced dialogue and is never shown speaking. If the player appears at all, it is as a non-speaking figure seen from behind or the side, or as a map token.

### 3.2 Clothing

**Use:**
- Everyday contemporary dress of the region: button shirts, polos and T-shirts; fleece or zip jackets (the highlands are cool and wet); trousers and cargo pants.
- Wrap skirts in kitenge or kikwembe (pagne) prints; headwraps; cardigans.
- Rubber gumboots for farm and rainy-season work, plastic or leather sandals, trainers.
- Work vests, lab coats and blank ID lanyards for professionals.

**Avoid:**
- "Tribal" costume, face paint, beadwork as a default, Maasai shúkà red checks (a different region and people), animal-print fabric, safari khaki explorer outfits and pith helmets.
- Barefoot-poverty staging, torn or dirty clothing, and identical prints on several characters.
- Real organisation logos, flags or uniforms (no WHO, UN, Red Cross or government insignia), and military-looking gear.

### 3.3 Livestock

- **Cattle:** Ankole-type cattle (large lyre-shaped horns, red-brown, brown, or brown-and-white coats), with an optional brown crossbred dairy cow. Avoid Holstein black-and-white as the default "cow" (the current One Health diagram uses one), and never draw zebu humps exaggerated into cartoons.
- **Goats:** small East African goats with short coats, mixed brown, black and white, often tethered or in a raised wooden goat house. They must be unmistakably goats: beard, short upright tail, rectangular pupils not required at this size, **never sheep-goat hybrids**.
- **Chickens:** free-range village chickens with mixed brown, black and copper plumage and normal proportions. Not white broilers in cages.
- **Animals look calm and anatomically correct:** four legs, correct joints, two eyes, one tail.
- Animal welfare: no beating, no animals carried by the legs, no dead animals.
- **Clinical guardrail: section 4.**

### 3.4 Farms, water points and landscape

- **Landscape:** steep green hills with terraced plots (beans, maize, cassava, sorghum), banana groves, eucalyptus rows, the lake with wooded islands, and volcanic peaks far away (low prominence, hazed). Red-brown laterite roads. Rain clouds sitting on the hills.
- **Homes:** one-storey mud-brick or plastered houses (clay, ochre, whitewash, sometimes a painted lower band) with corrugated-iron roofs. Wooden doors, swept earth yards, simple pole fences, a small kitchen garden. Modest but cared for.
- **Smallholding:** a wooden-pole cattle enclosure (kraal), a raised wooden goat shed with a thatched or iron roof, chickens in the yard, a hand hoe leaning on a wall, a water barrel catching roof runoff.
- **Water points:** a protected spring (a concrete spring box with a metal spout and concrete apron), a communal tap stand or a borehole hand pump, with yellow 20 L jerrycans and a footpath. Use these water points, not open wells, ponds or "dirty stream" imagery.
- **Clinic:** a small public health centre with a painted lower-wall band (teal or blue), a concrete or tiled floor, wooden benches in the waiting area, a simple exam couch, metal cabinets, a handwashing station (sink or bucket with tap) and a solar-powered light. Neither a squalid ward nor a Western hospital.
- **Medicines:** shown as orderly, blank-labelled boxes on shelves and in cabinets. Ordinary, organised, cared for.
- **Not in this region:** savannah, acacia, lions, giraffes or elephants; desert; huts as the only housing; mud-splattered "crisis" imagery.

### 3.5 Names and language

- Do not invent visible names, brand names, place names or slogans in images. All words live in the UI (TextMeshPro), where they can be read, translated and corrected.

---

## 4. Clinical guardrails for art (from the producer and Dr Kakra's reviews)

These are **pass/fail**. Any breach rejects the image, however good it looks otherwise.

1. **Livestock symptoms stay non-specific.** No visible disease signs: no lesions, sores, discharge, swelling, diarrhoea, emaciation, bloating, flies on animals, blood, collapsed or dead animals, or vet procedures in progress. "Several animals are unwell" lives in the dialogue, not the picture. At most, one animal may be resting calmly on clean bedding.
2. **The water is not shown as the confirmed cause.**
   - No skulls, hazard or biohazard symbols, warning triangles, red X marks, or "danger"/"contaminated" signs or labels (in any language).
   - No dead fish, green slime, foam, oil sheen, chemical drums or effluent pipes.
   - No sick people near the water.
   - A water source **after rain** may show what residents observed: muddy runoff, puddles and a slightly cloudy, brownish tint in the catchment or stream. The spout or tap itself is not shown pouring brown water.
3. **Antimicrobials are appropriate care.** Medicines appear as ordinary, organised supplies in clinical settings. No monster pills, villain germs fighting pills, crossed-out medicine, skull-and-pill imagery, pills spilling everywhere, or syringes aimed at anything. No named or real drug packaging.
4. **People's illness is not dramatised.** The child is tired, not ill-looking. No rashes, vomit, blood, IV lines or crying. No stigmatising cues toward patients, caregivers or farmers (for example, the farmer's yard is not shown dirtier than others).
5. **Exposure is not proof.** The art must never imply a connection the evidence has not established. For example, no visual link (arrows, glows, dark streams) between the farm and the water source in any background. The One Health diagram shows the domains as connected systems, not a causal chain.
6. **No player speaking.** The Coordinator is never drawn mid-speech or in a dialogue pose.

---

## 5. Technical specifications

### 5.1 Screen layout and safe zones (1920×1080 reference)

From the current UI builders (`OneHealthUIBuilder.cs`, `FungusDialogStyler.cs`):
- **HUD band:** y 0–160, full width. Five stat chips across the top.
- **Dialogue band:** y 720–1080, x 300–1620. A 1280×290 white box sits 24 px above the bottom edge, and a 164 px circular bust portrait at its upper left (centred at about x 438, y 812) overlaps the top border.
- **Stage zone:** y 160–720. Full-body figures in Scenarios 1–2 stand at the left or right third, at about 770 px tall, with heads below y 160 (fixes U12).
- **Visual cards** (map, One Health, badges) appear in a centred white card, with an image area of about 900×440 to 960×600.

```
y=0    ┌───────────────────────────── HUD band (keep calm: sky, ceiling, foliage) ──┐
y=160  │                                                                          │
       │   [stage L]          focal zone (location identity lives here)  [stage R]│
       │   x 200–700                     x 640–1280                    x 1220–1720│
y=720  │        ┌───────────── dialogue band (floor, ground, low detail) ────────┐ │
y=1080 └────────┴────────────────────────────────────────────────────────────────┴─┘
```

### 5.2 Backgrounds

- **Master:** 2560×1440 PNG (16:9). **In-game export:** 1920×1080 PNG, RGB, no alpha. Never ship below 1920×1080. Never upscale more than 2× from the generator's native output, and use an illustration-aware upscaler when you do.
- **Composition:**
  - The location's identity (clinic desk, kraal, spring box, meeting tree) sits in the focal zone.
  - The **HUD band** holds low-contrast, low-detail content: sky, ceiling, upper foliage.
  - The **dialogue band** holds plain floor, ground or path.
  - The left and right stage areas are open floor or ground with no tall props, so standing characters do not merge with objects.
- **No readable text, letters, numbers, signage, posters with writing, clock numerals, screen content or logos.** Posters and charts are allowed only as blank paper or coloured blocks and simple shapes. A clock may appear only without numerals, or better, not at all.
- **No people in the stage zones.** Crowds appear only in the meeting and briefing backgrounds, mid-ground and small.
- Each background must be readable with the engine grade applied (`ClinicGrade #F4EEE6`, `OvercastGrade #E3E8EC`, `LateAfternoonGrade #FFE9CF`). Check that a slight multiply does not crush the shadows.

### 5.3 Characters

- **Full body:** transparent PNG (RGBA, straight alpha, clean anti-aliased edge, **no white halo or matte fringe**). **Canvas 1024×2048**, figure centred horizontally (x = 512), **feet on the baseline y = 1984**, no cast shadow.
- **Figure heights** (top of head or hair to the sole, on the 2048 canvas). Hats and headwraps add above this.

| Character | Height (px) | Character | Height (px) |
|---|---|---|---|
| Dr. Malik | 1800 | Environmental officer | 1730 |
| Samuel | 1790 | Amina | 1720 |
| Animal-health worker | 1780 | Radio operator | 1720 |
| Water collector | 1770 | Caregiver | 1710 |
| Player (Coordinator) | 1740 | Farmer | 1700 |
| Child | 1180 | | |

- **Unity import (for the engineer):** Sprite (2D and UI), pivot bottom-centre, the same pixels-per-unit for every character, Compression None or High Quality, mipmaps off. Then every character stands on the same line at the same scale.
- **Bust:** a **512×512** transparent PNG cropped from the full-body master (section 6.3). The face is horizontally centred, the eye line sits at 40–44% from the top, and the head plus hair fills 55–65% of the crop's height. The crop runs from just above the hair or hat to mid-chest. At 160 px display the eyes, brows and mouth must still read (see the rubric).
- **Expressions:** every speaking character has **neutral, concerned and positive**.
  - **Neutral:** relaxed, attentive, a slight closed-mouth smile.
  - **Concerned:** inner brows raised and drawn together, mouth slightly turned down or pressed. The gesture is thoughtful (hand to chin, arms loosely folded, a hand on the hip). Not fearful, angry or crying.
  - **Positive:** a warm, open smile, relaxed brows, an open-palm or thumbs-up gesture.
  - **Only the face, arms and hands change between expressions.** Outfit, colours, props, hairstyle, accessories, body proportions, camera and scale are identical. The legs and feet stay in the same stance so the bottom of the sprite doesn't jump when expressions swap.
  - Legacy names: Dr. Malik's "reassuring" is his positive. Amina's extra "welcoming" is optional (P3).

### 5.4 Graphics and UI art

- **Flat vector layer** (medal, lock, logo mark, map markers): the icon grammar in `DESIGN_SYSTEM.md`, meaning flat fills, one white glyph on a rounded-square tile where it is an icon, a `#1B423A` outline where outlined, no gradients or bevels. Masters as **SVG** where possible, exported to PNG at 512×512 (icons and medal) and 1024×1024 (logo mark).
- **Field map:** illustrated top-down base map without text, at 1920×1200 PNG. Markers and route lines are separate transparent PNG or SVG sprites, so the engine controls state (blocked, accessible) and the meaning never depends on generated pixels.
- **One Health diagram:** three circular vignettes (people, animals, environment), each a 1024×1024 transparent PNG. The arrows, ring and labels are built in the UI with TextMeshPro.

### 5.5 File naming and folder layout

Use lower-case snake_case with a type prefix. Characters recur across scenarios, so they live in one shared folder rather than per scenario.

```
Assets/Art/
  Characters/
    amina/                chr_amina_full_neutral.png, chr_amina_full_concerned.png, chr_amina_full_positive.png
                          chr_amina_bust_neutral.png,  chr_amina_bust_concerned.png,  chr_amina_bust_positive.png
    malik/  samuel/  caregiver/  child/  player/
    farmer/  water_collector/  animal_health_worker/  environmental_officer/  radio_operator/
  Backgrounds/
    bg_clinic_interior.png            bg_clinic_waiting.png
    bg_community_meeting.png          bg_community_briefing.png
    bg_farm_smallholding.png          bg_water_spring_after_rain.png
    bg_route_heavy_rain.png           bg_field_base.png
    bg_main_menu_landscape.png
  Maps/
    map_s3_field_base.png  map_marker_team_base.png  map_marker_site_accessible.png
    map_marker_route_blocked.png  map_marker_farm.png  map_marker_water.png  map_marker_community.png
    map_route_planned.png  map_route_alternate.png
  Graphics/OneHealth/
    oh_vignette_people.png  oh_vignette_animals.png  oh_vignette_environment.png
  UI/
    Badges/   badge_medal_base.png  badge_emblem_<badge_id>.png
    Icons/    icon_lock_closed.png  icon_lock_open.png
  Branding/
    logo_mark.svg  logo_mark_1024.png
ArtDrop/              <- at the repo root, OUTSIDE Assets, so Unity doesn't import it (see HANDOFF.md)
  <asset ID>/         delivered variants, sidecars (.json), source files, rejects/
```

- **Expression and crop suffixes are fixed:** `_full_` and `_bust_`, then `neutral`, `concerned` or `positive`.
- **Replacing existing assets:** builder scripts load some art by string path (`SC03Builder.MapPath`, `SC03/SC04Builder` background paths, `ScenarioKit.PortraitArt`). Either:
  - **(a)** overwrite the existing file in place, keeping the filename and `.meta`, so the GUID and every reference survive. Prefer this for the two Scenario 1 backgrounds and the field map.
  - **(b)** add the file at the new path and have the engineer update those constants in one pass. Do not move or rename existing files outside Unity.
- **Provenance:** every delivered image gets a sidecar `ArtDrop/<asset ID>/<file>.json` with the model or endpoint, date, seed, prompt, negative prompt, reference images, any post-processing (upscaler, background removal, retouch) and the critic's rubric score.

---

## 6. Production method (how to get consistency out of an AI generator)

1. **Lock the style first.** Redraw **Samuel (neutral)** first, using `Sector_contact.png` as the rendering reference and `samuel_neutral.png` as the design reference. Once it passes the rubric, it becomes the **golden reference** for every later character (along with `Sector_contact.png`). Then redraw Amina, then the five new characters, each referencing the golden image.
2. **Expressions by editing, not regeneration.** Generate and approve the **neutral** full body first. Make concerned and positive with an **image-editing** pass on the approved neutral ("same character, same outfit; change only the face and arms to…"). Use a fresh text-to-image generation only as a fallback, with the outfit anchor reused verbatim.
3. **Busts are crops.** Crop all three busts from the approved full bodies. Do not generate busts separately; a separate generation drifts in face and outfit. If the face at crop size is too soft (head under about 220 px tall in the source), run a **face-detail pass** (image-to-image on the head region at low strength), not a new bust.
4. **Transparency.** Use the generator's native transparent output if it has one. Otherwise generate on a flat, plain `#D8D8D8` background and run a background-removal model. Then check the edge against both black and white for halo, and remove any fringe.
5. **Text-free first, fix later.** If a background is excellent except for a stray glyph, inpaint the glyph area with plain wall or paper. Never ship "almost-letters".
6. **The critic reviews every image** with `CRITIQUE_RUBRIC.md` before import. One reviewer scores; the art director signs off P1.

---

## 7. Avoid list (AI-generated tells to reject)

**Anatomy and faces**
- Extra, missing, fused or bent-backwards fingers; hands melting into props; six-toed feet or shoes that merge.
- Warped or asymmetric faces: mismatched eyes, drifting pupils, smeared teeth, a doubled nose bridge or ear placement that changes between expressions.
- "Same face" across characters, or the one generic "AI-pretty" face.
- Plastic or waxy skin, airbrushed gradients, glossy highlights on skin, or lightened skin. Skin tones that shift between expressions.
- Limb length or joint errors: an elbow at the wrong height, knees missing, legs of different lengths.

**Consistency**
- Outfits that change between expressions: colour, pattern, pocket count, prop (tablet becoming clipboard), bag colour (the current caregiver's bag changes from blue to brown), hat shape, earrings.
- Proportions or scale that change between expressions. A figure height that does not match the height table.
- Light direction that differs from upper-left, or a different line weight from the golden reference.

**Text and symbols**
- Any readable or almost-readable text: garbled signage, pseudo-letters on posters, binders, screens, clocks, T-shirts, ID cards, caps or packaging.
- Fake logos, emblems with letters, watermarks, signatures, or artist marks in corners.
- Warning and hazard symbols, red crosses (the medical cross is a protected emblem), flags.

**Rendering**
- Over-saturation (neon greens and blues, an HDR look), bloom, lens flare, god rays, heavy vignettes.
- A mix of styles in one image (cel character on a painted background, a 3D-rendered prop).
- Painterly mush, smeared detail, upscaling artifacts, JPEG blocks, and noise passing for texture.
- Duplicated or tiled elements (repeated crowd figures, identical clouds, mirrored trees), and objects that merge into each other (benches into people, fences into animals).
- Impossible architecture: windows that do not line up, doors without frames, perspective that breaks.

**Content**
- Hybrid or malformed animals (sheep-goat blends, extra legs, a hen with a duck body), the wrong breeds (Holsteins as the default, white broilers).
- Region errors: savannah, acacia, safari animals, desert; Maasai dress; tropical beach palms on the lake.
- Anything in section 4 (clinical guardrails) or section 3 (representation).
- A presenter or "hero" figure in a background (it competes with the actual speaker).
