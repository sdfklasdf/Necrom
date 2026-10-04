$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '..\Assets\Necrom\FirstPlayable\Audio\Q3Review'
New-Item -ItemType Directory -Force -Path $root | Out-Null

function Write-ReviewWav {
    param(
        [string]$Name,
        [double]$Duration,
        [double]$StartHz,
        [double]$EndHz,
        [double]$Noise,
        [int]$Seed
    )
    $sampleRate = 22050
    $count = [int]($sampleRate * $Duration)
    $path = Join-Path $root $Name
    $stream = [System.IO.File]::Create($path)
    $writer = New-Object System.IO.BinaryWriter($stream)
    try {
        $dataBytes = $count * 2
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
        $writer.Write([int](36 + $dataBytes))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('WAVE'))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('fmt '))
        $writer.Write([int]16)
        $writer.Write([int16]1)
        $writer.Write([int16]1)
        $writer.Write([int]$sampleRate)
        $writer.Write([int]($sampleRate * 2))
        $writer.Write([int16]2)
        $writer.Write([int16]16)
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('data'))
        $writer.Write([int]$dataBytes)

        $rng = New-Object System.Random($Seed)
        $phase = 0.0
        for ($i = 0; $i -lt $count; $i++) {
            $t = $i / [double][Math]::Max(1, $count - 1)
            $hz = $StartHz + ($EndHz - $StartHz) * $t
            $phase += 2.0 * [Math]::PI * $hz / $sampleRate
            $attack = [Math]::Min(1.0, $t / 0.08)
            $release = [Math]::Pow(1.0 - $t, 2.4)
            $env = $attack * $release
            $tone = [Math]::Sin($phase) + 0.28 * [Math]::Sin($phase * 2.01)
            $rand = (($rng.NextDouble() * 2.0) - 1.0) * $Noise
            $sample = ($tone * (1.0 - $Noise) + $rand) * $env * 0.42
            $sample = [Math]::Max(-1.0, [Math]::Min(1.0, $sample))
            $writer.Write([int16]([Math]::Round($sample * 32767.0)))
        }
    }
    finally {
        $writer.Dispose()
        $stream.Dispose()
    }
    Write-Output ("GENERATED " + $path)
}

# Deterministic synthetic review cues. No third-party samples are used.
Write-ReviewWav 'attack-review.wav' 0.14 520 150 0.34 1101
Write-ReviewWav 'hit-review.wav' 0.10 170 70 0.58 1102
Write-ReviewWav 'defeat-review.wav' 0.30 240 48 0.20 1103
Write-ReviewWav 'raise-review.wav' 0.36 170 720 0.12 1104
Write-ReviewWav 'ally-contribution-review.wav' 0.12 760 390 0.16 1105

$provenance = @'
Q3 review SFX provenance
Generator: scripts/generate-q3-review-sfx.ps1
Method: deterministic additive tone/noise synthesis written directly to PCM WAV
Third-party samples: none
License dependency: none for generated waveforms
Status: REVIEW CANDIDATE ONLY; production sound design / loudness / device mix approval NOT RUN
'@
Set-Content -Path (Join-Path $root 'PROVENANCE.txt') -Value $provenance -Encoding UTF8
