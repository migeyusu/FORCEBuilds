#support raw nuget
$call="C:\nuget.exe"
#& $call setApiKey 02881b7b-d6c4-44c1-9d58-4e7f9a96f80f
$path=dir *.csproj  -name
if($path.Length -eq 0)
{
echo "没有csproj文件"
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
exit
}
& $call pack $path -IncludeReferencedProjects
$nupkgfile=gci -Name -Filter *.nupkg | select -Last 1
#echo $nupkgfile
# $x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
& $call push $nupkgfile  -Source http://localhost/LocalNuGet/api/v2/package #https://www.nuget.org/api/v2/package
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")