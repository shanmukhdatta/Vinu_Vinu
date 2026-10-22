# Generate a short ORIGINAL, low-volume, unprotected stereo WAV for test routing.
$ErrorActionPreference = 'Stop'
$dest = Join-Path (Join-Path $PSScriptRoot '..') 'test-audio.wav'
$sampleRate = 48000
$channels = 2
$seconds = 8
$frames = $sampleRate * $seconds
$bytesPerSample = 2
$dataBytes = $frames * $channels * $bytesPerSample
$writer = [System.IO.BinaryWriter]::new([System.IO.File]::Open($dest, [System.IO.FileMode]::Create))
try {
  $writer.Write([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
  $writer.Write([int](36 + $dataBytes))
  $writer.Write([System.Text.Encoding]::ASCII.GetBytes('WAVE'))
  $writer.Write([System.Text.Encoding]::ASCII.GetBytes('fmt '))
  $writer.Write([int]16)
  $writer.Write([uint16]1)
  $writer.Write([uint16]$channels)
  $writer.Write([int]$sampleRate)
  $writer.Write([int]($sampleRate * $channels * $bytesPerSample))
  $writer.Write([uint16]($channels * $bytesPerSample))
  $writer.Write([uint16]16)
  $writer.Write([System.Text.Encoding]::ASCII.GetBytes('data'))
  $writer.Write([int]$dataBytes)
  for ($i = 0; $i -lt $frames; $i++) {
    $ramp = [Math]::Min(1.0, [Math]::Min($i, $frames - 1 - $i) / ($sampleRate * 0.05))
    $sample = [int16]([Math]::Round(0.08 * 32767 * $ramp * [Math]::Sin(2 * [Math]::PI * 440 * $i / $sampleRate)))
    $writer.Write($sample)
    $writer.Write($sample)
  }
}
finally { $writer.Dispose() }
Write-Host "Created $dest (8-second 440Hz original stereo WAV at low level)."
