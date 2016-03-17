Import-Module psake

$nuget = (Get-Command nuget.exe).Path
$msbuild = (Get-Command msbuild.exe).Path
$hg = (Get-Command hg.exe).Path
$git = (Get-Command git.exe).Path
$nunit = (Get-Command $PSScriptRoot\packages\NUnit.ConsoleRunner.3.2.0\tools\nunit3-console.exe -ErrorAction SilentlyContinue).Path
$localPackageSource = (Resolve-Path "C:\src\packages")
$solutionFile = Resolve-Path $PSScriptRoot\Treesor.sln

Task default -depends build

Task package_restore {

    & $nuget restore

} -precondition { Test-Path $nuget }

Task clean {

    & $msbuild $solutionFile /t:Clean /p:Configuration=Release
    
    Remove-Item $PSScriptRoot\*.nupkg -ErrorAction SilentlyContinue
    #Remove-Item $PSScriptRoot\packages -Recurse
}

Task build {

    & $msbuild $solutionFile /t:Build /p:Configuration=Debug

} -precondition { Test-Path $msbuild } -depends package_restore

#Task test {
#
#    & $nunit (Resolve-Path $PSScriptRoot/Elementary.Hierarchy.Test/Elementary.Hierarchy.Test.csproj)
#
#} -precondition { Test-Path $nunit } -depends build,package_restore    