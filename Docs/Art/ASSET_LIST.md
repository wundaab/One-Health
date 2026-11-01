# One Health Innovator: asset list

This list covers every character, scene and illustration asset that the four scenarios need and that is missing, still a placeholder, or flagged for redo. Read it with `ART_BIBLE.md`, which sets the style, specs and naming, and `PROMPTS.md`, which has the generation prompts.

**Scenario numbering.** This list uses the game's numbering:
- S1 is the clinic introduction.
- S2 is "The Fever That Won't Wait".
- S3 is "The Warning Signs". Its blueprint is labelled S04.
- S4 is "Kivu's Turning Point". Its blueprint is labelled S05.
- Activity codes (A1–A8) come from the blueprints.

**Priority**
- **P1:** needed for the pitch build. These are visible in every playthrough or replace the worst AI tells.
- **P2:** needed for a complete, consistent release.
- **P3:** polish or optional.

**Status**
- **Missing:** no art exists.
- **Placeholder:** an icon or programmer art stands in.
- **Redo:** art exists but fails the bible (resolution, text, style or consistency).

---

## 1. Characters

Every character is delivered as **3 full-body PNGs** (neutral, concerned and positive; 1024×2048, transparent, on the baseline) plus **3 busts** (512×512, transparent) cropped from them. That makes 6 files per character. Generate the full bodies, then crop the busts. Never generate busts separately.

| ID | Character | Where it appears | Status | Priority | Description |
|---|---|---|---|---|---|
| CHR-01 | **Samuel**, community representative and farmer | S1; S3 A2 and A6 (community voice); S4 A1, A2 and A6 | Redo: 290 px crops, and skin tone and hat brim change between expressions. He is the sole recurring community representative in S3–S4. | **P1** | Same design: a man in his 40s with a woven straw hat and amber band, a green work shirt, khaki cargo trousers and green gumboots. **Generate first: this becomes the golden style reference.** |
| CHR-02 | **Amina**, community health worker | S1; S2 E01; S3 A2, A3 and A7; S4 A2, A4 and A7 | Redo: 290 px, and her prop changes from a tablet to a folder between expressions | **P1** | Same design: a young woman with long curly hair, a blue headband, a white tee, a mid-blue health-worker vest, dark denim cargo trousers and white trainers, always holding a tablet. |
| CHR-03 | **Dr. Malik**, clinician | S1; S2 | Redo: 290 px, his prop changes between expressions, and the HUD covers his head on stage (U12) | **P1** | Same design: a man of about 40 with a trimmed beard, round dark glasses, an open white coat, a blue shirt, a stethoscope and a clipboard. His "reassuring" expression maps to positive. |
| CHR-04 | **Farmer** (new) | S3 A2 and A4; S4 A3 and A6. One of the most frequent speakers (about 10 lines per scenario). | Placeholder: `animals_icon.png` | **P1** | A woman in her 50s who runs a mixed smallholding. Rust-and-ochre headwrap, a brown work shirt, a patterned wrap skirt over trousers, black gumboots and a woven basket. |
| CHR-05 | **Water collector** (new) | S3 A2; S4 A4 and A6 | Placeholder: `community_icon.png` | **P1** | A young man in his early 20s. Teal-blue T-shirt, an open grey zip jacket and dark trousers, carrying a plain yellow 20 L jerrycan. |
| CHR-06 | **Animal-health worker** (new, replaces the sector contact) | S2 sector contact (optional); S3 A2, A4 and A6; S4 A3 and A6 | Placeholder: `Sector_contact.png`, a different ink style and a 3/4 crop with no full body | **P1** | A man of about 30 based on the sector-contact design: a forest-green cap and multi-pocket vest, a cream shirt, a blank ID lanyard, a clipboard and brown boots. Redrawn full body in the cast style. **Remove the leaf logo from the cap and vest.** |
| CHR-07 | **Environmental officer** (new) | S3 A3 and A6; S4 A4, A6 and A7 | Placeholder: `environment_icon.png` | **P1** | A woman in her 40s with braids in a low bun, a blue rain jacket over a sand shirt, olive trousers, black gumboots and a plain grey sample case with no labels. |
| CHR-08 | **Radio operator** (new) | S3 A1 and A5 (the opening and route-disruption calls); S4 A5 | Placeholder: `information_icon.png` | **P1** | A woman in her late 40s with short locs, a navy polo, an over-ear headset resting around her neck, a handheld two-way radio and khaki trousers. |
| CHR-09 | **Caregiver** | S2 | Redo: 290–350 px, and her bag changes from blue to brown between expressions | P2 | Same design: a headwrap, purple cardigan, kitenge wrap skirt and sandals. **Always the blue crossbody bag.** |
| CHR-10 | **Child** | S2 | Redo: 253–328 px, and the positive pose is a seated sprite at a different scale | P2 | Same design: a boy of about 8 in a cream tee, blue shorts, blue backpack and sandals. All three expressions **standing**. Tired, not ill-looking. |
| CHR-11 | **Player (Coordinator)** | Not a speaker. Optional non-speaking figure or map token. | Redo (optional): 273 px | P3 | Same design: green zip jacket, white tee, khaki cargo trousers, blue trainers and a backpack. Front, back and side only. **No speaking pose.** |
| CHR-12 | Amina "welcoming" | S1 opening | Redo (optional) | P3 | An extra expression (a waving open hand), made by editing the approved Amina neutral. |
| — | Narrator (S4) | S4 intro and debriefs | n/a | — | **No portrait.** Narrator lines show no bust. If the slot must be filled, the engineer can use the logo mark (GFX-05), not a person. |

