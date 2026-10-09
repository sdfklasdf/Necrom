# EXP UI acceptance (AWU-EXP-FIX-01)
Approved order: EXP layout fix + commit existing account EXP changes. No new combat logic.
Baseline main: 4968eb4367275e6ea8b9e4439c1fa8a0fbb8fed6, 3 modified / 8 untracked status entries (bin is a directory).

## Cause and fix
Baseline label Rect overlaps OpenGacha and DefenseWaveRenderContainer in actual PlayMode; white on white is unreadable. Independent overlay previously used root safe-area anchors, initialized once, and sorting 200 beneath navigation canvases 30000+.
New root Canvas owns scale; child AccountExpSafeArea updates every frame. Dark text on opaque pale backing; layout reserves actual HUD and navigation Rect bounds. Sorting 29990 stays below full-screen modals. All Images are non-raycasting.
Editor zoom 1.3 cropping is an Editor viewing configuration: use fit scale; it cannot be fixed by a runtime anchor.

## Evidence boundaries
- baseline.txt / baseline-0..4: expected failure before fix. Last baseline sample had an invalid viewport; do not reuse it.
- fixed.txt, verified.txt, acceptance.txt and their images are INVALID acceptance: hot reload, stopped background frames or capture timing. Preserved locally for investigation, excluded from approved acceptance set.
- acceptance-final.txt and six matching PNGs: actual running Editor, runtime resolution transitions 390x844 -> 360x640 -> 1080x1920 -> 844x390 -> 768x1024 -> 390x844. Every label/slider avoids HUD/menu Rects; original bar missing/label unreadable symptom removed. Exact PNG dimensions independently checked.
- acceptance-safe: final rerun plus simulated notch safe-area bounds and runtime error counter. Simulated insets are geometry evidence, NOT physical-device/notch evidence.
- .NET domain suite: 105 pass, 0 fail, 0 skipped under installed .NET 10 using DOTNET_ROLL_FORWARD=Major. Native .NET 8 testhost unavailable; original attempt aborted. TRX in ignored Artifacts/EXP-UI-20261009.
- Probe is editor-only and opt-in via Library/exp-ui-job.txt. Diagnostic runInBackground is restored. Offline popup is visually dismissed without granting/deleting its pending reward; normal battle still writes progression during play.

No mobile build, device, Figma parity, complete SP/save fault recovery, exhaustive infinite resolutions or release readiness is claimed.
Existing dirty source files preserved; only AccountExperienceBarUI intentionally changed. Generated Tests/bin output is preserved but excluded from commit.

Final acceptance-safe: six runtime samples PASS, two simulated inset bounds PASS, ERROR_COUNT=0. Existing nine non-UI dirty files match preserved SHA256; intentional UI change only. Invalid probes moved to ignored Artifacts/EXP-UI-20261009/invalid-probes.
