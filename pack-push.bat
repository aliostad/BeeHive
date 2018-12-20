del /F /Q .\artifacts\*.*
dotnet pack BeeHive.sln -o ..\..\artifacts
dotnet nuget push "artifacts\*.nupkg" -s nuget.org