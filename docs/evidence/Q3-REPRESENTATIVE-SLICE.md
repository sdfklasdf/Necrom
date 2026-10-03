# Q3 representative visual/content slice — EV-014

## Verdict
PASS for the bounded representative review slice; Q3 VERTICAL SLICE remains PARTIAL / IN_PROGRESS. Existing Q2 / FP-29 acceptance is preserved, not reimplemented. This is a review art candidate, not approved commercial final art.

DEV routing: DEV-02-05/06/07/08/13 and DEV-04-09/10. Q3 exit still requires final commercial direction, copy/assets/audio/motion, responsive/accessibility and representative device acceptance.

## Independent work units
| Unit | Input | Work | Output | Done condition | Evidence boundary | Readback / verification | Failure return | Downstream impact |
|---|---|---|---|---|---|---|---|---|
| Q3-A visual contract | Existing HUD 11:89 and token/state contract; no approved final art direction | Create Guard, raised form, necromancer, background; compose actual 390x844 frames; explicit full-Formation disabled CTA | Four PNG review assets, provenance, native Figma frames and capacity component | Actual assets compose with existing 14-state contract; full capacity treatment explicit | DESIGNED; review candidate only | Figma screenshots and stable node IDs; source PNGs reused in runtime | Correct composition/state/token mismatch before runtime acceptance | Runtime art/layout input |
| Q3-B runtime presentation | Canonical saved scene/prefab and production combat/roster loops | Wire art, subscribe actual damage/roster/contributor events; player-facing Korean review copy; fix text overflow and short-screen geometry | Saved scene/prefab, presentation component, procedural attack/hit/defeat/Raise/contribution feedback | Reopened scene/prefab retain 4/4 art refs; actual normal path produces feedback; no duplicate layer | IMPLEMENTED / EXECUTED; procedural motion, no authored animation clips or final audio | Editor reopen readback, five targeted tests, real Windows player input | Fix wiring/overflow/protected-area failures then rerun affected tests | Representative slice playback |
| Q3-C evidence closure | Q3-A/B and existing Q2 acceptance | Fresh full regression, build, actual input, three ratios, Figma comparison | XML/logs, 30 runtime PNGs, three Figma PNGs, report | Current build/input/responsive proof and explicit remaining boundary | VALIDATED within desktop review scope | 5/5 targeted,129/129 PlayMode; build GUID; 10 native clicks; 12 geometry checks | Failed attempts never promoted; rerun after corrections | Next Q3 quality work and portable handoff |

## Actual implementation
- Imported sprite artifacts: guard.png, raised.png, player.png (1024x1536 RGBA); background.png (1448x1086). Transparent character alpha and sprite import are real. Runtime renders these assets using RawImages.
- FirstPlayableVisualPresentation observes production player/allied AttackApplied results and actual allied roster changes. It does not insert a test-only state adapter.
- Attack lunge, hit tint, defeat collapse, Raise pulse and exact-actor allied contribution cues play from actual game events.
- Canonical scene: Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity.
- Canonical prefab: Assets/Necrom/FirstPlayable/Prefabs/FirstPlayable.prefab.
- Editor menu automation FirstPlayableQ3Art applies/imports the assets, saves the scene/prefab and reopens/readbacks them. Actual saved artifacts exist.
- Review typeface is static Noto Sans CJK KR Regular with OFL-CJK.txt bundled. Provenance/license text exists; production typography approval/legal clearance remains UNKNOWN.
- Five allied slots are five instances of the current Guard-derived unit, not five monster species. Launch count UNDECIDED; three regular plus one boss is only a proposal.

## Fresh executed evidence
Raw Unity logs listed below are preserved byte-for-byte inside artifacts/Q3/raw-unity-logs.zip; XML and scene readback are loose files.
| Check | Result | Artifact |
|---|---|---|
| Canonical import/reopen | PASS: scene and prefab art4/4; regular font source; canonical components19 | artifacts/Q3/Q3-art-readback.txt; final-reopen.log |
| Targeted current tests | PASS 5/5 | artifacts/Q3/final-targeted.xml / final-targeted.log |
| Entire relevant current PlayMode | PASS 129/129 | artifacts/Q3/final-full.xml / final-full.log |
| Windows development build | PASS, zero compile errors | artifacts/Q3/final-build.log |
| Native input, production loop | PASS, 10 real OS clicks, 6 encounters, 5 Raises | artifacts/Q3/native-input/run.txt,complete.txt,native-input-driver.txt and 18 PNGs |
| Responsive run | PASS,390x844 /768x1024 /360x640,4 states each | artifacts/Q3/responsive/run.txt,complete.txt and12 PNGs |
| Protected unit geometry | PASS12/12; overlay1 each; current text overflow0 | Responsive metadata |
| Figma/runtime comparison | PASS within review candidate scope; residuals below | figma-battle.png,figma-raised.png,figma-full-formation.png and runtime PNGs |

