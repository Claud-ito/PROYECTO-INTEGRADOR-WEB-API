param(
    [string]$BaseUrl = 'http://localhost:5077',
    [int]$ModuloConPisosId,
    [string]$OutputPath,
    [string]$DatabaseName = 'No especificada',
    [string]$VerificationContext = 'API integrada'
)

$ErrorActionPreference = 'Stop'
$results = [System.Collections.Generic.List[object]]::new()

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null
    )

    $request = @{
        Uri = $BaseUrl.TrimEnd('/') + $Path
        Method = $Method
        SkipHttpErrorCheck = $true
        ErrorAction = 'Stop'
    }
    if ($null -ne $Body) {
        $request.ContentType = 'application/json'
        $request.Body = $Body | ConvertTo-Json -Compress -Depth 5
    }

    $response = Invoke-WebRequest @request
    $bodyText = if ($response.Content -is [byte[]]) {
        [System.Text.Encoding]::UTF8.GetString($response.Content)
    }
    else {
        [string]$response.Content
    }
    $result = [pscustomobject]@{
        method = $Method
        path = $Path
        requestBody = $request.Body
        status = [int]$response.StatusCode
        location = [string]$response.Headers.Location
        responseBody = $bodyText
    }
    $results.Add($result)
    return $result
}

function Assert-Status {
    param([object]$Result, [int]$Expected)
    if ($Result.status -ne $Expected) {
        throw "$($Result.method) $($Result.path): esperaba $Expected y recibió $($Result.status). $($Result.responseBody)"
    }
}

$suffix = [guid]::NewGuid().ToString('N').Substring(0, 8).ToUpperInvariant()
$codigo = "T3-$suffix"
$codigoOtro = "T3-$suffix-B"
$moduloId = 0
$otroId = 0
$passed = $false

try {
    Assert-Status (Invoke-Api GET '/api/modulos') 200
    Assert-Status (Invoke-Api GET '/api/modulos/999999') 404
    Assert-Status (Invoke-Api GET '/api/modulos/0') 400
    Assert-Status (Invoke-Api GET '/api/modulos?soloActivos=invalido') 400

    $created = Invoke-Api POST '/api/modulos' @{ codigo = " $codigo "; nombre = ' Modulo de prueba ' }
    Assert-Status $created 201
    $modulo = $created.responseBody | ConvertFrom-Json
    $moduloId = [int]$modulo.moduloId
    if ($modulo.codigo -ne $codigo -or $modulo.nombre -ne 'Modulo de prueba' -or !$modulo.activo) {
        throw 'POST no devolvió los campos normalizados y activo=true.'
    }
    if (!$created.location.EndsWith("/api/modulos/$moduloId")) {
        throw "El encabezado Location no apunta al módulo creado: $($created.location)"
    }
    Assert-Status (Invoke-Api GET "/api/modulos/$moduloId") 200
    Assert-Status (Invoke-Api POST '/api/modulos' @{ codigo = $codigo.ToLowerInvariant(); nombre = 'Duplicado' }) 400
    Assert-Status (Invoke-Api POST '/api/modulos' @{ codigo = '   '; nombre = 'Nombre' }) 400
    Assert-Status (Invoke-Api POST '/api/modulos' @{ codigo = "T3-$suffix-X"; nombre = '   ' }) 400
    Assert-Status (Invoke-Api POST '/api/modulos' @{ codigo = '123456789012345678901'; nombre = 'Largo' }) 400
    Assert-Status (Invoke-Api POST '/api/modulos' @{ codigo = "T3-$suffix-Y" }) 400

    Assert-Status (Invoke-Api PUT "/api/modulos/$moduloId" @{ codigo = $codigo.ToLowerInvariant(); nombre = ' Modulo actualizado '; activo = $false }) 204
    $updated = Invoke-Api GET "/api/modulos/$moduloId"
    Assert-Status $updated 200
    $updatedBody = $updated.responseBody | ConvertFrom-Json
    if ($updatedBody.nombre -ne 'Modulo actualizado' -or $updatedBody.activo) {
        throw 'PUT no persistió el nombre o la baja lógica.'
    }
    $activeList = Invoke-Api GET '/api/modulos?soloActivos=true'
    Assert-Status $activeList 200
    if (($activeList.responseBody | ConvertFrom-Json | Where-Object moduloId -eq $moduloId).Count -ne 0) {
        throw 'soloActivos=true incluyó un módulo inactivo.'
    }
    Assert-Status (Invoke-Api PUT "/api/modulos/$moduloId" @{ codigo = $codigo; nombre = 'Sin activo' }) 400
    Assert-Status (Invoke-Api PUT '/api/modulos/999999' @{ codigo = $codigo; nombre = 'Inexistente'; activo = $true }) 404

    $other = Invoke-Api POST '/api/modulos' @{ codigo = $codigoOtro; nombre = 'Otro módulo' }
    Assert-Status $other 201
    $otroId = [int](($other.responseBody | ConvertFrom-Json).moduloId)
    Assert-Status (Invoke-Api PUT "/api/modulos/$moduloId" @{ codigo = $codigoOtro.ToLowerInvariant(); nombre = 'Duplicado'; activo = $true }) 400

    if ($ModuloConPisosId -gt 0) {
        Assert-Status (Invoke-Api DELETE "/api/modulos/$ModuloConPisosId") 400
    }

    Assert-Status (Invoke-Api DELETE "/api/modulos/$otroId") 204
    Assert-Status (Invoke-Api GET "/api/modulos/$otroId") 404
    $otroId = 0
    Assert-Status (Invoke-Api DELETE "/api/modulos/$moduloId") 204
    Assert-Status (Invoke-Api GET "/api/modulos/$moduloId") 404
    $moduloId = 0
    Assert-Status (Invoke-Api DELETE '/api/modulos/999999') 404
    Assert-Status (Invoke-Api DELETE '/api/modulos/0') 400

    $passed = $true
    Write-Output "Correcto: $($results.Count) solicitudes HTTP verificadas."
}
finally {
    if ($OutputPath) {
        $evidence = [pscustomobject]@{
            executedAt = (Get-Date).ToString('yyyy-MM-ddTHH:mm:sszzz')
            baseUrl = $BaseUrl
            databaseName = $DatabaseName
            verificationContext = $VerificationContext
            moduloConPisosId = $ModuloConPisosId
            passed = $passed
            results = $results
        }
        $evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding utf8
    }
}
