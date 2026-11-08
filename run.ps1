Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " Starting Pranav Enterprise Web Application (.NET 8.0)" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan
$env:Path = "$env:LOCALAPPDATA\Microsoft\dotnet;$env:Path"
Write-Host "Web Portal listening at: http://localhost:5000" -ForegroundColor Yellow
dotnet run --project src/PranavEnterprises.Web/PranavEnterprises.Web.csproj --urls "http://localhost:5000"