Final native run: 20261003T184511014-49990614e99a4222a0bbaf4306a5e1b1.
Counters: player attack7,hit20,defeat6,Raise5,allied contribution13,visible allies5; last actual contributor ally:35. Full army correctly disables Raise. ArmyProofPending at the last Raise reflects the newest unit not yet contributing; older allies' damage is not misattributed to it.
Responsive run: 20261003T184629652-b78ea5ef430645768a002be5d7be63e0.
Build GUID: 30898652adf74ebba940ed64edf2c2ac.
Managed runtime DLL SHA256: 2D575C70B3B81CE1D182C164822635D0AC3AB66D1D98115C6F4BC103AE8A2A2B.
The executable is a Unity launcher stub; its unchanged hash is NOT proof of updated gameplay code.
Player artifact on the verified PC: C:\Dev\Necrom\Artifacts\VisualPlayer\NecromVisual.exe.

## Figma canonical readback
File eXqKU1qHXsn52SJfIGltZo; HUD node11:89 retains its original14 semantic states.
- Battle frame18:72: https://www.figma.com/design/eXqKU1qHXsn52SJfIGltZo?node-id=18-72
- Raised frame18:78: https://www.figma.com/design/eXqKU1qHXsn52SJfIGltZo?node-id=18-78
- Full-Formation frame18:84: https://www.figma.com/design/eXqKU1qHXsn52SJfIGltZo?node-id=18-84
- Capacity component set19:90: Available / Full; full capacity is CTA treatment, not a fifteenth semantic state.

| Intent | Actual mapping / comparison | Finding |
|---|---|---|
| Semantic truth | Target3 /Raise7 /Army4 preserved | 14 renderer states;9 canonical captured;5 boundary states |
| Theme | DarkSurface #202532;Soul #087f5b;Arcane #6741d9;Secondary #4d5566;Info #1864ab;Danger #c92a2a;white | Existing verified colors retained |
| Typography | 12/16,20/28,14/20; regular Korean review fonts | Runtime CJK KR vs Figma KR family; final family not approved |
| HUD layout | Panel padding16,gap8,radius18; 390x844 stack x24,width342,y362.24/490.24/682.24 | Existing zone intent preserved |
| Combat | Combat viewport y33.76,height312.48 at390x844; protected region excludes HUD | Unit art scales to protected region at short portrait |
| CTA | Raise48 minimum; Next44 rounded12; full disabled “군단 가득 참” | Explicit Figma/runtime treatment |
| Defeated body | Vertical collapse to .35,opacity.65 | Figma CROP image transform repaired to match settled runtime intent |

Material mismatches actually corrected:
1. Block diagnostic markers replaced with four actual review assets.
2. Variable-font Thin face and Korean text overflow corrected with static Regular source and font-metric leading. New overflow test first FAILED, then PASSED.
3. 360x640 Guard escaped protected area; a new geometry test first FAILED, then PASSED after scale was constrained by protected height.
4. Next CTA increased from undersized square treatment to44px rounded; full-Formation CTA/design extension added.
5. Figma initial and committed semantic instances corrected; defeated-body transform clipping repaired.

Residual review differences: font-family/rasterization differences, very small ally silhouettes and short-portrait detail legibility, unapproved candidate direction/final copy, no authored rig/frame animation or final audio. No claim of pixel-identical fidelity or approved final art.

## State evidence boundary
Canonical captured9:
Target Active/Defeated;
Raise TargetNotReady/Eligible/CommittedAwaitingProof/ProofObserved;
Army Empty/ProofPending/ProofObserved.
Component-boundary5:
Target None;
Raise NoTarget/SourceUnavailableOrConsumed/InsufficientSoul;
Army Owned.
These five remain renderer/component boundary coverage, not ordinary canonical gameplay screenshots. No fabricated normal-path capture.

## Limitations and next exit gaps
- PHYSICAL DEVICE / MOBILE INSTALL /1080x2400 actual pixel launch: NOT RUN.
- SafeArea evidence is desktop simulated 4% vertical insets, not physical-device SafeArea validation.
- FINAL FONT/COPY/ICON/ALL ASSET RIGHTS: UNKNOWN / NOT RUN; included generated art is review candidate, not release-approved.
- Actual user fun, retention and performance acceptance: NOT RUN / UNKNOWN.
- Authored production animation/audio and final direction convergence: DEFERRED to next Q3 work.
- Known legacy TMP enableWordWrapping obsolete warning remains; build had zero errors, not zero warnings.
- Failed pre-fix runs had no PASS promotion. Final XML/logs above are fresh; old124/124 is not reused.
- Subsequent serialization whitespace cleanup was nonsemantic; final saved scene/prefab reopen passed after cleanup.

## How the developer can play now
Unity Hub: open C:\Dev\Necrom with6000.3.25f1, open the canonical FirstPlayable scene, choose portrait Game view, press Play. Allow automatic combat, click Raise after defeat, then Next Encounter. The generated review art and actual contribution feedback are in the normal path.
Alternatively launch the local Windows player above without QA arguments. This does not establish mobile installation.

## Next priority
Q3 presentation quality closure: small-screen silhouette/readability plus authored attack/hit/defeat/Raise/allied motion and audio feedback with explicit provenance and normal-path playback. Preserve existing gameplay acceptance; do not expand monster count before representative quality converges. Fresh-read Drive CURRENT/HANDOFF/EV-014 and this report before continuing.
