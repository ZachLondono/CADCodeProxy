$filepath = ".\CADCodeProxy.csproj"
[xml]$project = get-content $filePath
$version = $project.Project.PropertyGroup.Version 
Invoke-Expression "..\Scripts\nuget.exe add '.\\bin\\Release\\net7.0\\publish\\CADCodeProxy.$version.nupkg' -Source 'R:\\Development\\nuget\'"
