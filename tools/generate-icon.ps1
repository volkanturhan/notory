# Generates notory's application icon: an amber->orange rounded square with a
# white note card and a few grey text lines — a quick note / scratchpad.
#
# Frames are written as uncompressed 32-bit BMP (DIB) entries via GDI+ itself,
# because System.Drawing.Icon / the WinForms NotifyIcon load BMP frames
# reliably, whereas PNG-compressed frames can fail to decode.
#
# Run from anywhere; it writes ../notory/Assets/notory.ico.
Add-Type -AssemblyName System.Drawing

function New-RoundedRect([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconBitmap([int]$S) {
    $bmp = New-Object System.Drawing.Bitmap($S, $S, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # Background rounded square, filled with a diagonal amber -> orange gradient.
    $m = [single]($S * 0.06)
    $side = [single]($S - 2 * $m)
    $bg = New-RoundedRect $m $m $side $side ([single]($S * 0.22))
    $amber = [System.Drawing.Color]::FromArgb(255, 251, 191, 36)    # #FBBF24
    $orange = [System.Drawing.Color]::FromArgb(255, 245, 158, 11)   # #F59E0B
    $rect = New-Object System.Drawing.RectangleF(0, 0, $S, $S)
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $amber, $orange, 45.0)
    $g.FillPath($grad, $bg)

    # White note card.
    $cardW = [single]($S * 0.46)
    $cardH = [single]($S * 0.52)
    $cx = [single](($S - $cardW) / 2)
    $cy = [single](($S - $cardH) / 2)
    $card = New-RoundedRect $cx $cy $cardW $cardH ([single]($S * 0.05))
    $g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), $card)

    # Grey text lines on the card.
    $line = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 209, 213, 219))  # #D1D5DB
    $lh = [single]($S * 0.045)
    $lx = [single]($cx + $cardW * 0.16)
    $lw = [single]($cardW * 0.68)
    for ($i = 0; $i -lt 3; $i++) {
        $ly = [single]($cy + $cardH * 0.26 + $i * $cardH * 0.22)
        $g.FillPath($line, (New-RoundedRect $lx $ly $lw $lh ([single]($lh / 2))))
    }

    $g.Dispose()
    return $bmp
}

# Returns a complete single-frame .ico (as bytes) for one size, produced by
# GDI+ itself via GetHicon -> Icon.Save, so the pixel data and its directory
# entry are guaranteed mutually consistent; we only repackage them below.
function Get-SingleFrameIco([System.Drawing.Bitmap]$bmp) {
    $hicon = $bmp.GetHicon()
    $icon = [System.Drawing.Icon]::FromHandle($hicon)
    $ms = New-Object System.IO.MemoryStream
    $icon.Save($ms)
    $icon.Dispose()
    $bytes = $ms.ToArray()
    $ms.Dispose()
    return , $bytes
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)

$singles = New-Object 'System.Collections.Generic.List[byte[]]'
foreach ($s in $sizes) {
    $bmp = New-IconBitmap $s
    $singles.Add((Get-SingleFrameIco $bmp))
    $bmp.Dispose()
}

$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter($out)
$w.Write([uint16]0)
$w.Write([uint16]1)
$w.Write([uint16]$sizes.Count)

$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $single = $singles[$i]
    $blobLength = $single.Length - 22
    $entry = New-Object byte[] 16
    [System.Array]::Copy($single, 6, $entry, 0, 16)
    [System.BitConverter]::GetBytes([uint32]$blobLength).CopyTo($entry, 8)
    [System.BitConverter]::GetBytes([uint32]$offset).CopyTo($entry, 12)
    $w.Write($entry, 0, 16)
    $offset += $blobLength
}
foreach ($single in $singles) {
    $w.Write($single, 22, $single.Length - 22)
}
$w.Flush()

$target = Join-Path $PSScriptRoot '..\notory\Assets\notory.ico'
[System.IO.File]::WriteAllBytes($target, $out.ToArray())
$w.Dispose()
Write-Output "Wrote $((Resolve-Path $target).Path) ($((Get-Item $target).Length) bytes)"
