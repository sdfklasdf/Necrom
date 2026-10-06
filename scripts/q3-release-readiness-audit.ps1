param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$failures = New-Object System.Collections.Generic.List[string]

function Write-Check([string]$Name, [bool]$Pass, [string]$Detail) {
    $state = if ($Pass) { 'PASS' } else { 'FAIL' }
    Write-Output ("{0}|{1}|{2}" -f $state, $Name, $Detail)
    if (-not $Pass) { $script:failures.Add($Name) }
}

function Check-Hash([string]$RelativePath, [string]$Expected) {
    $path = Join-Path $RepoRoot $RelativePath
    if (-not (Test-Path $path)) {
        Write-Check $RelativePath $false 'missing'
        return
    }

    $actual = (Get-FileHash $path -Algorithm SHA256).Hash.ToUpperInvariant()
    Write-Check $RelativePath ($actual -eq $Expected.ToUpperInvariant()) ("sha256=" + $actual)
}

Set-Location $RepoRoot
Write-Output 'Q3_RELEASE_READINESS_AUDIT_V1'
Write-Output ('repo=' + $RepoRoot)

$artProvPath = Join-Path $RepoRoot 'Assets\Necrom\FirstPlayable\Art\Q3\ProductionCandidate\provenance.json'
$audioProvPath = Join-Path $RepoRoot 'Assets\Necrom\FirstPlayable\Audio\Q3ProductionCandidate\PROVENANCE.json'

Write-Check 'art provenance json' (Test-Path $artProvPath) $artProvPath
Write-Check 'audio provenance json' (Test-Path $audioProvPath) $audioProvPath

if (Test-Path $artProvPath) {
    $art = Get-Content $artProvPath -Raw | ConvertFrom-Json
    foreach ($entry in $art.files) {
        Check-Hash ('Assets\Necrom\FirstPlayable\Art\Q3\ProductionCandidate\' + $entry.file) $entry.sha256
        Write-Check ('art generation id ' + $entry.file) (-not [string]::IsNullOrWhiteSpace($entry.generationId)) $entry.generationId
        Write-Check ('art rights boundary ' + $entry.file) ($entry.rightsStatus -match 'UNKNOWN') $entry.rightsStatus
    }
}

if (Test-Path $audioProvPath) {
    $audio = Get-Content $audioProvPath -Raw | ConvertFrom-Json
    foreach ($entry in $audio.assets) {
        Check-Hash ('Assets\Necrom\FirstPlayable\Audio\Q3ProductionCandidate\' + $entry.file) $entry.sha256
        Write-Check ('audio task id ' + $entry.file) (-not [string]::IsNullOrWhiteSpace($entry.taskId)) $entry.taskId
        Write-Check ('audio prompt ' + $entry.file) (-not [string]::IsNullOrWhiteSpace($entry.prompt)) ('chars=' + $entry.prompt.Length)
    }
    Write-Check 'audio legal boundary remains open' ($audio.finalReleaseRightsLegal -eq 'UNKNOWN_NOT_RUN') $audio.finalReleaseRightsLegal
    Write-Check 'audio physical-device mix remains open' ($audio.physicalDeviceMix -eq 'NOT_RUN') $audio.physicalDeviceMix
}

$fonts = @{
    'Assets\Necrom\FirstPlayable\Fonts\Production\NotoSansKR-Regular.otf' = '69975A0AC8472717870AEFEAB0A4D52739308D90856B9955313B2AD5E0148D68'
    'Assets\Necrom\FirstPlayable\Fonts\Production\NotoSansKR-Medium.otf' = 'B46988EF13E8BAC08F3933AF686EAF770972994F9B6D335BE0184D60169B5431'
    'Assets\Necrom\FirstPlayable\Fonts\Production\NotoSansKR-Bold.otf' = '5A6CEB287ED2FC6CFC6213144EBEA68CBD94B20FC9EB873D8486493BF02D9BDA'
}
foreach ($item in $fonts.GetEnumerator()) {
    Check-Hash $item.Key $item.Value
}

foreach ($sdf in @(
    'Assets\Necrom\FirstPlayable\Fonts\Production\NotoSansKR-Regular SDF.asset',
    'Assets\Necrom\FirstPlayable\Fonts\Production\NotoSansKR-Medium SDF.asset',
    'Assets\Necrom\FirstPlayable\Fonts\Production\NotoSansKR-Bold SDF.asset'
)) {
    Write-Check $sdf (Test-Path (Join-Path $RepoRoot $sdf)) 'runtime TMP SDF asset'
}

$licensePath = Join-Path $RepoRoot 'Assets\Necrom\FirstPlayable\Fonts\OFL-CJK.txt'
$licensePresent = Test-Path $licensePath
Write-Check 'font license file present' $licensePresent $licensePath
if ($licensePresent) {
    $license = Get-Content $licensePath -Raw
    Write-Check 'font license identifies SIL OFL 1.1' ($license -match 'SIL OPEN FONT LICENSE Version 1\.1') 'license text matched'
}

foreach ($doc in @(
    'docs\evidence\Q3-PRODUCTION-ART-REFINEMENT.md',
    'docs\evidence\Q3-PRODUCTION-UI-FIDELITY.md',
    'docs\evidence\Q3-PRODUCTION-MOTION-AUDIO.md'
)) {
    Write-Check $doc (Test-Path (Join-Path $RepoRoot $doc)) 'evidence document present'
}

$testExpectations = @{
    'Artifacts\EV020-final2-targeted.xml' = 10
    'Artifacts\EV020-final2-full.xml' = 134
}
foreach ($item in $testExpectations.GetEnumerator()) {
    $path = Join-Path $RepoRoot $item.Key
    if (-not (Test-Path $path)) {
        Write-Check $item.Key $false 'missing'
        continue
    }

    [xml]$xml = Get-Content $path -Raw
    $run = $xml.'test-run'
    $pass = ($run.result -eq 'Passed' -and
             [int]$run.passed -eq $item.Value -and
             [int]$run.failed -eq 0)
    Write-Check $item.Key $pass ("result={0}; passed={1}; failed={2}" -f $run.result, $run.passed, $run.failed)
}

Write-Output 'BOUNDARY|final release-rights/legal clearance|UNKNOWN_NOT_RUN'
Write-Output 'BOUNDARY|Founder creative audition|NOT_RUN'
Write-Output 'BOUNDARY|representative physical mobile acceptance|NOT_RUN'
Write-Output 'BOUNDARY|real-user fun/retention|NOT_RUN'
Write-Output ('FAILURE_COUNT=' + $failures.Count)

if ($failures.Count -gt 0) {
    Write-Output ('FAILED_CHECKS=' + ($failures -join ','))
    exit 1
}

Write-Output 'AUDIT_RESULT=PASS_EVIDENCE_INTEGRITY_ONLY'
Write-Output 'LEGAL_APPROVAL=NOT_ASSERTED'
exit 0
