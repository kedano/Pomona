

echo "Building Nuget packages"



.nuget\nuget.exe pack app\Pomona.Common\Pomona.Common.csproj -build -symbols -OutputDirectory build
.nuget\nuget.exe pack app\Pomona\Pomona.csproj -build -symbols -OutputDirectory build
