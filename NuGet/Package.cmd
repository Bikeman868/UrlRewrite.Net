cls
rem pushd "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC"
rem call vcvarsall.bat x86
rem popd

rem pushd ..\Solution
rem set msbuild = "%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
rem %msbuild% UrlRewrite.sln /p:Configuration=Release-Net40 /p:Platform="Any CPU"
rem %msbuild% UrlRewrite.sln /p:Configuration=Release-Net45 /p:Platform="Any CPU"
rem popd

nuget.exe pack UrlRewrite.Net.nuspec
