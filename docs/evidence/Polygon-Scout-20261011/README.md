# Purchased POLYGON scout visual acceptance — 2026-10-11
Overall: PARTIAL. Static render and placement PASS; animation motion BLOCKED_NO_CLIPS.

## Scope and actual import
Founder reported purchase and requested Import at06:56+09. Native Unity6000.3.25f1 AssetDatabase confirms Assets/Synty/PolygonFantasyCharacters:27 prefabs, of which12 characters and15 props. The Synty import totals2739 files/165867475 input bytes and adds ShaderGraph17.3.0 to already-dirty package manifests.
No further purchase/download, bulk120 replacement, battle integration, stats, synergy, or roster changes.

## AWU1 — saved placement PASS
Existing Assets/Necrom/QuarterView3D/DummyFormation.unity now contains real vendor3D models, not the old square sprites. Three types repeated3 times in the existing3x3 fixture:
- MON_001: SM_Chr_Male_Rouge_01
- MON_002: SM_Chr_Female_Witch_01
- MON_003: SM_Chr_Male_Wizard_01
Identity bindings are isDummy=false. Native prefab links remain intact. Mappings are arbitrary visual QA selections, not final monster lore/trait assignments.
Actual baked geometry heights1.35 units; yaw225/upright; grid1.75x2.2; original vendor material variants01_A/B/C via instance overrides. Source prefab bytes unchanged. Existing production/dummy catalog validation still expects sprites and was not converted into a production3D pipeline.
Saved scene reopened successfully, exact model links/IDs/heights/materials read back. X45/Y45 orthographic camera retained.

## AWU2 — actual native PlayMode
390x844 GameView was verified. Images captured by Camera.Render DURING native PlayMode, not a device build or mockup.
-9 real skinned model instances, supported Synty/Generic_Basic shader and actual atlas references.
-Baked geometry projected rectangles:70.83..80.83px wide,55.77..58.11px tall; no clipping; pair overlaps0.
-Scoped runtime errors0; initial verification-tool errors remain separately recorded.
-Actual avatars9/9 valid humanoid; motion controllers/clips0. Vendor AssetDatabase clip paths0; Characters.fbx embedded clips0 and importAnimation0.
-T-pose is visible. Idle/walk/attack timing/deformation/root motion/animated overlap NOT RUN. No procedural motion substituted for missing animations.
-Original FirstPlayable scene restored clean and global quality restored All/High/150/4/4/2.
Visual inspection:three costume silhouettes/IDs discernible; normal atlas colors and hard shadows, no pink missing-material render. Similar brown palettes and T-pose limit final army readability. No claim of polished production art or120-rig/mobile performance.

## Measurement failures retained
First Renderer.bounds projection reported20 overlaps and was overly conservative. Switched to actual baked vertices. A useScale=false snapshot then underestimated geometry after TransformPoint and incorrectly reported0; invalidated by a fresh useScale=true reopen check, which revealed3 actual projected-rect overlaps. Unity6.3 API scale compensation was verified against official docs; final probe uses true, grid widened1.4->1.75, and fresh saved/native checks pass0.
A REFIT job submitted before refreshed assembly loaded failed Unknown command; replay after compilation succeeded. Earlier PASS lines are intermediate, final useScaleTrue/native logs govern.

API reference: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SkinnedMeshRenderer.BakeMesh.html

## Preservation and backup
Existing protected TMP font3 SHA and starting imported Packages/manifest+lock bytes unchanged.
No intentional source material write. Vendor material01_A/B/C hashes changed during Editor usage; observed Built-in shader keyword reclassification, likely automatic Unity serialization (inference). Full pre-run material bytes were not saved; starting hashes retained. Do not claim every vendor byte stayed unchanged. See preservation.txt.
Current purchased content locally archived at C:/Dev/Necrom/Artifacts/Polygon-20261011/Synty-import-local-backup.zip; SHA256 in preservation.txt. Purchased vendor folder and package edits remain uncommitted. Git backup of owned scene/probe/logs/screenshots does not include purchased content; fresh clones require the same pack Import and ShaderGraph dependency.
Only scene, opt-in Editor probe and evidence are committed for this order. FullCore162 previous evidence not rerun:domain/gameplay source unchanged. Native new-script compilation and selected live scene checks actually executed.
Next dependency: obtain/approve compatible humanoid motion clips and verify these same3 models before widening scope.
