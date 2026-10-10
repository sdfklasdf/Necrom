# UPM recovery — AWU-UPM-RECOVERY-01
Reproduced prior30s Editor IPC failure on2026-10-11 KST.
Remote process environment lacked ProgramData. Direct UPM server failed with
ERR_INVALID_ARG_TYPE at getLocalConfigFolder; official diagnostic failed at same boundary.
ComSpec-only and TMP-only did not fix diagnostics. ProgramData-only restored the tool:
6 checks passed, system proxy auto-config check UNKNOWN (not enabled).
Registry, download latency/speed, proxy-environment check, and UPM IPC all passed.

Safe remote launch prerequisite:
$env:ProgramData = [Environment]::GetFolderPath('CommonApplicationData')
This sets only the child process environment, no global registry/firewall/security change.
Binary present and --version returned9.26.1; no packages/caches deleted or manifest edits.

Separate direct-launch license failure: no valid Editor license, exit198.
Opened installed Unity Hub using existing authenticated session, then opened Necrom there.
No password entry, new license acceptance, purchase, or asset search performed.
Hub launch recovered licensing and normal Editor/UPM; live read-only Client.List and
Client.Search('com.unity.ugui') verified actual Editor connection.
Health trigger initially collided with writer lock; probe now defers IOException and
triggers use atomic temp→rename. Prior probe exception retained in Console history.

Evidence: red-editor.txt, red-server.txt, diagnostic.txt, license-separate-block.txt,
health.txt and package-manager.png (native screenshot).
Future remote direct launches must supply ProgramData and have active Hub licensing.
Asset Store purchased download/import remains NOT RUN; no bought files currently supplied.
Existing font3 dirty bytes protected; test bin excluded. Existing battle code untouched.
