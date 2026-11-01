# One Health Innovator: generation prompts

This file has prompts for every P1 and P2 asset in `ASSET_LIST.md`. Style, specs and the avoid list are in `ART_BIBLE.md`. Scoring is in `CRITIQUE_RUBRIC.md`.

All paths below are absolute and point into ``.

---

## 1. How to use this file

### 1.1 Generation route for each asset type

| Asset type | Route | Why |
|---|---|---|
| Character, neutral | **Image-to-image with references** (preferred) or text-to-image | References carry the style and the existing design. Text-to-image is the fallback. |
| Character, concerned and positive | **Edit the approved neutral image** | This is the only reliable way to keep face, outfit and scale identical. |
| Character busts | **Crop the approved full body.** Never generate a bust. | Separate generations drift in face and outfit. |
| Backgrounds BG-01 and BG-02 (redos) | Image-to-image from the existing image as a layout reference, plus the text prompt | This keeps the room and the meeting place the team already knows. |
| Backgrounds BG-03 to BG-09 | Text-to-image, then image-to-image with an approved background as the style reference | Each new scene matches BG-02 once BG-02 is approved. |
| BG-07 briefing | Edit the approved BG-02 | It is the same place at a different time of day. |
| Diagram vignettes, map base | Text-to-image, with an approved background as the style reference | — |
| Medal, locks, logo, map markers, badge emblems | A vector-capable model for **concepts only**, then a person builds the final in vector | AI cannot reliably draw exact symmetry, closed shapes or precise construction. |

### 1.2 Negative prompts

- Diffusion pipelines in the Stable Diffusion family, and some FLUX pipelines, accept a separate negative-prompt field. Paste the **Negative** block there.
- Instruction-following and editing models (the Gemini image, GPT-image, FLUX Kontext and Seedream families) usually **have no negative field**, and the words "no X" can even suggest X. For these:
  - phrase the prompt positively ("blank white ID card", "plain wall");
  - append the short **Avoid** line given with each prompt as a final sentence;
  - leave out the long negative block.

### 1.3 Size, transparency and post-processing

| Asset | Generate at | Deliver |
|---|---|---|
| Character full body | A **1:2** portrait (or 9:16 if 1:2 is not offered) at the model's highest native resolution, **at least 1536 px tall**. If the head in the source is under about 220 px tall, upscale 2× first. | A 1024×2048 RGBA canvas, feet on y = 1984, figure height from the bible table, centred at x = 512 |
| Character bust | — (crop) | 512×512 RGBA (§1.5) |
| Background | 16:9 at the highest native resolution (2K or more preferred) | Upscale to a 2560×1440 master, export 1920×1080 PNG |
| Map base | 16:10 at the highest native resolution | 1920×1200 PNG |
| Vignettes | 1:1, at least 1024 | 1024×1024 RGBA, circular content |
| Vector items | 1:1 concept | SVG plus PNG (see `ASSET_LIST.md`) |

**Transparency**
- If your tool offers a transparent-background option (some GPT-image-based tools do), use it.
- Otherwise generate on **flat plain light grey #D8D8D8**: grey separates cleanly from white coats and green outfits. Then run a background-removal model.
- Check the cut-out over black and over white for halos, and erode a 1 px fringe if needed.

**Upscaling.** Use an illustration-aware upscaler (ESRGAN-class models tuned for line art or illustration), or a "creative" upscaler at its **lowest creativity**. Higher creativity invents texture and pseudo-text.

**Record keeping and delivery.** Deliver every file to `ArtDrop\<asset ID>\`, following `HANDOFF.md`. That includes the variants, a `.json` sidecar with the model, seed, prompt, references and post-processing steps, and the rubric score.

### 1.4 Recommended model families

The team generates in its own tools. Every prompt here is written for **a general illustration model**, used either as plain text-to-image or with **optional image-reference input**, where the tool accepts reference or "style" images. If your tool takes no references, use the text-to-image prompt: the OUTFIT ANCHOR and the shared blocks carry the consistency on their own. With references, the results are more consistent, so use them whenever your tool allows it.

The families below are **capability families**, not exact product or endpoint names. Use whichever of them your tool offers (fal.ai, OpenRouter, a vendor app or a local install).

| Need | Capability required | Families that fit (check availability) |
|---|---|---|
| **Character redraws, new characters, expression edits** | Instruction-based **image editing** with **one or more reference images**: preserves a character's identity, outfit and line style across edits, and follows long prompts precisely | The **FLUX.1 Kontext** family (fal.ai); the **Gemini 2.5 Flash Image ("Nano Banana")** family (fal.ai and OpenRouter); **Seedream 4.x** edit or multi-reference (fal.ai); **Qwen-Image-Edit** (fal.ai); the **GPT-image** family (where exposed). Run the same test on two or three of them (§1.6) and keep the winner for the whole cast. **Do not mix models within one character.** |
| **Backgrounds** | **High native resolution (2K or more)** text-to-image, strong prompt adherence on layout ("empty floor at left and right", "calm top band"), reliably text-free output, and an optional style-reference image | **Seedream 4.x** (native output up to 4K); the **FLUX 1.1 Pro Ultra** or newer FLUX pro generation; **Imagen 4**-class models, where available. Use the editing families above for image-to-image variants (BG-07, BG-01 and BG-02 redos). |
| **Vector concepts** (medal, lock, logo, markers, emblems) | Flat vector or icon styles, ideally with **SVG output** | The **Recraft** family (vector illustration and icon styles), or **Ideogram** for logo concepts. A person finishes the art in Figma, Illustrator or Inkscape. |
| **Utilities** | Background removal; upscaling | BiRefNet-class background removal; ESRGAN-class illustration upscalers |

**Avoid** anime-tuned or photoreal-tuned checkpoints and old SD 1.5-class models. They pull toward styles 1 and 2, the two styles we are retiring.

### 1.5 Bust crop recipe (all characters)

Crop all three expressions with identical crop rectangles, so the busts don't jump when the expression changes.

1. On the approved 1024×2048 neutral full body, find the eye line *E* and the face centre *X*.
2. Make a square crop of side **S = 2.6 × head height** (chin to hairline, excluding the hat). Centre it horizontally on *X*, and place the eye line at **42% from the top**. Hats and headwraps may touch the top edge but must not be cut off mid-brim if avoidable. For Samuel's hat, use S = 3.0 × head height.
3. Resize to **512×512** with Lanczos. Keep the alpha.
4. Apply the same rectangle to the concerned and positive images. If an arm or hand gesture enters the crop awkwardly (a finger cut off at the edge), shift the crop by 20 px at most. Never rescale one expression differently from the others.
5. Test: downscale to 160 px inside a circular mask on Cream `#F7F2E7` with a Navy `#1F2A44` ring, as in the game (`FungusDialogStyler.cs`, portrait slot 164 px). The eyes, brows and mouth must read (see the rubric).

