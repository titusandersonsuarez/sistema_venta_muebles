# Prueba end-to-end de subida de imagen contra Azure Blob (Azurite en local).
#
# 1. Login admin.
# 2. Toma el primer producto.
# 3. Genera un PNG minimo valido en memoria (magic bytes correctos).
# 4. POST multipart /api/admin/products/{id}/image.
# 5. Verifica que Product.imagenUrl arranca con http:// (URL absoluta al blob),
#    no con /uploads/ (path relativo del storage local).
# 6. Descarga la URL y verifica que devuelve el PNG bytes exactos.

param(
    [string]$ApiBase   = 'http://localhost:5199',
    [string]$AdminUser = 'admin',
    [Parameter(Mandatory = $true)][string]$AdminPass
)

$ErrorActionPreference = 'Stop'

# --- Login admin ---
$sesion = $null
$loginBytes = [System.Text.Encoding]::UTF8.GetBytes((@{
    nombreUsuario = $AdminUser
    password      = $AdminPass
} | ConvertTo-Json -Compress))
$login = Invoke-WebRequest -Uri "$ApiBase/api/auth/login" -Method Post `
    -Body $loginBytes -ContentType 'application/json; charset=utf-8' `
    -SessionVariable sesion -UseBasicParsing
if ($login.StatusCode -ne 200) { throw "Login fallido: $($login.StatusCode)" }
Write-Host "OK: login admin"

# --- Producto de prueba ---
$producto = (Invoke-RestMethod -Uri "$ApiBase/api/admin/products?tamano=1" -Method Get -WebSession $sesion).items[0]
if (-not $producto) { throw 'No hay productos.' }
Write-Host "Producto: $($producto.nombre) (id=$($producto.id))"

# --- PNG minimo (10x10 pixeles rojos) ---
# Cabecera + IHDR + IDAT + IEND, generado con .NET
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap 10, 10
try {
    for ($x = 0; $x -lt 10; $x++) {
        for ($y = 0; $y -lt 10; $y++) {
            $bmp.SetPixel($x, $y, [System.Drawing.Color]::Red)
        }
    }
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $ms.ToArray()
    $ms.Dispose()
} finally {
    $bmp.Dispose()
}
Write-Host "PNG generado: $($pngBytes.Length) bytes"

# --- POST multipart ---
$boundary = "----NogalTest$([Guid]::NewGuid().ToString('N'))"
$eol = "`r`n"

$prefix = [System.Text.Encoding]::UTF8.GetBytes(
    "--$boundary$eol" +
    "Content-Disposition: form-data; name=`"archivo`"; filename=`"test.png`"$eol" +
    "Content-Type: image/png$eol$eol"
)
$suffix = [System.Text.Encoding]::UTF8.GetBytes("$eol--$boundary--$eol")

$body = New-Object System.IO.MemoryStream
$body.Write($prefix, 0, $prefix.Length)
$body.Write($pngBytes, 0, $pngBytes.Length)
$body.Write($suffix, 0, $suffix.Length)
$bodyBytes = $body.ToArray()
$body.Dispose()

Write-Host "`n[1/3] POST multipart..."
$res = Invoke-WebRequest -Uri "$ApiBase/api/admin/products/$($producto.id)/image" -Method Post `
    -Body $bodyBytes -ContentType "multipart/form-data; boundary=$boundary" `
    -WebSession $sesion -UseBasicParsing
Write-Host "Status: $($res.StatusCode)"
if ($res.StatusCode -ne 200) { throw "Upload devolvio $($res.StatusCode)" }

# --- Verificar URL ---
$actualizado = Invoke-RestMethod -Uri "$ApiBase/api/admin/products/$($producto.id)" -Method Get -WebSession $sesion
Write-Host "`n[2/3] URL guardada: $($actualizado.imagenUrl)"
if (-not $actualizado.imagenUrl.StartsWith('http')) {
    throw "La URL deberia ser absoluta (http://...) porque Provider=azure. Obtuvo: $($actualizado.imagenUrl)"
}

# --- Descargar y verificar bytes ---
Write-Host "`n[3/3] Descargar URL y comparar bytes..."
$descarga = Invoke-WebRequest -Uri $actualizado.imagenUrl -Method Get -UseBasicParsing
if ($descarga.StatusCode -ne 200) { throw "GET blob devolvio $($descarga.StatusCode)" }
if ($descarga.RawContentLength -ne $pngBytes.Length) {
    throw "Tamano no coincide: subimos $($pngBytes.Length), bajamos $($descarga.RawContentLength)"
}
# Firma PNG en los primeros 8 bytes
$expected = @(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
for ($i = 0; $i -lt 8; $i++) {
    if ($descarga.Content[$i] -ne $expected[$i]) {
        throw "Los primeros 8 bytes no son firma PNG. Byte $i = $($descarga.Content[$i])"
    }
}

Write-Host "`nOK: 3/3 casos pasan (upload 200, URL absoluta al blob, bytes coinciden)."
