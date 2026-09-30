# Prueba end-to-end del modulo de resenas.
#
# 1. Toma un producto publico existente.
# 2. Verifica que la lista publica de resenas del producto arranca vacia
#    (o al menos no contiene la resena que vamos a crear).
# 3. POST publico -> guarda una resena Pendiente y responde 202 con mensaje.
# 4. Verifica que la lista publica NO la muestra (Aprobado=false).
# 5. Login admin -> cookie HttpOnly nogal_auth.
# 6. GET /api/admin/reviews?estado=pendiente -> ve la nueva resena.
# 7. POST /api/admin/reviews/{id}/aprobar.
# 8. Verifica que la lista publica ahora SI la muestra.
# 9. DELETE /api/admin/reviews/{id} para limpiar.

param(
    [string]$ApiBase = 'http://localhost:5199',
    [string]$AdminUser = 'admin',
    [string]$AdminPass = 'nogal2026'
)

$ErrorActionPreference = 'Stop'

function Post-Json {
    param([string]$Uri, [object]$Body, [Microsoft.PowerShell.Commands.WebRequestSession]$Session = $null)
    $json = $Body | ConvertTo-Json -Depth 6 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    if ($Session) {
        return Invoke-RestMethod -Uri $Uri -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8' -WebSession $Session
    }
    return Invoke-RestMethod -Uri $Uri -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8'
}

function Post-JsonExpectStatus {
    param([string]$Uri, [object]$Body, [int]$Expected)
    $json = $Body | ConvertTo-Json -Depth 6 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    try {
        $r = Invoke-WebRequest -Uri $Uri -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8' -UseBasicParsing
        return [pscustomobject]@{ Status = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        $status = [int]$_.Exception.Response.StatusCode
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $body = $reader.ReadToEnd()
        return [pscustomobject]@{ Status = $status; Body = $body }
    }
}

# --- 1. Producto de prueba ---
$producto = (Invoke-RestMethod -Uri "$ApiBase/api/products?tamano=1" -Method Get).items[0]
if (-not $producto) { throw 'No hay productos en la BD.' }
Write-Host "Producto: $($producto.nombre) [$($producto.slug)]"

# --- 2. Lista publica antes ---
$antesPub = Invoke-RestMethod -Uri "$ApiBase/api/products/$($producto.slug)/reviews" -Method Get
$antesConteo = @($antesPub).Count
Write-Host "Resenas publicas antes: $antesConteo"

# --- 3. POST publico ---
Write-Host "`n[1/6] POST publico de resena..."
$autor = "Test $([Guid]::NewGuid().ToString('N').Substring(0, 6))"
$post = Post-JsonExpectStatus "$ApiBase/api/products/$($producto.slug)/reviews" @{
    autor  = $autor
    rating = 5
    texto  = 'Esta resena viene del script E2E y valida el flujo completo de moderacion admin.'
} 202
Write-Host "Status: $($post.Status) | Body: $($post.Body)"
if ($post.Status -ne 202) { throw "POST resena deberia devolver 202 (Accepted), devolvio $($post.Status)" }

# --- 4. Publico NO la muestra ---
Write-Host "`n[2/6] Publico no ve pendientes..."
$pubPend = Invoke-RestMethod -Uri "$ApiBase/api/products/$($producto.slug)/reviews" -Method Get
$pubPendConteo = @($pubPend).Count
$encontradaPend = @($pubPend) | Where-Object { $_.autor -eq $autor }
if ($pubPendConteo -ne $antesConteo) { Write-Host "Nota: conteo publico cambio de $antesConteo a $pubPendConteo (otros aprobados posiblemente)" }
if ($encontradaPend) { throw "La resena aparece en el listado publico ANTES de aprobar. Falla la moderacion." }
Write-Host "OK: la resena esta oculta hasta aprobar."

# --- 5. Login admin ---
Write-Host "`n[3/6] Login admin..."
$sesion = $null
$login = Invoke-WebRequest -Uri "$ApiBase/api/auth/login" -Method Post `
    -Body ([System.Text.Encoding]::UTF8.GetBytes((@{ nombreUsuario = $AdminUser; password = $AdminPass } | ConvertTo-Json -Compress))) `
    -ContentType 'application/json; charset=utf-8' -SessionVariable sesion -UseBasicParsing
if ($login.StatusCode -ne 200) { throw "Login fallido: $($login.StatusCode)" }
Write-Host "OK: cookie nogal_auth grabada en la sesion."

# --- 6. Admin ve pendiente ---
Write-Host "`n[4/6] Admin lista pendientes..."
$admin = Invoke-RestMethod -Uri "$ApiBase/api/admin/reviews?estado=pendiente&tamano=100" -Method Get -WebSession $sesion
$nuestra = $admin.items | Where-Object { $_.autor -eq $autor } | Select-Object -First 1
if (-not $nuestra) { throw "El admin no ve la resena en pendientes. Total pendientes: $($admin.total)" }
Write-Host "OK: admin ve resena id=$($nuestra.id) producto=$($nuestra.productoNombre)"

# --- 7. Aprobar ---
Write-Host "`n[5/6] Aprobar..."
$aprobada = Invoke-RestMethod -Uri "$ApiBase/api/admin/reviews/$($nuestra.id)/aprobar" -Method Post -WebSession $sesion
if (-not $aprobada.aprobado) { throw "Despues de aprobar, aprobado=false. Response: $($aprobada | ConvertTo-Json -Compress)" }
Write-Host "OK: aprobado=$($aprobada.aprobado)"

# --- 8. Publico ahora la muestra ---
Write-Host "`n[6/6] Publico ahora la ve..."
$pubDespues = Invoke-RestMethod -Uri "$ApiBase/api/products/$($producto.slug)/reviews" -Method Get
$encontradaOk = @($pubDespues) | Where-Object { $_.autor -eq $autor }
if (-not $encontradaOk) { throw "Despues de aprobar, la resena sigue oculta en publico." }
Write-Host "OK: aparece en publico con rating $($encontradaOk.rating)"

# --- 9. Cleanup ---
Invoke-WebRequest -Uri "$ApiBase/api/admin/reviews/$($nuestra.id)" -Method Delete -WebSession $sesion -UseBasicParsing | Out-Null
Write-Host "`nCleanup: resena eliminada."

Write-Host "`nOK: 6/6 casos pasan (POST 202, oculta pendiente, admin listar, aprobar, publica visible, eliminar)."