**Do Samuel, Amina and Dr. Malik need consistency redraws? Yes, all three, at P1.**
- They are 290 px crops from the AI reference sheet, upscaled about 2.65× on stage.
- Their props change between expressions (Amina's tablet becomes a folder, Malik's clipboard comes and goes), and Samuel's skin tone and hat brim drift.
- They would sit next to the new 2048 px characters in S3–S4.

Keep their **designs** exactly (silhouette, colours, hair, outfit) so returning players recognise them. Change only the resolution, the line and shading to bible spec, prop consistency and palette saturation.

---

## 2. Backgrounds

All backgrounds: a 2560×1440 master, exported to 1920×1080 PNG (RGB). **No text or signage.** Keep the HUD band (y 0–160) and the dialogue band (y 720–1080) low in detail. No foreground people. See ART_BIBLE §5.1–5.2.

| ID | Location | Where it appears | Status | Priority | Description |
|---|---|---|---|---|---|
| BG-01 | **Clinic interior** | S1 throughout; S2 E01–E06 | Redo: 756×484 with garbled posters, sign and clock | **P1** | The same room layout as today: an exam couch on the left, the doctor's desk centre-back, a door, shelves, and a sink with a handwashing station on the right. A teal lower-wall band and tiled floor, morning light from a left window showing hills. Posters are blank colour blocks. Open floor at the left and right for standing figures. |
| BG-02 | **Community meeting area** | S1 community scene; until BG-07 and BG-03–06 exist, the current fallback for all of S3 and S4 | Redo: 749×484, a painted style with a presenter in frame, a garbled easel, a sheep-goat hybrid and repeated faces | **P1** | The big shade tree (the "meeting tree") with wooden benches in a semicircle on swept earth, houses with iron roofs, terraced hills and the lake behind. Morning. **No presenter and no easel.** A few seated villagers in the mid-ground at the far left and right, seen from behind or the side. The centre stage is open. |
| BG-03 | **Farm / animal enclosure** | S3 A4 (farmer request); S4 A3 (animal-health workstream); farm report (A1) | Missing | **P1** | A hillside smallholding: a mud-brick house with an iron roof, a wooden-pole kraal with two calm, healthy Ankole cattle, a raised goat shed with goats, village chickens, banana plants, bean and maize terraces, a eucalyptus windbreak and a glimpse of the lake. Overcast and damp after rain. **No visible animal illness.** |
| BG-04 | **Water source after rain** | S3 A2 and A3 (water report); S4 A4 (water and environment workstream) | Missing | **P1** | A protected spring: a concrete spring box with a metal spout, a concrete apron and a stone-lined channel, a footpath and three plain yellow jerrycans. Wet ground, puddles, and muddy runoff beside the path with a slightly cloudy pool. The sky is clearing. **No signs, symbols, dead animals or people drinking.** |
| BG-05 | **Heavy rain: flooded route** | S3 A5 (heavy rain changes the plan) | Missing (the overcast grade on BG-02 stands in) | **P1** | A laterite road down to a low concrete river crossing that is submerged by a swollen brown river. Heavy rain, low cloud on the hills, a lone motorbike parked safely on the near side. **No people in danger.** It reads as "can't cross here", not as disaster. |
| BG-06 | **Field-team / coordination base** | S3 A1 (reports arrive), A3 (evidence board), A7 (field report); S4 A1 (handover), A5 (disruption) | Missing | **P1** | The covered veranda and office of a small sector field office: a wooden table with a two-way radio base set, a blank pinned-up paper map, folders with no labels, rubber boots by the door, rain-grey hills beyond the veranda posts. Teal tarp accents. |
| BG-07 | **Scenario 4 community briefing** | S4 A7 (public briefing) and A8 (ending) | Missing (BG-02 with the late-afternoon grade stands in) | **P1** | The same meeting-tree location as BG-02 at late afternoon, warm light from the upper left. More villagers seated in the mid-ground, a simple table at the front with a water container and blank paper sheets, and an open speaking spot. **No presenter.** Calm and resolved. Generate it as an image-to-image variant of BG-02. |
| BG-08 | Clinic waiting area | S2 E01 ("focus on waiting area") and E02 (caregiver arrives) | Missing (BG-01 is reused) | P2 | The same clinic and wall colours as BG-01, viewed toward reception: wooden benches, a reception counter, a door to outside showing daylight. Only a few seated people, at the far edges. |
| BG-09 | Main-menu key art | Main menu | Redo: anime style, 1600×731, the logo baked in | P2 | A wide Lake Kivu landscape in the target style: terraced hills, village roofs, the lake, distant volcanoes in haze. **No logo or text.** The **left 45% is calm** (sky or lake) for the menu card. 2560×1440. |
| BG-10 | Field base in rain | S3 A5 radio call | Missing | P3 | A rain variant of BG-06 made by image-to-image. Optional, because the engine grade plus a rain particle system covers it. |
| FX-01 | Rain overlay | S3 A5; optional rainy S3 beats | Missing | P3 | A tileable transparent rain-streak texture (1024×1024), or better, a Unity particle system built by the engineer. Not an AI-generation task. |

