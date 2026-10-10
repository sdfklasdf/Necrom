# Dummy formation visual QA — AWU-DUMMY-3D-01
Scene: Assets/Necrom/QuarterView3D/DummyFormation.unity.
QA variant of QuarterView3D; base scene and existing MON prefab/catalog files unchanged.
MON_001/002/003 are existing 2D square Sprite Prefabs, each repeated3 times in world space.
The base scene's120 cube proxies are disabled in this QA variant only.
No purchased assets searched/imported; no gameplay, synergy calculation, or content stats changed.

Baseline: original prefab rotation, grid pitch1.05 x0.76.
At390x844 sprites appear slanted/foreshortened and9 pairs of projected rectangles overlap.
Compared fixture: camera-facing rotation, pitch1.4 x2.2, nine ground markers and ID labels.
Test-only spacing/orientation, not a approved combat grid or balance decision.
X45/Y45 orthographic camera retained. Rendering sizes540x960,390x844,360x640.
Final actual PlayMode:9 instance identities/serialized Sprite resolution, unclipped sprite
rectangles and0 overlaps in each size, scoped Unity errors0.
Sprite dimensions:71.95px,52.56px,47.97px respectively; equal across front/back rows
because orthographic projection preserves screen size.
Visual inspection: nine sprites and labels distinct, front/back rows and columns readable.

Evidence: baseline-390x844.png; editor-540x960.png; play-{size}.png;
metrics.txt; validation.txt; readback.txt.
Captures use Camera.Render during native Unity PlayMode, not synthetic mockups/device screenshots.
Final saved scene reopened: nine prefab instances, all threeMON identities/Sprite refs, overlap0;
original clean scene and prior global quality restored.

Failed QA tool attempts preserved:
initial-failure.txt: asset reference obtained before single-scene transition became stale;
reloading the visual catalog after OpenScene resolved it without catalog edits.
runtime-check-failure.txt: editor-only Prefab source metadata check inappropriate in PlayMode;
runtime now checks MON identity and actual Sprite ref, edit-mode checks saved Prefab links.
readback-return-failure.txt: original Scene handle unloaded; original path now cached before transition.
These are verification-tool failures, not hidden gameplay fixes.

Limits: flat Sprites do not prove real3D model silhouette/depth/rig/material/shadow readability.
No attack animation/effect/HUD/120-rig/mobile performance or actual combat integration tested.
Import readiness is limited to restoredEditor/UPM plus available test data pipeline;
representative purchased assets must be checked before bulk Import.