### 1.6 Model bake-off (do this once before production)

Run CHR-01 Samuel neutral (§3.1) on two or three candidate models with the same references. Score each with the rubric, and choose the model with the best **style match** and **hands** scores. Lock that model and its settings for all characters.

---

## 2. Shared blocks

The character neutral prompts (§3) and the golden background prompt BG-02 (§4.1) have these blocks written out in full. Every other prompt marks a block's position with a placeholder such as `[STYLE-B, verbatim]` or `[AVOID-C]`. Paste the block text there, unchanged, before you send the prompt. A critic can compare the pasted text with the blocks below to check that nothing was edited.

**[STYLE-C] (characters)**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.

**[FRAME-FULL] (characters)**
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**[NEG-C] (characters)**
> text, letters, words, numbers, logo, emblem, patch with symbol, watermark, signature, extra fingers, missing fingers, fused fingers, six fingers, malformed hands, deformed face, asymmetric eyes, cross-eyed, plastic skin, glossy skin, airbrushed, 3D render, photorealistic, anime, chibi, oversized eyes, exaggerated proportions, lightened skin, over-saturated, neon colours, gradient background, scenery, cast shadow, cropped feet, cropped head, multiple people, duplicate limbs, white outline halo, sticker border, blurry, jpeg artifacts, brush texture, rim light, lens flare, mismatched shoes

**[AVOID-C]** (a one-line version for models without a negative field)
> Avoid: any text, letters or logos; extra or fused fingers; plastic or airbrushed skin; anime or 3D-render looks; background scenery; cast shadows; cropped feet.