---

## 3. Maps, diagrams, badges, icons and branding

| ID | Asset | Where it appears | Status | Priority | Size / format | Description |
|---|---|---|---|---|---|---|
| MAP-01 | **S3 field map (base)** | S3 A5 card ("Field map"); A1 report pins | Placeholder: programmer-drawn `field_map_placeholder.png` (960×600) | **P1** | 1920×1200 PNG, no text | A top-down, illustrated map: a river running north to south through the centre, a low crossing where the river widens, terraced fields, two clusters of iron-roofed houses, a lake edge at one side and laterite roads. Flat, in the bible palette. Markers and routes are separate sprites (MAP-02). |
| MAP-02 | **Map markers and route sprites** | S3 A1 and A5 | Placeholder: drawn into the placeholder | **P1** | SVG plus 256×256 PNG for each marker; route sprites at map scale | The team base (a navy roof pin), the **blocked crossing** (a coral circular marker with a white bar, not a skull or hazard symbol), the **accessible site** (a leaf-green ring pin), the planned route (dashed laterite line), the alternate route (solid line), and the farm, water and community pins (for A1). Flat vector, one glyph per pin. |
| GFX-01 | **One Health diagram, no text** | S1 One Health card; S2 connection board; S4 debrief "One Health" card | Redo: has baked text, a Holstein cow, an opaque box and a filename crop | **P1** | Three 1024×1024 transparent PNGs | Three circular vignettes. **People:** a health worker, a caregiver with a child and a farmer. **Animals:** an Ankole cow, a goat and village chickens. **Environment:** the lake, terraced hills, a stream and banana plants. The ring, arrows and labels are built in the UI. |
| GFX-02 | **Badge medal (base)** | All badge cards: S3 Evidence Tracker, Joint Response, One Health Coordinator; S4 Community Partner, Responsible Stewardship, Precaution With Evidence, Kivu Coordinator, One Health Champion | Redo: the ribbon is off-centre and the star overlaps the ring | **P1** | SVG plus a 512×512 PNG | A symmetric round medal: an amber disc with a cream inner ring, a centred emblem slot and two ribbon tails in teal and leaf green. Flat, with a `#1B423A` outline. **Recommend building it in vector by hand** (see the risks in `PROMPTS.md` §1.4). |
| GFX-03 | **Closed-lock icon** | Locked options and locked scenarios (main menu, choice menu) | Redo: the shackle is detached in `lock_icon.png` and `locked_icon.png` | **P1** | SVG plus a 256×256 PNG | A white closed padlock glyph on a rounded-square Navy `#1F2A44` tile, with the shackle fully seated in the body and a keyhole. It matches the existing tile-icon grammar. **Vector by hand.** |
| GFX-04 | Open-lock icon (matching) | Unlock moments and the reward panel | Redo | P2 | SVG plus a 256×256 PNG | The same construction as GFX-03, with the shackle raised and rotated on its left leg. |
| GFX-05 | **Logo mark, clean** | Main menu, title cards, results; optional narrator slot | Placeholder: `logo_placeholder.png` (a green circle), and the real logo is baked into the key-art JPEG | **P1** | SVG plus a 1024×1024 transparent PNG; the wordmark is set separately in Nunito ExtraBold by a designer | A text-free emblem: a circle split into three interlocking segments (a person, an Ankole cow head and a leaf over water waves) in Teal, Leaf Green and Amber, with a `#1B423A` outline. **AI for concepts only. A designer finalises it in vector.** |
| GFX-06 | Badge emblem set (8) | The 8 badge cards listed under GFX-02 | Missing (the same star for every badge) | P2 | SVG plus 256×256 each, centred in the medal slot | One white glyph per badge: a magnifier over a leaf (Evidence Tracker), three linked rings (Joint Response), a compass (One Health Coordinator), two hands (Community Partner), a shield with a leaf (Responsible Stewardship), a droplet with a check (Precaution With Evidence), a lake-and-hill mark (Kivu Coordinator) and the logo mark (One Health Champion). **Vector by hand.** |
| GFX-07 | Safe-water icon | S4 A2 and A4 workstream tiles | Missing (the environment icon stands in) | P3 | 256×256 tile icon | A white droplet glyph on a `#2F6FB0` tile, in the existing icon grammar. |
| GFX-08 | Trust icon redo | HUD | Redo: an ambiguous glyph (critique §3.1) | P3 | 256×256 tile icon | A handshake glyph simplified to two clasped hands. Vector. |

