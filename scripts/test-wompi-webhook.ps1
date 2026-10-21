# Prueba end-to-end del webhook Wompi.
#
# 1. Crea un pedido de prueba contra POST /api/orders.
# 2. Arma un payload firmado de "transaction.updated" con el mismo
#    EventsSecret que corre el server (env var Wompi__EventsSecret).
# 3. Dispara el webhook y verifica que el pedido pasó a Pago confirmado.
# 4. Reenvía el mismo evento y verifica que el server responde "Evento
#    duplicado, ignorado" (idempotencia).
# 5. Manda un evento con firma alterada y verifica 401.

param(
    [string]$ApiBase = 'http://localhost:5199',
    [string]$EventsSecret = 'test_events_secret'
)

$ErrorActionPreference = 'Stop'

function Post-Json {
    param([string]$Uri, [object]$Body)
    $json = $Body | ConvertTo-Json -Depth 6 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    return Invoke-RestMethod -Uri $Uri -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8'
}

function Post-Raw {
    param([string]$Uri, [string]$Body)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Body)
    try {
        $response = Invoke-WebRequest -Uri $Uri -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8' -UseBasicParsing
        return [pscustomobject]@{ Status = [int]$response.StatusCode; Body = $response.Content }
    } catch {
        $status = [int]$_.Exception.Response.StatusCode
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $body = $reader.ReadToEnd()
        return [pscustomobject]@{ Status = $status; Body = $body }
    }
}

function Sha256Hex {
    param([string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Text))
        return -join ($bytes | ForEach-Object { $_.ToString('x2') })
    } finally { $sha.Dispose() }
}

# --- 1. Crear pedido ---
$producto = (Invoke-RestMethod -Uri "$ApiBase/api/products?tamano=1" -Method Get).items[0]
if (-not $producto) { throw 'No hay productos en la BD; corre el seeder primero.' }
$variantId = $null
if ($producto.variantes -and $producto.variantes.Count -gt 0) {
    $variantId = $producto.variantes[0].id
}

$pedidoRequest = @{
    cliente  = 'Titus Webhook Test'
    ciudad   = 'Bogota'
    contacto = '300 111 2222'
    items    = @(@{ productId = $producto.id; productVariantId = $variantId; cantidad = 1 })
}
$pedido = Post-Json "$ApiBase/api/orders" $pedidoRequest
Write-Host "Pedido creado: $($pedido.codigo) (total $($pedido.total))"

# --- 2. Firmar y disparar webhook ---
$trxId = 'TEST-' + [Guid]::NewGuid().ToString('N').Substring(0, 12)
$amountCents = [long]([math]::Round($pedido.total * 100))
$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

$concat = "$trxId" + 'APPROVED' + "$amountCents" + "$timestamp" + $EventsSecret
$checksum = Sha256Hex $concat

$webhookBody = @{
    event = 'transaction.updated'
    data  = @{
        transaction = @{
            id              = $trxId
            reference       = $pedido.codigo
            amount_in_cents = $amountCents
            currency        = 'COP'
            status          = 'APPROVED'
        }
    }
    timestamp = $timestamp
    signature = @{
        properties = @('transaction.id', 'transaction.status', 'transaction.amount_in_cents')
        checksum   = $checksum
    }
    environment = 'test'
    sent_at     = (Get-Date).ToUniversalTime().ToString('o')
}
$bodyJson = $webhookBody | ConvertTo-Json -Depth 6 -Compress

Write-Host "`n[1/3] Webhook con firma valida..."
$res1 = Post-Raw "$ApiBase/api/webhooks/wompi" $bodyJson
Write-Host "Status: $($res1.Status) | Body: $($res1.Body)"
if ($res1.Status -ne 200) { throw "Primer POST deberia haber devuelto 200, devolvio $($res1.Status)" }

# --- 3. Verificar estado del pedido ---
$actualizado = Invoke-RestMethod -Uri "$ApiBase/api/orders/$($pedido.codigo)" -Method Get
Write-Host "Estado del pedido: $($actualizado.estado) | pago proveedor: $($actualizado.pagoProveedor) | txnId: $($actualizado.pagoTransaccionId)"
if ($actualizado.estado -ne 'Pago confirmado') { throw "El pedido deberia estar en 'Pago confirmado', esta en '$($actualizado.estado)'" }

# --- 4. Reenviar mismo evento (idempotencia) ---
Write-Host "`n[2/3] Webhook duplicado..."
$res2 = Post-Raw "$ApiBase/api/webhooks/wompi" $bodyJson
Write-Host "Status: $($res2.Status) | Body: $($res2.Body)"
if ($res2.Status -ne 200) { throw "Duplicado deberia haber devuelto 200, devolvio $($res2.Status)" }
if ($res2.Body -notmatch 'duplicado') { throw "El body del duplicado deberia mencionar 'duplicado', fue: $($res2.Body)" }

# --- 5. Firma invalida ---
Write-Host "`n[3/3] Webhook con firma alterada..."
$webhookBadSig = $webhookBody.Clone()
$webhookBadSig.signature = @{
    properties = $webhookBody.signature.properties
    checksum   = ('0' * 64)
}
# Cambio timestamp para que EventoId sea distinto y no colisione con el anterior
$webhookBadSig.timestamp = $timestamp + 1
$badJson = $webhookBadSig | ConvertTo-Json -Depth 6 -Compress
$res3 = Post-Raw "$ApiBase/api/webhooks/wompi" $badJson
Write-Host "Status: $($res3.Status) | Body: $($res3.Body)"
if ($res3.Status -ne 401) { throw "Firma invalida deberia haber devuelto 401, devolvio $($res3.Status)" }

Write-Host "`nOK: 3/3 casos pasan (firma valida aplica, duplicado ignorado, firma invalida rechazada)."
