$ErrorActionPreference='Stop'
$a='C:\Dev\Necrom-playmode-recovery\Artifacts'
$d='C:\Dev\Necrom\docs\evidence\awu-5'
New-Item -ItemType Directory -Force $d | Out-Null
Add-Type -AssemblyName System.Drawing
$sets=@(@{Name='native';Dir="$a\VisualPlayer\VisualEvidence\20261003T172043846-6c4fdaddc223462983df28af041e304c";Count=11},@{Name='responsive';Dir="$a\VisualPlayer\VisualEvidence\20261003T172130982-3d65b347c5684b3989f239ad55ae3d1e";Count=12})
$report=@()
foreach($set in $sets){
 if(!(Test-Path "$($set.Dir)\complete.txt")){throw 'Missing completion'}
 $pngs=@(Get-ChildItem $set.Dir -Filter '*.png');if($pngs.Count-ne$set.Count){throw 'Wrong capture count'}
 foreach($png in $pngs){
  $t=Get-Content ([IO.Path]::ChangeExtension($png.FullName,'.txt'))
  if($t-notcontains'overlayCount=1' -or ($t-match'overflow=True')){throw 'Overlay/overflow violation'}
  $size=[regex]::Match($png.Name,'^(\d+)x(\d+)')
  $b=New-Object Drawing.Bitmap($png.FullName)
  if($b.Width-ne[int]$size.Groups[1].Value -or $b.Height-ne[int]$size.Groups[2].Value){throw 'PNG dimensions wrong'};$b.Dispose()
  $rects=@{}
  foreach($zone in @('TargetStatusReadabilityZone','RaiseActionStatusReadabilityZone','ArmyStatusReadabilityZone','ProtectedCombatReadabilityZone','CombatViewport')){
   $line=@($t|Where-Object{$_-like"$zone=*"});if($line.Count-ne1){throw 'Missing geometry'}
   $matches=[regex]::Matches($line[0],'\((-?\d+\.\d+), (-?\d+\.\d+), (-?\d+\.\d+)\)')
   if($matches.Count-ne4){throw 'Invalid rectangle'}
   $ys=@($matches|ForEach-Object{[double]$_.Groups[2].Value})
   $rects[$zone]=@(($ys|Measure-Object -Minimum).Minimum,($ys|Measure-Object -Maximum).Maximum)
  }
  foreach($zone in @('TargetStatusReadabilityZone','RaiseActionStatusReadabilityZone','ArmyStatusReadabilityZone')){
   if($rects[$zone][1]-gt$rects['ProtectedCombatReadabilityZone'][0]){throw 'Protected combat overlap'}
  }
  $report+="$($set.Name)/$($png.Name): dimensions/overlay/no-overflow/5-zones/protected-separation PASS"
 }
 New-Item -ItemType Directory -Force "$d\$($set.Name)" | Out-Null
 Get-ChildItem $set.Dir | Copy-Item -Destination "$d\$($set.Name)" -Recurse -Force
}
foreach($label in @('closure-targeted','closure-regression')){
 [xml]$x=Get-Content "$a\AWU4-U5-$label.xml"
 if($x.'test-run'.result-ne'Passed' -or [int]$x.'test-run'.failed-ne0){throw 'Regression failed'}
 $report+="$label $($x.'test-run'.passed)/$($x.'test-run'.total) PASS; start=$($x.'test-run'.'start-time') end=$($x.'test-run'.'end-time')"
 Copy-Item "$a\AWU4-U5-$label.xml" $d
 Copy-Item "$a\AWU4-U5-$label.log" $d
}
Copy-Item "$a\AWU4-U5-native-build2.log" $d
Copy-Item "C:\Dev\Necrom\Artifacts\AWU4-U5-main-readback.log" $d
Get-ChildItem C:\Dev\Necrom\Artifacts -Filter '*readback*' | ForEach-Object {Copy-Item $_.FullName $d}
$report+='buildGUID=48db96ec38a94ca5b0ad55c08339b24b'
$report+='playerSHA256='+ (Get-FileHash "$a\VisualPlayer\NecromVisual.exe" -Algorithm SHA256).Hash
$files=@('FirstPlayableCombatHudPresenter.cs','FirstPlayableCombatHudUnityView.cs','FirstPlayableDamageDeathPipeline.cs','FirstPlayableGameplayComposition.cs','FirstPlayableRuntimeVisualCapture.cs')
foreach($f in $files){
 $p="Assets\Necrom\FirstPlayable\Runtime\$f"
 $m=(Get-FileHash "C:\Dev\Necrom\$p").Hash;$r=(Get-FileHash "C:\Dev\Necrom-playmode-recovery\$p").Hash
 if($m-ne$r){throw "Source mismatch $f"};$report+="$p SHA256=$m main=recovery"
}
foreach($f in @('FirstPlayableIntegratedLoopTests.cs','FirstPlayableCombatHudRendererFoundationTests.cs')){
 $p="Assets\Necrom\Tests\PlayMode\$f"
 $m=(Get-FileHash "C:\Dev\Necrom\$p").Hash;$r=(Get-FileHash "C:\Dev\Necrom-playmode-recovery\$p").Hash
 if($m-ne$r){throw "Test mismatch $f"};$report+="$p SHA256=$m main=recovery"
}
$report | Set-Content "$d\verification.txt" -Encoding UTF8
$report
Get-ChildItem $a -Filter '*U5*' | Select-Object -ExpandProperty Name