**[STYLE-B] (backgrounds)**
> 2D game background illustration in a clean-line cel style designed to sit behind cel-shaded characters: simplified shapes, flat colour areas with one soft shadow tone, thin coloured outlines on foreground objects only and no black outlines, atmospheric perspective that fades distant hills toward pale blue-grey (#9DBBC8), medium-low saturation, calm and uncluttered, light coming from the upper left. Wide 16:9 composition, eye-level camera at standing height, horizon at about 45% of the image height. The top 15% of the image is calm and low in detail. The bottom third is plain, open floor or ground with little detail. The left and right thirds have open, empty floor space where characters will stand. No people in the foreground.

**[NEG-B] (backgrounds)**
> text, letters, words, numbers, signage, posters with writing, labels, logo, watermark, signature, clock, screens with content, people in the foreground, presenter, close-up faces, duplicated objects, repeated trees, warped perspective, photorealistic, 3D render, oil painting, painterly brush texture, anime, over-saturated, neon, HDR, lens flare, god rays, heavy vignette, fisheye, tilted horizon, blurry, jpeg artifacts, skull, hazard symbol, warning sign, red cross, flag, dead animals, sick animals, garbage, sewage, savannah, acacia, palm beach

**[AVOID-B]**
> Avoid: any text, letters, signs or logos; people in the foreground; photoreal, painterly or anime looks; over-saturation; hazard or warning symbols.

---

## 3. Characters

**References for every character prompt.** Attach them in this order:
1. **Style and rendering reference:** `Assets\Art\02_Scenario_2\sector_contact\Sector_contact.png`. After CHR-01 is approved, use the approved `chr_samuel_full_neutral.png` instead.
2. **Design reference** (for redraws): the character's existing sprite, listed per character.
3. **Proportion reference** (for new characters): `Assets\Art\01_Scenario_1\01_Characters\Samuel\samuel_neutral.png`, or the approved Samuel redraw.

The **OUTFIT ANCHOR** in each section is reused **verbatim** in the neutral prompt, in both expression edits and in any fallback regeneration. Do not paraphrase it.

---

### 3.1 CHR-01 Samuel (P1). Generate first: this is the golden reference.

**Design reference:** `Assets\Art\01_Scenario_1\01_Characters\Samuel\samuel_neutral.png` (and `samuel_concerned.png`, `samuel_positive.png`)

**OUTFIT ANCHOR**
> Samuel, a farmer and community representative from the Lake Kivu region in his mid-40s, deep-brown skin (#5A3825), short black hair, a neat short black beard and moustache, kind steady eyes. He wears a wide-brimmed natural woven straw hat with an amber (#F2A73B) band; a green (#3E8E5A) long-sleeve button-up work shirt with the sleeves rolled to the elbows and two plain buttoned chest pockets with no emblem; a brown leather belt with a brass buckle; khaki (#C9B48A) cargo trousers with side pockets; and dark green (#2F5E3E) knee-high rubber gumboots.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> Samuel, a farmer and community representative from the Lake Kivu region in his mid-40s, deep-brown skin (#5A3825), short black hair, a neat short black beard and moustache, kind steady eyes. He wears a wide-brimmed natural woven straw hat with an amber (#F2A73B) band; a green (#3E8E5A) long-sleeve button-up work shirt with the sleeves rolled to the elbows and two plain buttoned chest pockets with no emblem; a brown leather belt with a brass buckle; khaki (#C9B48A) cargo trousers with side pockets; and dark green (#2F5E3E) knee-high rubber gumboots.
> Expression: neutral and attentive, a slight closed-mouth smile. Pose: both hands resting on his hips, confident relaxed stance.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Image-to-image prompt, neutral** (attach the style reference and the design reference)
> Redraw the man in the second reference image as a high-resolution 2D game character, keeping his exact design, outfit, colours, hat and pose, rendered in the line and shading style of the first reference image: clean warm near-black outlines, flat colours with one hard-edged shadow tone, matte skin, muted palette, light from the upper left. [OUTFIT ANCHOR, pasted verbatim]. Expression: neutral and attentive, a slight closed-mouth smile; both hands resting on his hips. Full body from hat to boot soles, eye-level, on a plain flat light-grey background. Remove any logo or emblem from the shirt. [AVOID-C]

**Edit prompt, concerned** (input: the approved neutral)
> Same character, same outfit, colours, hat, scale, camera and leg stance exactly as in the input image. Change only the face and arms: concerned expression, inner brows raised and drawn together, mouth slightly pressed and turned down, eyes looking slightly down to the side; arms loosely folded across his chest. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive** (input: the approved neutral)
> Same character, same outfit, colours, hat, scale, camera and leg stance exactly as in the input image. Change only the face and arms: positive expression, a warm open smile showing a little of the upper teeth, relaxed brows, friendly eyes; right hand raised open-palm at chest height in a welcoming gesture, left thumb hooked in his belt. Keep the plain light-grey background. [AVOID-C]

**Text-to-image fallback for expressions:** replace the `Expression … Pose …` sentence in the neutral prompt with the face and arm text from the edit prompt.

**Negative:** [NEG-C], plus `cowboy hat, baseball cap, shirt logo, yellow shirt, sandals`

---

### 3.2 CHR-02 Amina (P1)

**Design reference:** `Assets\Art\01_Scenario_1\01_Characters\Amina\amina_neutral.png`

**OUTFIT ANCHOR**
> Amina, a community health worker from the Lake Kivu region in her early 20s, medium-deep brown skin (#6B4429), long dark curly hair pulled back into a full high ponytail with a mid-blue (#3A78C2) fabric headband, small gold stud earrings. She wears a plain white short-sleeve crew-neck T-shirt; a mid-blue (#3A78C2) zip-front health-worker vest with two chest pockets and a small plain white circle patch on the left chest, with no symbol and no text; dark indigo (#2E3F66) denim cargo trousers with side pockets, cuffs rolled once; and white low-top trainers. She always holds a dark grey (#3A3F45) tablet with a blank screen.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> Amina, a community health worker from the Lake Kivu region in her early 20s, medium-deep brown skin (#6B4429), long dark curly hair pulled back into a full high ponytail with a mid-blue (#3A78C2) fabric headband, small gold stud earrings. She wears a plain white short-sleeve crew-neck T-shirt; a mid-blue (#3A78C2) zip-front health-worker vest with two chest pockets and a small plain white circle patch on the left chest, with no symbol and no text; dark indigo (#2E3F66) denim cargo trousers with side pockets, cuffs rolled once; and white low-top trainers. She always holds a dark grey (#3A3F45) tablet with a blank screen.
> Expression: neutral, friendly and attentive, a slight closed-mouth smile. Pose: holding the tablet in both hands at waist height, the screen facing her.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Image-to-image prompt, neutral:** as for Samuel. Attach the style reference (the approved Samuel), then `amina_neutral.png`, then paste Amina's OUTFIT ANCHOR. Keep her design; render at bible spec; keep the tablet; desaturate the vest slightly to #3A78C2.

**Edit prompt, concerned**
> Same character, same outfit, colours, hair, tablet, scale, camera and leg stance exactly as in the input image. Change only the face and arms: concerned expression, inner brows raised and drawn together, lips pressed; the tablet held flat against her chest with her left arm, right hand lightly touching her chin. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, colours, hair, tablet, scale, camera and leg stance exactly as in the input image. Change only the face and arms: positive expression, a big warm smile, bright eyes; the tablet held in her left hand at her side, right hand giving a thumbs-up at shoulder height. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `clipboard, folder, saturated royal blue, nurse cap, red cross`

---

### 3.3 CHR-03 Dr. Malik (P1)

**Design reference:** `Assets\Art\01_Scenario_1\01_Characters\Dr_Malik\malik_neutral.png`

**OUTFIT ANCHOR**
> Dr. Malik, a clinician from the Lake Kivu region of about 40, deep-brown skin (#4A2E1F), short black coily hair, a neatly trimmed full black beard, round dark-brown framed glasses. He wears an open white knee-length lab coat with full-length sleeves and no name tag; a mid-blue (#4A7FB5) collared button shirt tucked in, with no tie; a black stethoscope around his neck; a dark brown leather belt; charcoal-grey (#4A4E55) trousers; and dark brown leather shoes. He always carries a dark grey clipboard holding blank paper.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> Dr. Malik, a clinician from the Lake Kivu region of about 40, deep-brown skin (#4A2E1F), short black coily hair, a neatly trimmed full black beard, round dark-brown framed glasses. He wears an open white knee-length lab coat with full-length sleeves and no name tag; a mid-blue (#4A7FB5) collared button shirt tucked in, with no tie; a black stethoscope around his neck; a dark brown leather belt; charcoal-grey (#4A4E55) trousers; and dark brown leather shoes. He always carries a dark grey clipboard holding blank paper.
> Expression: neutral, calm and professional, a slight closed-mouth smile. Pose: the clipboard held at his side in his left hand, right hand relaxed.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Edit prompt, concerned**
> Same character, same outfit, glasses, colours, clipboard, scale, camera and leg stance exactly as in the input image. Change only the face and arms: concerned and thoughtful, brows drawn together, mouth slightly pressed; the clipboard held against his chest with his left arm, right hand touching his chin. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive** (the "reassuring" expression)
> Same character, same outfit, glasses, colours, clipboard, scale, camera and leg stance exactly as in the input image. Change only the face and arms: reassuring, a gentle warm smile and soft eyes; right hand extended slightly forward at waist height, palm open, in a calming gesture, the clipboard in his left hand. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `tablet, surgical mask, scrubs, syringe, name badge text`

---

### 3.4 CHR-04 Farmer (P1, new)

**Proportion reference:** the approved Samuel. **No design reference.**

**OUTFIT ANCHOR**
> The farmer, a woman from the Lake Kivu region in her early 50s who runs her own mixed smallholding, medium brown skin (#7D5235), smile lines, strong capable hands, a sturdy build. She wears a rust and ochre (#B5653A, #F2A73B) patterned headwrap tied at the back; small gold hoop earrings; a long-sleeve brown (#8A5A2B) cotton work shirt with the sleeves pushed to the forearms; a calf-length wrap skirt of kitenge cloth in a deep green (#2E5E3E) and rust (#B5653A) geometric leaf pattern, worn over dark brown work trousers; and black (#2A2A2A) rubber gumboots. An empty woven natural sisal basket hangs from her left forearm.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The farmer, a woman from the Lake Kivu region in her early 50s who runs her own mixed smallholding, medium brown skin (#7D5235), smile lines, strong capable hands, a sturdy build. She wears a rust and ochre (#B5653A, #F2A73B) patterned headwrap tied at the back; small gold hoop earrings; a long-sleeve brown (#8A5A2B) cotton work shirt with the sleeves pushed to the forearms; a calf-length wrap skirt of kitenge cloth in a deep green (#2E5E3E) and rust (#B5653A) geometric leaf pattern, worn over dark brown work trousers; and black (#2A2A2A) rubber gumboots. An empty woven natural sisal basket hangs from her left forearm.
> Expression: neutral, steady and direct, a composed mouth. Pose: standing upright, right hand relaxed at her side, the basket on her left forearm.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Image-to-image prompt, neutral:** attach the approved Samuel as the style and proportion reference. Prompt: "Create a new character in exactly the same art style, line weight, shading and scale as the reference image: [OUTFIT ANCHOR] [Expression and pose sentences] [FRAME-FULL] [AVOID-C]"

**Edit prompt, concerned**
> Same character, same outfit, headwrap, colours, basket, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: concerned, brows drawn together, lips pressed, gaze slightly to the side as if thinking about her animals; right hand resting flat on her collarbone. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, headwrap, colours, basket, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: positive, a broad warm smile and crinkled eyes; right hand open with the palm up at waist height, as if explaining a practical plan. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `animal print, beads, face paint, barefoot, torn clothes, hoe, machete, carrying load on head`

---

### 3.5 CHR-05 Water collector (P1, new)

**OUTFIT ANCHOR**
> The water collector, a slim young man from the Lake Kivu region in his early 20s, medium-warm brown skin (#8F6142), short cropped black hair with a neat fade, clean-shaven, an open friendly face. He wears a teal-blue (#2F8FB0) crew-neck T-shirt with no print; an open light-grey (#A7ADB2) zip jacket with the sleeves pushed up; dark charcoal (#3A3D42) track trousers; and black plastic sandals. In his right hand he carries one plain yellow (#E8B634) 20-litre plastic jerrycan by its handle, with no label and no text.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The water collector, a slim young man from the Lake Kivu region in his early 20s, medium-warm brown skin (#8F6142), short cropped black hair with a neat fade, clean-shaven, an open friendly face. He wears a teal-blue (#2F8FB0) crew-neck T-shirt with no print; an open light-grey (#A7ADB2) zip jacket with the sleeves pushed up; dark charcoal (#3A3D42) track trousers; and black plastic sandals. In his right hand he carries one plain yellow (#E8B634) 20-litre plastic jerrycan by its handle, with no label and no text.
> Expression: neutral and attentive, a relaxed mouth. Pose: the jerrycan hanging from his right hand at his side, left hand relaxed.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Edit prompt, concerned**
> Same character, same outfit, colours, jerrycan in the right hand, scale, camera and leg stance exactly as in the input image. Change only the face and left arm: puzzled and concerned, brows drawn together, mouth slightly open as if unsure; left hand rubbing the back of his neck. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, colours, jerrycan in the right hand, scale, camera and leg stance exactly as in the input image. Change only the face and left arm: positive, an easy open smile; left hand raised in a small friendly wave at shoulder height. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `brown water, dirty water, label on jerrycan, carrying on head, bicycle, child, woman`

---

### 3.6 CHR-06 Animal-health worker (P1, new, replaces the sector contact)

**Design reference:** `Assets\Art\02_Scenario_2\sector_contact\Sector_contact.png`. This keeps the costume. Draw the full body in the cast's line weight.

**OUTFIT ANCHOR**
> The animal-health worker, a man from the Lake Kivu region of about 30, medium-deep brown skin (#6B4429), short black hair, a short boxed beard. He wears a plain forest-green (#3F6B4A) baseball cap with no logo; a cream (#EFE4C8) long-sleeve collared field shirt with the sleeves rolled to the forearms; a forest-green (#3F6B4A) multi-pocket field vest with four flap pockets and no logo or patch; a dark navy lanyard with a blank white ID card with no text and no photo; a brown leather belt; dark navy (#2B3550) work trousers; and dark brown (#5A3B26) lace-up work boots. He always carries a black clipboard holding blank paper.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The animal-health worker, a man from the Lake Kivu region of about 30, medium-deep brown skin (#6B4429), short black hair, a short boxed beard. He wears a plain forest-green (#3F6B4A) baseball cap with no logo; a cream (#EFE4C8) long-sleeve collared field shirt with the sleeves rolled to the forearms; a forest-green (#3F6B4A) multi-pocket field vest with four flap pockets and no logo or patch; a dark navy lanyard with a blank white ID card with no text and no photo; a brown leather belt; dark navy (#2B3550) work trousers; and dark brown (#5A3B26) lace-up work boots. He always carries a black clipboard holding blank paper.
> Expression: neutral, calm and competent, a slight closed-mouth smile. Pose: the clipboard held against his chest with his left arm, right hand relaxed at his side.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Image-to-image prompt, neutral:** attach the approved Samuel (style), then `Sector_contact.png` (design). Prompt: "Redraw the man in the second reference image as a full-body standing character, keeping his face, cap, vest and shirt design, in the exact line weight, shading and proportions of the first reference image. Remove the leaf logos from the cap and vest. [OUTFIT ANCHOR] [Expression and pose sentences] [FRAME-FULL] [AVOID-C]"

**Edit prompt, concerned**
> Same character, same outfit, cap, colours, clipboard, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: thoughtful and concerned, brows drawn together, lips pressed; right hand touching his chin, the clipboard against his chest. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, cap, colours, clipboard, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: positive, a friendly open smile, mid-explanation; right hand open with the palm up, gesturing outward at waist height. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `leaf logo, cap logo, syringe, medicine bottle, stethoscope, lab coat, animal`

---

### 3.7 CHR-07 Environmental officer (P1, new)

**OUTFIT ANCHOR**
> The environmental officer, a woman from the Lake Kivu region in her early 40s, deep-warm brown skin (#5A3825), medium-length black braids gathered in a neat low bun, small silver stud earrings, an intelligent composed face. She wears an open mid-blue (#2F6FB0) waterproof rain jacket with the hood down; a sand-coloured (#E8DDC4) collared shirt; dark olive (#4F5A3A) field trousers tucked into black (#2A2A2A) rubber gumboots; and a blank white ID card on a teal (#2E7D6B) lanyard. In her left hand she always carries a plain matte-grey (#7A8085) hard-shell sample case with a black handle, with no labels and no symbols.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The environmental officer, a woman from the Lake Kivu region in her early 40s, deep-warm brown skin (#5A3825), medium-length black braids gathered in a neat low bun, small silver stud earrings, an intelligent composed face. She wears an open mid-blue (#2F6FB0) waterproof rain jacket with the hood down; a sand-coloured (#E8DDC4) collared shirt; dark olive (#4F5A3A) field trousers tucked into black (#2A2A2A) rubber gumboots; and a blank white ID card on a teal (#2E7D6B) lanyard. In her left hand she always carries a plain matte-grey (#7A8085) hard-shell sample case with a black handle, with no labels and no symbols.
> Expression: neutral, composed and observant, a slight closed-mouth smile. Pose: the sample case in her left hand at her side, right hand relaxed.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Edit prompt, concerned**
> Same character, same outfit, braids, colours, sample case in the left hand, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: concerned but measured, brows drawn together, lips pressed; right hand at her chin as if weighing the evidence. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, braids, colours, sample case in the left hand, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: positive, a warm confident smile; right hand open-palm at chest height in an explaining gesture. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `hazard symbol, biohazard, test tubes, gas mask, hazmat suit, gloves, wide-brim hat, glasses`

---

### 3.8 CHR-08 Radio operator (P1, new)

**OUTFIT ANCHOR**
> The radio operator, a woman from the Lake Kivu region in her late 40s, deep-brown skin (#4A2E1F), short black locs reaching her jaw, small gold hoop earrings, laugh lines, an alert calm face. She wears a navy (#1F2A44) short-sleeve polo shirt with no logo; a black over-ear headset with a small boom microphone resting around her neck, not on her ears; a black wristwatch; khaki (#C9B48A) trousers; and black trainers. In her right hand she always holds a black handheld two-way radio with a short antenna.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The radio operator, a woman from the Lake Kivu region in her late 40s, deep-brown skin (#4A2E1F), short black locs reaching her jaw, small gold hoop earrings, laugh lines, an alert calm face. She wears a navy (#1F2A44) short-sleeve polo shirt with no logo; a black over-ear headset with a small boom microphone resting around her neck, not on her ears; a black wristwatch; khaki (#C9B48A) trousers; and black trainers. In her right hand she always holds a black handheld two-way radio with a short antenna.
> Expression: neutral, calm and alert, a slight closed-mouth smile. Pose: the radio held in her right hand at waist height, left hand relaxed at her side.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Edit prompt, concerned**
> Same character, same outfit, locs, headset around the neck, colours, radio in the right hand, scale, camera and leg stance exactly as in the input image. Change only the face and left arm: concerned and focused, brows drawn together, listening intently; left hand lightly touching the headset ear cup at her neck. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, locs, headset around the neck, colours, radio in the right hand, scale, camera and leg stance exactly as in the input image. Change only the face and left arm: positive, a warm smile; left hand giving a thumbs-up at chest height. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `military uniform, camouflage, police, radio held to mouth, headset on head, desk, chair`

---

### 3.9 CHR-09 Caregiver (P2)

**Design reference:** `Assets\Art\02_Scenario_2\01_Characters\Caregiver\caregiver_neutral.png`. This is the version with the **blue** bag.

**OUTFIT ANCHOR**
> The caregiver, a mother from the Lake Kivu region of about 30, medium brown skin (#7D5235), her hair wrapped in a mustard-yellow and orange (#E0A030, #D9782F) patterned headwrap with a small knot on top. She wears a cream (#F1E6D0) scoop-neck top; an open plum-purple (#6B3F7A) knit cardigan; an ankle-length wrap skirt of blue (#2F5C9A) kitenge cloth with a mustard-gold (#D9A33A) swirling geometric pattern; and brown leather flat sandals. A navy-blue (#2E4C78) fabric crossbody bag on a long strap crosses her body and rests at her right hip.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair, eyes and rubber. Matte skin, no airbrushing, no gradients, no painterly texture. Grounded semi-realistic proportions, about 7.5 heads tall, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The caregiver, a mother from the Lake Kivu region of about 30, medium brown skin (#7D5235), her hair wrapped in a mustard-yellow and orange (#E0A030, #D9782F) patterned headwrap with a small knot on top. She wears a cream (#F1E6D0) scoop-neck top; an open plum-purple (#6B3F7A) knit cardigan; an ankle-length wrap skirt of blue (#2F5C9A) kitenge cloth with a mustard-gold (#D9A33A) swirling geometric pattern; and brown leather flat sandals. A navy-blue (#2E4C78) fabric crossbody bag on a long strap crosses her body and rests at her right hip.
> Expression: neutral, attentive and a little impatient to be seen, a composed mouth. Pose: hands loosely clasped in front of her waist.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Edit prompt, concerned**
> Same character, same outfit, headwrap, colours, navy crossbody bag, scale, camera and leg stance exactly as in the input image. Change only the face and arms: worried, inner brows raised, lips pressed; right hand touching her cheek, left hand holding the bag strap. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same character, same outfit, headwrap, colours, navy crossbody bag, scale, camera and leg stance exactly as in the input image. Change only the face and arms: relieved and positive, a soft warm smile, shoulders relaxed; both hands resting lightly together at her waist. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `brown bag, leather handbag, crying, baby on back`

---

### 3.10 CHR-10 Child (P2)

**Design reference:** `Assets\Art\02_Scenario_2\01_Characters\Child\child_neutral.png`

**OUTFIT ANCHOR**
> The child, a boy from the Lake Kivu region of about 8 years old, child proportions about 5.5 heads tall, medium-deep brown skin (#6B4429), short black coily hair. He wears a cream (#F1E6D0) short-sleeve T-shirt with no print; dark blue (#2F5C9A) knee-length shorts; a mid-blue (#3A6FB0) school backpack worn on both shoulders; and dark brown sandals.

**Text-to-image prompt, neutral**
> 2D game character illustration in a clean-line cel-shaded style. Confident warm near-black outlines (#2B2420) of even medium-heavy weight with a slight taper; thinner interior lines for folds and features. Flat base colours with one hard-edged, slightly cool shadow tone; tiny crisp highlights only on hair and eyes. Matte skin, no airbrushing, no gradients, no painterly texture. Natural child proportions, natural and specific facial features. Muted, medium-saturation palette. Even daylight from the upper left, shadows to the lower right. No cast shadow.
> The child, a boy from the Lake Kivu region of about 8 years old, child proportions about 5.5 heads tall, medium-deep brown skin (#6B4429), short black coily hair. He wears a cream (#F1E6D0) short-sleeve T-shirt with no print; dark blue (#2F5C9A) knee-length shorts; a mid-blue (#3A6FB0) school backpack worn on both shoulders; and dark brown sandals.
> Expression: quiet and a little tired but responsive, calm eyes, a relaxed mouth. Pose: standing, both hands holding his backpack straps.
> Full-body view, standing, eye-level camera, body turned about 15 degrees toward the viewer's right, the whole figure visible from the top of the head to the soles of the shoes with a small margin, feet flat on one ground line, centred, on a plain flat light-grey (#D8D8D8) background, vertical portrait canvas.

**Edit prompt, concerned**
> Same child, same outfit, backpack, colours, scale, camera and leg stance exactly as in the input image. Change only the face and posture of the head and shoulders: tired and a little unsure, eyes lowered, shoulders slightly drooped, inner brows slightly raised; hands still on the straps. He looks tired, not sick: no tears, no sweat, no rash. Keep the plain light-grey background. [AVOID-C]

**Edit prompt, positive**
> Same child, same outfit, backpack, colours, scale, camera and leg stance exactly as in the input image. Change only the face and right arm: a small shy smile, brighter eyes; right hand raised in a small wave, left hand on the strap. Keep the plain light-grey background. [AVOID-C]

**Negative:** [NEG-C], plus `sitting, crying, tears, sweat, rash, thermometer, bandage, sickly, thin, ragged clothes`

---

## 4. Backgrounds

**Style reference for every background after BG-02 is approved:** the approved `bg_community_meeting.png`.

**Layout references** (image-to-image only):
- `Assets\Art\01_Scenario_1\02_Environments\Kivu_Clinic_Interior.png`
- `Assets\Art\01_Scenario_1\02_Environments\Kivu_Community_Outdoor_Meeting_Area.png`

These are composition references only. Use a low or medium structure strength so the new image takes the layout but not the painted rendering or the garbled text.

**Negative for all backgrounds:** [NEG-B], plus the extra terms listed per background.

### 4.1 BG-02 Community meeting area (P1). Generate first: this is the golden background.

**Prompt**
> 2D game background illustration in a clean-line cel style designed to sit behind cel-shaded characters: simplified shapes, flat colour areas with one soft shadow tone, thin coloured outlines on foreground objects only and no black outlines, atmospheric perspective that fades distant hills toward pale blue-grey (#9DBBC8), medium-low saturation, calm and uncluttered, light coming from the upper left. Wide 16:9 composition, eye-level camera at standing height, horizon at about 45% of the image height. The top 15% of the image is calm and low in detail. The bottom third is plain, open floor or ground with little detail. The left and right thirds have open, empty floor space where characters will stand. No people in the foreground.
> Scene: a village meeting place in the hills above Lake Kivu on a clear morning. A large old shade tree on the left frames the scene, its canopy reaching across the top-left. Under it, simple wooden benches stand in a loose semicircle on swept red-brown earth (#B5653A). A few villagers sit on the far-left and far-right benches in the mid-ground, small in frame, seen from behind or in profile, in everyday clothes and headwraps. The centre of the space is open. Behind: single-storey plastered houses in clay and whitewash with grey and rusted corrugated-iron roofs, pole fences, banana plants and a few eucalyptus trees. Beyond them, green terraced hills (#6F9A5A) step down to the blue lake (#4F86A6) with wooded islands, and distant hazy volcanic peaks. Soft white clouds. Two goats grazing far off in the mid-ground on the right, and a village hen near a fence.

**Image-to-image variant:** attach `Kivu_Community_Outdoor_Meeting_Area.png` and add: "Use the reference only for the layout: tree on the left, benches, houses, lake. Remove the presenter and the easel completely. Redraw everything in the clean cel style described."

**Negative extra:** `easel, chart, whiteboard, standing presenter, speaker, microphone, sheep, crowd faces in close-up`

### 4.2 BG-01 Clinic interior (P1)

**Prompt**
> [STYLE-B, verbatim]
> Scene: the consultation room of a small public health centre in the Lake Kivu region, morning. The walls are cream (#F7F2E7) above a painted teal (#2E7D6B) lower band, and the floor is light terracotta tile. Left: a padded blue examination couch with a metal frame and a small step stool beside it. Left wall: a window with a blue frame showing green hills and a strip of lake. Centre-back: a simple wooden desk with a closed laptop, a small potted plant and a neat stack of blank folders, with a plain wooden chair behind it and two blue visitor chairs in front of it. Back wall: a wooden shelf unit with neatly arranged plain white and blue boxes with no labels, and a closed blue door with a small window. Right: a white handwashing sink with a soap dispenser, a metal trolley with a few plain supply boxes, and a yellow lidded bin. On the walls, two blank pale-blue paper sheets pinned up, with no writing and no pictures. A plain ceiling with one flat ceiling light. Open floor at the left-centre and right-centre for characters to stand.

**Image-to-image variant:** attach `Kivu_Clinic_Interior.png`. "Keep this room layout. Replace every poster, sign, label and the clock with blank wall or blank paper. Redraw in the clean cel style described."

**Negative extra:** `poster with pictures, handwashing poster, clock, consultation room sign, One Health sign, medical cross, x-ray, syringe, pills, skeleton, anatomy chart`

### 4.3 BG-03 Farm / animal enclosure (P1)

**Prompt**
> [STYLE-B, verbatim]
> Scene: a family smallholding on a terraced hillside above Lake Kivu, overcast and damp just after rain, soft even light still coming from the upper left. Centre-right mid-ground: a sturdy enclosure of wooden poles (a kraal) holding two calm, healthy Ankole cattle with large lyre-shaped horns and red-brown coats, standing and chewing. Beside it, a raised wooden goat house with an iron roof and three small healthy goats (mixed brown, black and white, short coats, beards). Several village chickens with brown and copper plumage peck in the yard. Left mid-ground: a single-storey mud-brick house with a clay-plastered front and a corrugated-iron roof, a water barrel under the roof edge, and a hand hoe leaning by the door. Behind: banana plants, bean and maize terraces, a line of eucalyptus, and a glimpse of the grey-blue lake under low clouds on the hills. The foreground is a damp swept-earth yard with a few small puddles, open and empty at the left and right for characters.

**Negative extra:** `sick animals, lying animals, thin animals, wounds, flies, blood, dead animal, vet treating animal, syringe, Holstein cow, black and white cow, sheep, pig, mud splattered, dirty, garbage`

### 4.4 BG-04 Water source after rain (P1)

**Prompt**
> [STYLE-B, verbatim]
> Scene: a community protected spring on a green hillside near Lake Kivu, just after heavy rain, with the sky beginning to clear and soft light from the upper left. Centre: a small concrete spring box set into the slope, with a metal outlet pipe over a concrete apron and a short stone-lined drainage channel. Three plain yellow 20-litre jerrycans (#E8B634) with no labels wait in a row beside the apron. A footpath of red-brown earth (#B5653A) winds down to it, wet and shiny with small puddles. To one side, muddy rainwater runs down a gully into a small, slightly cloudy brownish pool. Banana plants, ferns and grass around the spring, eucalyptus behind, and terraced hills under lifting grey-white clouds. Calm, ordinary, everyday. Open ground at the left and right for characters.

**Negative extra:** `warning sign, danger sign, skull, hazard symbol, red X, contaminated label, dead fish, dead animal, green slime, foam, oil sheen, chemical drum, pipe dumping waste, people drinking, sick people, brown water pouring from the pipe, flies`

### 4.5 BG-05 Heavy rain, flooded route (P1)

**Prompt**
> [STYLE-B, verbatim]
> Scene: heavy rain in the Lake Kivu hills. A red-brown laterite road comes down from the left foreground toward a low concrete river crossing in the centre mid-ground. The river has risen, and fast brown water flows over the crossing so the road disappears under it: clearly impassable, but not a disaster. Diagonal rain streaks fill the air, puddles spread across the road, the hills are grey-green, and low clouds sit on the ridges. On the near side, a motorbike is parked safely at the roadside under a tree. Banana plants and eucalyptus line the road. Muted, cool palette (#C9CFD3 sky, #5C6670 slate, #8A4A2B wet earth). No people near the water. Open road surface in the left and right foreground.

**Negative extra:** `people in water, car swept away, destroyed houses, rescue boat, emergency vehicles, lightning, dramatic storm, dead animals, debris, warning sign`

### 4.6 BG-06 Field-team / coordination base (P1)

**Prompt**
> [STYLE-B, verbatim]
> Scene: the covered veranda and open office door of a small rural sector field office in the Lake Kivu region, daytime, overcast. A corrugated-iron roof on wooden posts shelters a concrete veranda. Centre-back: a sturdy wooden table holding a two-way radio base set with a handset, a closed laptop, a thermos and neat blank folders. On the wall behind the table, a large blank pale paper map is pinned up, showing only soft coloured shapes with no lines of text. A teal (#2E7D6B) tarpaulin rolled at the edge of the roof, two plastic chairs, a pair of rubber boots and a folded raincoat by the door. Beyond the posts: wet green terraced hills, low cloud and a dirt track. Plain concrete floor in the foreground, open at the left and right for characters.

**Negative extra:** `military, weapons, flags, logos, aid agency vehicles, computer screens with content, whiteboard writing, maps with labels, compass rose`

### 4.7 BG-07 Scenario 4 community briefing (P1). An image-to-image variant of the approved BG-02.

**Edit prompt** (input: the approved `bg_community_meeting.png`)
> Same place, same camera, same tree, benches, houses and lake. Change the time to late afternoon: warm golden light from the upper left, longer soft shadows, a warm sky. More villagers now sit on the benches in the mid-ground at the left and right, small in frame, seen from behind or in profile, men and women of different ages in everyday clothes and headwraps, calm and attentive. In the centre mid-ground, add a simple wooden table holding a plain water container and a few blank paper sheets, with an open speaking spot in front of it and no one standing there. Calm, hopeful, resolved. Keep the foreground open. [AVOID-B]

**Text-to-image fallback:** the BG-02 prompt, with the scene's time changed to "late afternoon, warm golden light" and the table and fuller benches added as described above.

**Negative extra:** `presenter, speaker standing, microphone, banner, bunting with text, stage, podium with logo, flags`

### 4.8 BG-08 Clinic waiting area (P2). Style and colour reference: the approved BG-01.

**Prompt**
> [STYLE-B, verbatim]
> Scene: the waiting area of the same small health centre as the reference image: cream walls with the same teal lower band and the same terracotta tile floor, morning light. Wooden benches along the left wall. A simple wooden reception counter on the right with plain folders and a potted plant. An open double door at the back shows a bright yard and green hills. Two adults sit at the far left end of the benches, small and seen in profile. Blank pale-blue paper sheets pinned to the wall. Open floor at the centre-left and centre-right.

**Negative extra:** the BG-01 extras, plus `queue, crowd, stretcher, crying`

### 4.9 BG-09 Main-menu landscape (P2)

**Prompt**
> [STYLE-B, verbatim, but replace its last three sentences ("The top 15%…", "The bottom third…", "The left and right thirds…") with: "No people anywhere in the image."]
> Scene: a wide panoramic view over Lake Kivu on a clear bright morning. On the right, green terraced hills with a scattered village of clay-walled houses with iron roofs, banana groves and eucalyptus slope down toward the lake. The lake (#4F86A6) fills the middle distance with wooded islands, and hazy blue volcanic peaks rise on the far horizon. The left 45% of the image is mostly calm open sky and lake surface, with soft simple clouds and very little detail, leaving space for a menu panel. Medium-low saturation, inviting and peaceful.

**Negative extra:** `logo, title, text, globe, emblem, anime sky, over-saturated blue, giant volcano in the centre, lens flare`

---

## 5. Maps, diagram, badges, icons and logo

### 5.1 MAP-01 S3 field map base (P1)

**Prompt**
> Top-down illustrated game map in a flat, clean cel style, as if seen from directly above, 16:10. Soft muted palette: pale green fields (#C5DBA0 and #A9C2A0), darker green tree clusters (#6F9A5A), red-brown earth roads (#B5653A), and a blue river (#4F86A6) with pale banks. A river runs from the top to the bottom through the centre, curving gently and widening at the centre where a road crosses it at a low crossing. Two roads: one from the left-centre to the crossing and on to the right side, and one from the left-centre down to a lower-middle clearing. Small clusters of house roofs, grey and rust squares, at the left-centre and upper right. Terraced field patches in soft stripes. A lake edge along the bottom-right corner. Light rain-cloud shadows drift across the map. No text, labels, symbols, compass rose, legend, pins, markers or icons: a clean base for game markers to sit on.

**Negative:** `text, labels, letters, numbers, compass rose, legend, scale bar, pins, markers, icons, perspective view, 3D, isometric, satellite photo, grid`

**Note:** keep the road and river layout matching the placeholder (`Assets\Art\03_Scenario_3\field_map_placeholder.png`), so the A5 caption still describes the image correctly. The placeholder can be used as an image-to-image layout reference at low strength.

### 5.2 MAP-02 Map markers (P1)

Draw these in vector by hand. The prompt below is for concept exploration only.

> Flat vector map pin icon set, consistent style, on a white background: a team-base pin (navy #1F2A44 house glyph), an accessible-site pin (leaf green #4CAF7D ring), a blocked-crossing marker (a coral #D95A4F circle with a horizontal white bar), and farm (cow head), water (droplet) and community (two people) pins in teal #2E7D6B. Each has a white glyph and a #1B423A outline, simple geometric shapes, with no text and no shading.

**Rules:**
- The blocked marker is a "no entry" style bar, **not a skull, X or hazard sign**.
- The water pin is a neutral droplet. It is never red and never carries a warning mark.

### 5.3 GFX-01 One Health vignettes (P1)

Make three separate images. Each is a 1024×1024 transparent PNG with the content inside a circle.

**Shared prefix**
> Circular vignette illustration in a clean-line cel style matching a cel-shaded character cast: thin warm outlines, flat colours with one shadow tone, muted medium saturation, light from the upper left. The whole scene sits inside a perfect circle that fills the canvas; outside the circle is a plain flat light-grey (#D8D8D8) background. No text, no ring border, no arrows.

**People**
> [prefix] Inside the circle: a small group from the Lake Kivu region standing together in front of a simple health post: a young female community health worker in a mid-blue vest holding a tablet, a mother in a headwrap with her young son, and a farmer in a straw hat, all smiling calmly. Green hills behind.

**Animals**
> [prefix] Inside the circle: a calm, healthy Ankole cow with large lyre-shaped horns and a red-brown coat, a small brown-and-white goat, and two village chickens with copper and brown plumage, standing on green grass by a pole fence, terraced hills behind.

**Environment**
> [prefix] Inside the circle: green terraced hills sloping to the blue water of Lake Kivu, a small clear stream running through banana plants and eucalyptus in the foreground, soft clouds and a distant hazy volcanic peak.

**Negative:** [NEG-B], plus `Holstein cow, black and white cow, sheep, ring border, arrows, labels, doctor stethoscope close-up, sick animal`

**Reference:** the approved BG-02, for style. The people vignette can also take the approved Amina and Samuel as identity references, which makes the diagram feel like part of the game.

### 5.4 GFX-02 Badge medal base (P1)

Use a vector model for the concept; a person rebuilds it as an SVG.

> Flat vector game achievement medal, perfectly symmetrical, front view, centred on a plain white background: a round amber (#F2A73B) disc with a cream (#F7F2E7) inner ring and an empty flat amber centre for an emblem; two short ribbon tails hanging straight down from behind the disc, the left in teal (#2E7D6B) and the right in leaf green (#4CAF7D), each with a V-notched end, both exactly the same length. Uniform dark teal (#1B423A) outline, flat colours, no gradients, no bevel, no shine, no star, no text.

**Negative:** `text, letters, numbers, star, laurel, crown, 3D, bevel, gloss, gradient, asymmetric, off-centre ribbon, photorealistic gold`

### 5.5 GFX-03 / GFX-04 Lock icons (closed is P1, open is P2)

**Recommendation:** draw these by hand in vector. They take about 15 minutes, and AI does not reliably draw a seated shackle.

> Flat vector UI icon: a white padlock glyph centred on a rounded-square navy (#1F2A44) tile with a 20% corner radius. The padlock is closed: a thick U-shaped shackle with both legs fully inserted into a rounded rectangular body, and a simple keyhole cut out of the body. Perfectly symmetrical, flat, with no gradient, shadow, text or outline on the glyph.

**Open variant:** "...the padlock is open: the shackle is raised and rotated outward on its left leg, and the right leg is lifted clear of the body."

**Negative:** `3D, gloss, bevel, gradient, gold, chain, key, text, detached shackle, broken shackle`

### 5.6 GFX-05 Logo mark (P1)

Generate concepts only. A designer finalises it in vector and sets the wordmark in Nunito ExtraBold.

> Flat vector emblem for an educational game about One Health, perfectly circular and balanced, on a white background. A circle divided into three equal interlocking segments. One segment is teal (#2E7D6B) with a simple person silhouette (head and shoulders). One is leaf green (#4CAF7D) with a simple cow-head silhouette with lyre-shaped horns. One is amber (#F2A73B) with a leaf above two wave lines. White glyphs, a uniform dark teal (#1B423A) outline, flat colours, geometric and friendly. No text, no globe, no gradients, no bevel, no shine.

**Negative:** `text, letters, globe, earth, map of Africa, 3D, bevel, gloss, gradient, drop shadow, mascot, realistic cow`

**Note:** avoid a map of Africa or a globe. These read as "generic Africa" and were part of the old glossy logo.

### 5.7 GFX-06 Badge emblems (P2)

Make 8 white single-colour glyphs for the medal's centre slot. Draw them in vector. If a concept image helps, use this prompt with each glyph substituted:

> Flat vector icon, a single white glyph on a flat amber (#F2A73B) circle, simple geometric shapes, uniform stroke, centred, with no text: [GLYPH].

Glyphs:
- Evidence Tracker: a magnifying glass over a leaf.
- Joint Response: three interlocking rings.
- One Health Coordinator: a compass rose made of three arrows.
- Community Partner: two clasped hands.
- Responsible Stewardship: a shield with a leaf.
- Precaution With Evidence: a water droplet with a check mark.
- Kivu Coordinator: a hill above a wave.
- One Health Champion: the logo mark.

---

## 6. Hand-off checklist for each generated image

1. Generated with the locked model and settings, references attached as listed, and the prompt pasted unedited (OUTFIT ANCHOR verbatim).
2. Background removed (characters), upscaled if needed, placed on the canvas per spec, and busts cropped with the shared rectangle.
3. Scored with `CRITIQUE_RUBRIC.md`, with the score saved in the sidecar.
4. Deliver to `ArtDrop\<asset ID>\` exactly as `HANDOFF.md` describes. **Do not put files in `Assets/` yourself.** The engineer moves accepted files into `Assets/Art/...` (bible §5.5) and sets the import settings (bible §5.3).
