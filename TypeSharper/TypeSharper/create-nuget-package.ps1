rm -r -fo ./nuget-package
dotnet build -c Release
dotnet pack --include-symbols -c Release --output ./nuget-package