"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\msbuild.exe" db.lib-csharp-server.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\msbuild.exe" src/dBridges.csproj /t:pack /p:configuration=release