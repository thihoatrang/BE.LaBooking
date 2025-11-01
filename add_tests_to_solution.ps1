# PowerShell script to add test projects to solution
$solutionPath = "E:\BE_github\BE.LaBooking\LawAppointmentApp.sln"
$basePath = "E:\BE_github\BE.LaBooking"

# List of test projects to add
$testProjects = @(
    "Users.Application.Tests\Users.Application.Tests.csproj",
    "Appointments.Application.Tests\Appointments.Application.Tests.csproj",
    "Lawyers.Application.Tests\Lawyers.Application.Tests.csproj",
    "Chat.Application.Tests\Chat.Application.Tests.csproj",
    "Users.Services.API.Tests\Users.Services.API.Tests.csproj",
    "Appointments.Services.API.Tests\Appointments.Services.API.Tests.csproj",
    "Lawyers.Services.API.Tests\Lawyers.Services.API.Tests.csproj",
    "Chat.Services.API.Tests\Chat.Services.API.Tests.csproj",
    "API.Gateway.Tests\API.Gateway.Tests.csproj",
    "Users.Infrastructure.Tests\Users.Infrastructure.Tests.csproj",
    "Appointments.Infrastructure.Tests\Appointments.Infrastructure.Tests.csproj",
    "Lawyers.Infrastructure.Tests\Lawyers.Infrastructure.Tests.csproj"
)

Write-Host "Adding test projects to solution..." -ForegroundColor Green

foreach ($project in $testProjects) {
    $projectPath = Join-Path $basePath $project
    if (Test-Path $projectPath) {
        Write-Host "Adding: $project" -ForegroundColor Yellow
        dotnet sln $solutionPath add $projectPath
    } else {
        Write-Host "Project not found: $projectPath" -ForegroundColor Red
    }
}

Write-Host "`nDone! All test projects have been added to the solution." -ForegroundColor Green
Write-Host "You can now build and test using: dotnet build" -ForegroundColor Cyan
Write-Host "Or run all tests using: dotnet test" -ForegroundColor Cyan