Not on this list on purpose:
- Evidence cards, the case board, report sheets and the result panels. These are built in the UI from existing sliced sprites and text.
- The S4 report sheet and S3 teaser card, which are text in the UI.
- The glossy `05_Icons_HD` set, which should be retired, not replaced.

---

## 4. Counts

| Priority | Asset IDs | What gets generated or drawn |
|---|---|---|
| **P1** | **21 IDs:** 8 characters (CHR-01 to CHR-08), 7 backgrounds (BG-01 to BG-07), and 6 graphics (MAP-01, MAP-02, GFX-01, GFX-02, GFX-03, GFX-05) | 24 full-body images plus 24 bust crops, 7 backgrounds, 1 map base, about 9 map sprites, 3 diagram vignettes, the medal, the lock and the logo mark. About **70 image files**. Only 35 of them are accepted generations: 24 full bodies, 7 backgrounds, 1 map base and 3 vignettes. The busts are crops, and the medal, lock, logo and markers are vector work. |
| P2 | 6 IDs: CHR-09, CHR-10, BG-08, BG-09, GFX-04, GFX-06 (the 8 badge emblems count as one ID) | 6 full-body images plus 6 crops, 2 backgrounds, 1 icon and 8 emblems |
| P3 | 6 IDs: CHR-11, CHR-12, BG-10, FX-01, GFX-07, GFX-08 | — |

### Suggested order

1. CHR-01 Samuel neutral (the golden reference).
2. BG-02 community meeting (the golden background).
3. The remaining P1 characters, neutral first, then their expressions.
4. The other P1 backgrounds, with BG-07 last as a variant of BG-02.
5. GFX-01.
6. The vector items: GFX-02, GFX-03, GFX-05 and MAP-02. These can run in parallel with a designer.
