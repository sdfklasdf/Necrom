# Quarter-view 3D preparation — 2026-10-10
AWU-QUARTERVIEW-01; working title NECRO, release title undecided.

Saved scene: Assets/Necrom/QuarterView3D/QuarterView3D.unity.
Separate opt-in preparation scene; existing Canvas FirstPlayable remains unchanged.
Main Camera: orthographic, X45/Y45, distance24, near0.3/far60, Forward, HDR off.
Board fixture 7x16 rotated Y45; aspect-aware full bounds framing with 6% margin.
Directional Light: one, hard shadows, requested custom resolution1024, strength0.65.
Runtime profile: HardOnly/Medium/40m/one cascade/one pixel light/2xMSAA; soft particles and realtime reflection probes off.
Shared Standard materials, instancing enabled; 120 collider-free cubes are layout fixtures.

Evidence: validation.txt actual native Unity6000.3.25f1 PlayMode, bounds.txt, editor-540x960.png,
play-540x960.png, play-390x844.png, play-360x640.png, fresh-readback.txt serialized reload,
core-green.trx (128 passed), red.txt preimplementation absence check.
Camera.Render captures were taken during actual PlayMode, not a device build or performance benchmark.
Initial validation failed because setting shadowCascades=0 returned1. Corrected to explicit1.
recovery.txt records restoration to disk Ultra baseline after script reload lost the temporary edit-time snapshot.
Final validation verifies apply/restore and original clean scene restoration.

Not run: purchased asset Import, animated120 rigs, mobile build/device FPS/thermal/draw-call profiling,
FirstPlayable 3D integration, HUD/safe-area composition. No blanket mobile performance PASS.
Original three font dirty assets and untracked test bin remain protected/excluded from this commit.
Next: wait for purchased package download and Import order; inspect representative assets first.

After successful PlayMode/serialized readback, Editor exited (TMP quit clearing logged). Reopen attempt failed UPM local IPC after30s; final Editor not running. Existing font dirty state restored byte-for-byte from protected pre-AWU copies; post-quit files retained separately in ignored Artifacts/Font-After-Unity-Quit-20261010. No security settings or packages modified.
