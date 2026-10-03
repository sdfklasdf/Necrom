param([string]$Player='C:\Dev\Necrom-playmode-recovery\Artifacts\VisualPlayer\NecromVisual.exe')
$ErrorActionPreference='Stop'
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class NecromInput {
 [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X,Y; }
 [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left,Top,Right,Bottom; }
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h,out RECT r);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h,ref POINT p);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint dx,uint dy,uint data,UIntPtr extra);
}
'@
$base=Split-Path $Player
$root=Join-Path $base 'VisualEvidence'
$began=[DateTime]::UtcNow
$psi=New-Object Diagnostics.ProcessStartInfo
$psi.FileName=$Player
$psi.UseShellExecute=$false
$psi.Arguments='--necro-visual-qa --necro-native-input -screen-fullscreen 0 -logFile "'+(Join-Path $base 'native-player.log')+'"'
$proc=[Diagnostics.Process]::Start($psi)
$last=0
$runDir=$null
$records=New-Object 'System.Collections.Generic.List[string]'
try {
 while(-not $proc.HasExited -and ([DateTime]::UtcNow-$began).TotalSeconds -lt 60){
  $latest=Join-Path $root 'latest-run.txt'
  if((Test-Path $latest) -and (Get-Item $latest).LastWriteTimeUtc -ge $began){
   $runDir=Join-Path $root (Get-Content $latest -Raw).Trim()
   $request=Join-Path $runDir ('input-request-'+($last+1)+'.txt')
   if(Test-Path $request){
    $v=(Get-Content $request -Raw).Split('|')
    if($v.Length -eq 5 -and [int]$v[0] -gt $last){
     $proc.Refresh()
     $handle=$proc.MainWindowHandle
     if($handle -eq [IntPtr]::Zero){throw 'Own player window unavailable'}
     $rect=New-Object NecromInput+RECT
     if(-not [NecromInput]::GetClientRect($handle,[ref]$rect)){throw 'Cannot read own player client rectangle'}
     $virtualPixels="$($rect.Right-$rect.Left)x$($rect.Bottom-$rect.Top)"
     [void][NecromInput]::SetThreadDpiAwarenessContext([IntPtr](-4))
     if(-not [NecromInput]::GetClientRect($handle,[ref]$rect)){throw 'Cannot read DPI-aware client rectangle'}
     $records.Add("DPI="+[NecromInput]::GetDpiForWindow($handle)+" virtual="+$virtualPixels+" physical=$($rect.Right-$rect.Left)x$($rect.Bottom-$rect.Top)")
     if(($rect.Right-$rect.Left) -ne [int]$v[3] -or ($rect.Bottom-$rect.Top) -ne [int]$v[4]){throw "Actual client pixels mismatch: actual=$($rect.Right-$rect.Left)x$($rect.Bottom-$rect.Top), requested=$($v[3])x$($v[4])"}
     if(-not [NecromInput]::SetForegroundWindow($handle)){throw 'Could not focus own player'}
     $point=New-Object NecromInput+POINT
     $point.X=[int]$v[1]
     $point.Y=[int]$v[4]-[int]$v[2]
     if(-not [NecromInput]::ClientToScreen($handle,[ref]$point)){throw 'Cannot convert own client input coordinates'}
     if(-not [NecromInput]::SetCursorPos($point.X,$point.Y)){throw 'OS cursor input failed'}
     [NecromInput]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
     Start-Sleep -Milliseconds 50
     [NecromInput]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
     $last=[int]$v[0]
     $records.Add(([DateTime]::UtcNow.ToString('o'))+" request=$last client=$($v[1]),$($v[2]) screen=$($point.X),$($point.Y) pixels=$($v[3])x$($v[4])")
     $records|Set-Content (Join-Path $runDir 'native-input-driver.txt') -Encoding UTF8
    }
   }
  }
  Start-Sleep -Milliseconds 100
 }
 if(-not $proc.HasExited){throw 'Native player evidence timed out'}
 $proc.WaitForExit()
 if($proc.ExitCode -ne 0 -or -not $runDir -or -not(Test-Path (Join-Path $runDir 'complete.txt')) -or $last -ne 10){
  throw "Incomplete native input evidence: exit=$($proc.ExitCode), clicks=$last"
 }
 Write-Output "NATIVE_INPUT_PASS=10 real OS clicks; PLAYER_EXIT=$($proc.ExitCode); run=$runDir"
} finally {
 if(-not $proc.HasExited){Stop-Process -Id $proc.Id}
}
