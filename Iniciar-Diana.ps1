$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$apiProject = Join-Path $PSScriptRoot 'PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj'

function Invoke-DotnetStep {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo completar: dotnet $($Arguments -join ' '). Revisar el error anterior antes de continuar."
    }
}

Invoke-DotnetStep -Arguments @('tool', 'restore')
Invoke-DotnetStep -Arguments @('restore', $apiProject)
Invoke-DotnetStep -Arguments @('build', $apiProject, '--no-restore')
Invoke-DotnetStep -Arguments @('ef', 'database', 'update', '--project', $apiProject, '--no-build')
Invoke-DotnetStep -Arguments @('run', '--project', $apiProject, '--no-build', '--', '--seed')
Write-Host 'API en http://localhost:5077. OpenAPI: /openapi/v1.json. Ctrl+C para detener.'
Invoke-DotnetStep -Arguments @('run', '--project', $apiProject, '--no-build', '--launch-profile', 'http')
