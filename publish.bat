dotnet publish TescoCsvConv.csproj -c Release --self-contained true -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true
copy bin\Release\net5.0\win-x64\publish\*.* C:\Util
copy TescoFix.bat C:\Util