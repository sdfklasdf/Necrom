# AWU 2 — three dummy character pipeline
Actual Unity 6000.3.25f1: persisted ScriptableObject -> stable MON ID -> asset key -> prefab -> Sprite references validated, then instantiated and rendered in isolated DummyPipeline scene PlayMode. Scoped errors = 0. Fresh domain readback passed after recompile. Original FirstPlayable scene restored clean in edit mode.
| ID | Origin | Class | Joker | asset key |
|---|---|---|---|---|
| MON_001 | Skeleton | Guardian | Reaper | dummy/MON_001 |
| MON_002 | Plague | Warlock | Mimic | dummy/MON_002 |
| MON_003 | Dragon | Ranger | Vampire | dummy/MON_003 |
These assignments are arbitrary test fixtures, not balance/product decisions. Opt-in one Origin + one Class + optional Joker authoring convention is isolated; legacy 120-character data and combat remain unchanged. Ghoul is a display alias of canonical Plague.
64x64 box PNG Sprites, three prefabs, Catalog.asset, VisualCatalog.asset, and isolated scene live under Assets/Necrom/ContentDraft/Dummy. Dummy identity flags distinguish placeholders.
Unknown ID, duplicate ID, wrong key, missing prefab and missing TFT definitions were rejected in Unity. Core negative assignment tests rejected two Origins, missing Class and noncanonical alias. Full suite: 128 passed / zero failed/skipped; .NET 8 target using process-only .NET 10 Major rollforward. Expected preimplementation failures: assignment-red.trx, unity-red.txt.
The first camera capture had overlapping long labels; labels were split across lines and the final capture was inspected. Refresh job initially raced the old probe before recompile; fresh-readback.txt is from the verified new domain. A PowerShell whitespace cleanup used a wrong relative .NET base path and failed without changing those files; absolute-path cleanup corrected it.
Unity generated glyph/character/atlas changes in three existing NotoSansKR dynamic SDF assets while saving/loading. These are preserved, copied to ignored Artifacts/Protected-Font-20261010, and excluded from the dummy commit. See font-dirty.txt/font-hashes.txt. Existing untracked test bin directory is preserved/excluded. Never describe the entire worktree as clean.
FirstPlayableGameplayComposition SHA256 remains FCE9893FE0A160C30F1F75D1E19F95AE7371F495DD86B68C2FB4A70A3A2B6226; Resources monster/synergy hashes remain unchanged. No production roster replacement, combat effect executor or synergy calculation hookup.

## Purchase judgment
GO for continued asset selection/purchase planning: the 120-record authoring schema and 3-sample stable-ID visual pipeline have passed; no blocker found in those verified paths.
Vendor-specific compatibility is UNVERIFIED until a package is selected. A PNG/Sprite/Prefab pipeline does not prove Spine/3D rig/animation/shader/render-pipeline support, art quality, mobile performance or licensing. Before purchase, check listed Unity/render-pipeline compatibility, format and commercial-use terms; a demo/sample, if available, can be used for representative import. No nonexistent files are requested.
Next recommended AWU after purchase: isolated representative import and MON mapping/animation/scale/render checks before bulk import. Numeric synergy tiers, effect execution and roster assignments remain planning work for Minseok + Gemini.
