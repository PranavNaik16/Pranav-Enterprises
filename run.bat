@echo off
echo ========================================================
echo Starting Pranav Enterprise Web Application (.NET 8.0)
echo ========================================================
set PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%
dotnet run --project src/PranavEnterprises.Web/PranavEnterprises.Web.csproj --urls "http://localhost:5000"
pause
