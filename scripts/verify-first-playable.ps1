param([string]$Label='red',[string]$Filter='FirstPlayableCanonicalCompositionTests',[string]$Method='',[string]$Project='C:\Dev\Necrom-playmode-recovery')

$psi=New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName='C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe'
$psi.UseShellExecute=$false
$psi.EnvironmentVariables['HOME']=$env:USERPROFILE
$psi.EnvironmentVariables['TMP']=$env:TEMP
$psi.EnvironmentVariables['PROGRAMDATA']='C:\ProgramData'
$psi.EnvironmentVariables['ALLUSERSPROFILE']='C:\ProgramData'
$base='-batchmode -projectPath "'+$project+'" -logFile "'+$project+'\Artifacts\AWU4-'+$Label+'.log"'
if($Method){
$psi.Arguments=$base+' -executeMethod '+$Method
if(-not $Method.EndsWith('ImportReviewFont')){$psi.Arguments+=' -quit'}
}else{
$psi.Arguments=$base+' -runTests -testPlatform PlayMode -testResults "'+$project+'\Artifacts\AWU4-'+$Label+'.xml"'
if($Filter -and $Filter -ne 'ALL'){$psi.Arguments+=' -testFilter '+$Filter}
}
$p=[System.Diagnostics.Process]::Start($psi)
$p.WaitForExit()
Write-Output ('UNITY_EXIT='+$p.ExitCode)
exit $p.ExitCode
