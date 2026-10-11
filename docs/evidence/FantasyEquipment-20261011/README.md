# Fantasy Import and scout equipment QA — 2026-10-11

AWU1 PASS for native import/path/GUID scope; AWU2 PASS for the nine-scout visual preview and sampled native PlayMode scope. Unity 6000.3.25f1, C:/Dev/Necrom. Final visual run 11:50:50–11:50:59 KST; original clean FirstPlayable scene restored at 11:51:00. No production combat integration or 120-character expansion.

## AWU1

Sequential native Unity imports completed and AssetDatabase/path/GUID readback passed:

| Package | Manifest paths | Canonical root |
|---|---:|---|
| Goblin War Camp | 3018 | Assets/Synty/PolygonGoblinWarCamp |
| Viking Realm | 3367 | Assets/Synty/PolygonVikingRealm |
| Goblin Locomotion | 569, with 8 optional dependency entries isolated | Assets/Synty/AnimationGoblinLocomotion |
| Goblin Fighters | 552 | Assets/Synty/SidekickCharacters |
| Alpine Mountain | 1982 | Assets/Synty/PolygonNatureBiomes and PNB_Core |
| Particle FX | 1639 | Assets/Synty/PolygonParticleFX |

Models/animation FBXs, textures and prefabs are present under vendor roots. Native Synty inventory399 non-preview clips,398 human clips; no Attack/Cast/Spell/Magic named clips. The inventory's initial SidekickGoblinFighters label0 was an incorrect search label: canonical SidekickCharacters import552 paths passed. Simple Props/Items/Icons and modern-building packages excluded. import-plan.json is the seven-package preflight plan; its Simple entry was NOT imported.

Optional Goblin sample scripts initially failed compilation because Unity Input System is absent. Only five newly imported demo scripts plus companion directory/input data were moved outside Assets to Artifacts/FantasyImport-20261011/quarantine. No protected Packages changes. Fresh native compilation/readback passed; historical errors retained. Vendor demo scenes are not supported without their optional dependencies. Alpine's empty NotActive folder was omitted by native import; restored original package folder metadata/GUID, refreshed natively and readback passed. No content file was missing.

## AWU2

Free Human Spellcasting Animations acquired through Remote Desktop Commander Edge UI from https://kevdev.itch.io/human-spellcasting-animations-free (free skip-donation flow). Download17507716 bytes. Native import manifest246 paths passed. Two shared texture/folder GUIDs resolve to existing canonical Basic Motions paths. Native controller readback confirms the selected spell clips are humanoid.

| Scout, three instances each | Weapon | Attack state clip |
|---|---|---|
| MON_001 Rogue | New Goblin SM_Wep_Dagger_01, left hand | HumanM@Attack1H01_L |
| MON_002 Witch | New Goblin SM_Wep_Staff_01, right hand | HumanF@MagicAttackCall1H01_L |
| MON_003 Wizard | New Goblin SM_Wep_Staff_02, right hand | HumanM@MagicAttackDirect1H01_L |

Three preview controllers retain Idle/Walk; Witch/Wizard use real spellcasting instead of one-hand melee swings. Weapon source prefabs remain unchanged. Root motion disabled. Built-in Foot IK persisted in all9 controller states. Only local preview scene instances changed. Measured Idle sole/robe clearance was reduced by worldY offsets .026/.0242/.0234; original scene backup retained. Final Idle minY=.035 matches tile surfaceY=.035. Walking body/robe minima .0319–.0447 and casting .0339–.0379 are diagnostics; robe bounds do not prove hidden foot contact.

Final native Screen.width=390 and height=844 asserted. Idle48 + Walk25 + Attack/Cast44 =117 sampled frames. All nine avatars valid, real bone motion; all attack states progressed through at least one complete cycle (normalizedTime>1 assertion). Sampled projected mesh/weapon overlaps0, clipping0, root drift0, scoped runtime errors0. PNGs are native PlayMode camera renders, not fabricated GameView screenshots. Ten390x844 PNGs plus six1560x3376 native detail renders all belong to the final run; earlier stale attack60 capture moved to Artifacts history.

Visual inspection of Idle/Walk/attack/Cast representative standard/detail captures found no obvious joint inversion, visible clothing/body penetration or harmful weapon/body crossing. Foot IK and placement eliminated visible standing hover; walking retains expected lifted swing foot. Observed visual issue count0 in the reviewed captures. This is sampled quarter-view preview QA, not universal all-frame/all-camera certification, exact physical sole collision, actual-device QA or complete vendor pack material certification. No automatic attack transitions/gameplay damage/VFX production behavior added; Attack is a manually exercised preview state.

## Preservation and correction history

Before AWU1: fonts3 + Packages2 + original DummyFormation scene (6 files) SHA256 unchanged; all2739 existing Synty files SHA256 unchanged. Backups in Artifacts/FantasyImport-20261011: protected-before hierarchy, Synty-before.zip112382842 bytes, hash CSVs. Scene changes after this checkpoint are intentional equipment/grounding edits; do not interpret original scene hash preservation as the final scene state.

Kevin-before-spell.zip was created before AWU2 import. An initial PowerShell preflight used wildcard paths: [RM] filenames were incorrectly treated as new, and16 existing Run FBXs were overwritten by the imported package. Detected by full ZIP hash comparison, restored exactly those16 from the backup; metadata was unchanged. Fresh final comparison739 original Kevin files SHA256 mismatches0 (kevin-preservation.txt). Future checks use LiteralPath; no blanket reset/stash/clean used. Earlier failures remain in evidence, followed by successful verification.

Protected fonts3/Packages2 and prior untracked vendor/bin/auth-wall items are excluded from staging. Purchased/free vendor files remain local untracked; Git backs up the owned fixture/controllers/probes/evidence, not paid source packages. A fresh clone requires lawful separate vendor imports and the free animation pack. Original Desktop packages and backup ZIPs remain on the Windows device.

Read-only independent review found no critical defect; requested full-cycle assertion was implemented and validated. Persisted Foot IK9/9 verified. Native saved-scene readback and preservation checks precede scoped Commit/Push; final commit ID is recorded in CURRENT/HANDOFF.
