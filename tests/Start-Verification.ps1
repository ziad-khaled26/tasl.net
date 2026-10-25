$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$previousConnection = $env:ConnectionStrings__CinemaDb
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
Push-Location -LiteralPath $taskRoot
try {
    $env:ConnectionStrings__CinemaDb = 'Server=.;Database=CinemaBooking_Verification;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=False'
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'EF tool restore failed.' }
    dotnet ef database update --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext
    if ($LASTEXITCODE -ne 0) { throw 'Verification database migration failed.' }
    dotnet run --project CinemaBooking --no-launch-profile --urls http://127.0.0.1:5208
    if ($LASTEXITCODE -ne 0) { throw 'Verification server failed.' }
}
finally {
    $env:ConnectionStrings__CinemaDb = $previousConnection
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    Pop-Location
}
