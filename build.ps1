<#
.SYNOPSIS
	Собирает все независимые решения микросервисов проекта из корня репозитория.

.DESCRIPTION
	Каждый микросервис (UserService, EventService, BookingService) имеет собственный
	.slnx-файл и собирается независимо. Скрипт последовательно вызывает `dotnet build`
	для каждого из них, чтобы дать единую команду для локальной сборки/CI без объединения
	сервисов в общее решение.

.PARAMETER Configuration
	Конфигурация сборки (Debug/Release). По умолчанию Debug.

.EXAMPLE
	./build.ps1
	./build.ps1 -Configuration Release
#>
param(
	[string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$solutions = @(
	"UserService/UserService.slnx",
	"EventService/EventService.slnx",
	"BookingService/BookingService.slnx"
)

$repoRoot = $PSScriptRoot

foreach ($solution in $solutions) {
	$solutionPath = Join-Path $repoRoot $solution
	Write-Host "==> Building $solution ($Configuration)" -ForegroundColor Cyan

	dotnet build $solutionPath --configuration $Configuration
	if ($LASTEXITCODE -ne 0) {
		Write-Error "Build failed for $solution"
		exit $LASTEXITCODE
	}
}

Write-Host "All solutions built successfully." -ForegroundColor Green
