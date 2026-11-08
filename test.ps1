Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " Running Pranav Enterprise Unit Test Suite (xUnit)" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan
$env:Path = "$env:LOCALAPPDATA\Microsoft\dotnet;$env:Path"
dotnet test tests/PranavEnterprises.Tests/PranavEnterprises.Tests.csproj --logger "console;verbosity=normal"
