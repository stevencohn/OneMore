<#
.SYNOPSIS
Captures how OneNote renders text colors on a dark page, producing DarkColorTable.json
for MoreColorPicker.

.DESCRIPTION
Creates temporary calibration pages in the current OneNote section. Each page holds rows
of solid blocks, one block per sample color, bracketed by a left and right marker block
(pure black text, which OneNote renders bright on a dark page, so the markers are easy to
find). The script navigates to each page, screenshots the OneNote window, locates the
markers, samples the rendered color at each block, and writes the light/dark pairs as JSON.
Pages are deleted afterwards unless -Keep is specified.

BEFORE RUNNING
 - Use Windows PowerShell 5.1 (powershell.exe), not pwsh.
 - OneNote must be open on a section you can write to.
 - Put OneNote in dark mode with a dark canvas (Office theme Black, canvas not switched
   to light) AND a dark page background. Use the same page color you care about; results
   can differ by page color, e.g. automatic dark vs. a custom color.
 - Make the OneNote window large (at least ~1000px tall) and don't touch the mouse or
   keyboard while it runs (about 1 minute).

.EXAMPLE
powershell.exe -ExecutionPolicy Bypass -File .\Capture-DarkColors.ps1
#>

[CmdletBinding()]
param(
    [string]$OutFile,
    [string]$Section,
    [int]$RowsPerPage = 10,
    [int]$SettleMilliseconds = 2500,
    [switch]$Keep
)

$ErrorActionPreference = 'Stop'

if (-not $OutFile) {
    $root = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
    $OutFile = Join-Path $root 'DarkColorTable.json'
}

if ($PSVersionTable.PSEdition -eq 'Core') {
    throw 'Run this script with Windows PowerShell 5.1 (powershell.exe).'
}

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$source = @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class DarkCalib
{
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    public class Comp
    {
        public int X0, Y0, X1, Y1;
        public int W { get { return X1 - X0 + 1; } }
        public int H { get { return Y1 - Y0 + 1; } }
        public int CX { get { return (X0 + X1) / 2; } }
        public int CY { get { return (Y0 + Y1) / 2; } }
    }

    // sample definitions ------------------------------------------------------------------

    public static readonly double[] Levels =
        { 0.05, 0.15, 0.25, 0.35, 0.45, 0.55, 0.65, 0.75, 0.85, 0.95 };

    public static readonly double[] Saturations = { 0.35, 0.70, 1.00 };

    // Returns rows of hex colors; the first row is gray, then (hue, saturation) rows
    public static List<string[]> Rows()
    {
        var rows = new List<string[]>();

        var grays = new string[Levels.Length];
        for (int i = 0; i < Levels.Length; i++) { grays[i] = Hsl(0, 0, Levels[i]); }
        rows.Add(grays);

        for (int hue = 0; hue < 360; hue += 15)
        {
            foreach (double s in Saturations)
            {
                var row = new string[Levels.Length];
                for (int i = 0; i < Levels.Length; i++) { row[i] = Hsl(hue, s, Levels[i]); }
                rows.Add(row);
            }
        }

        return rows;
    }

    public static string Hsl(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = l - c / 2;
        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }
        return String.Format("{0:X2}{1:X2}{2:X2}",
            (int)Math.Round((r + m) * 255), (int)Math.Round((g + m) * 255), (int)Math.Round((b + m) * 255));
    }

    // image analysis ----------------------------------------------------------------------

    private static byte[] Read(Bitmap bmp, out int stride)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        stride = data.Stride;
        var bytes = new byte[stride * bmp.Height];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
        bmp.UnlockBits(data);
        return bytes;
    }

    // Finds connected blobs of pixels that differ strongly from the page background
    public static List<Comp> FindMarkers(Bitmap bmp, int bgR, int bgG, int bgB, int threshold)
    {
        int stride;
        var px = Read(bmp, out stride);
        int w = bmp.Width, h = bmp.Height;
        var mask = new bool[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int o = (y * stride) + (x * 4);
                int d = Math.Abs(px[o + 2] - bgR) + Math.Abs(px[o + 1] - bgG) + Math.Abs(px[o] - bgB);
                mask[(y * w) + x] = d > threshold;
            }
        }

        var comps = new List<Comp>();
        var stack = new Stack<int>();

        for (int i = 0; i < mask.Length; i++)
        {
            if (!mask[i]) { continue; }

            var c = new Comp { X0 = int.MaxValue, Y0 = int.MaxValue, X1 = -1, Y1 = -1 };
            stack.Push(i);
            mask[i] = false;

            while (stack.Count > 0)
            {
                int p = stack.Pop();
                int px0 = p % w, py0 = p / w;
                if (px0 < c.X0) c.X0 = px0;
                if (px0 > c.X1) c.X1 = px0;
                if (py0 < c.Y0) c.Y0 = py0;
                if (py0 > c.Y1) c.Y1 = py0;

                if (px0 > 0 && mask[p - 1]) { mask[p - 1] = false; stack.Push(p - 1); }
                if (px0 < w - 1 && mask[p + 1]) { mask[p + 1] = false; stack.Push(p + 1); }
                if (py0 > 0 && mask[p - w]) { mask[p - w] = false; stack.Push(p - w); }
                if (py0 < h - 1 && mask[p + w]) { mask[p + w] = false; stack.Push(p + w); }
            }

            comps.Add(c);
        }

        return comps;
    }

    // Median color of a small square around the point
    public static string Sample(Bitmap bmp, int cx, int cy, int radius)
    {
        var rs = new List<int>(); var gs = new List<int>(); var bs = new List<int>();
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height) { continue; }
                Color c = bmp.GetPixel(x, y);
                rs.Add(c.R); gs.Add(c.G); bs.Add(c.B);
            }
        }

        rs.Sort(); gs.Sort(); bs.Sort();
        int m = rs.Count / 2;
        return String.Format("{0:X2}{1:X2}{2:X2}", rs[m], gs[m], bs[m]);
    }
}
'@

Add-Type -TypeDefinition $source -ReferencedAssemblies System.Drawing

[void][DarkCalib]::SetProcessDPIAware()

$block = [string][char]0x2588
$ns = 'http://schemas.microsoft.com/office/onenote/2013/onenote'

$one = New-Object -ComObject OneNote.Application
$sectionId = $null
try { $sectionId = $one.Windows.CurrentWindow.CurrentSectionId } catch { }

if (-not $sectionId) {
    # fall back to finding a section by name in the hierarchy
    $hierarchy = ''
    $one.GetHierarchy('', 3, [ref]$hierarchy)
    $doc = New-Object System.Xml.XmlDocument
    $doc.LoadXml($hierarchy)
    $nsm = New-Object System.Xml.XmlNamespaceManager $doc.NameTable
    $nsm.AddNamespace('one', 'http://schemas.microsoft.com/office/onenote/2013/onenote')
    $sections = @($doc.SelectNodes('//one:Section', $nsm) | Where-Object { -not $_.isRecycleBin })

    if ($Section) {
        $match = $sections | Where-Object { $_.name -eq $Section } | Select-Object -First 1
        if ($match) { $sectionId = $match.ID }
    }

    if (-not $sectionId) {
        $names = ($sections | ForEach-Object { $_.name }) -join ', '
        throw "Could not determine the current section. Re-run with -Section '<name>' using one of: $names"
    }
}

$onenoteProcess = Get-Process ONENOTE -ErrorAction Stop |
    Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
$hwnd = $onenoteProcess.MainWindowHandle

function Get-Span([string]$hex, [string]$text) {
    "<span style='font-family:Consolas;font-size:28pt;color:#$hex'>$text</span>"
}

function New-CalibrationPage([string]$title, $rows) {
    $pageId = $null
    $one.CreateNewPage($sectionId, [ref]$pageId, 0)

    $lines = foreach ($row in $rows) {
        $cells = New-Object System.Collections.Generic.List[string]
        $cells.Add((Get-Span '000000' "$block "))
        foreach ($hex in $row) { $cells.Add((Get-Span $hex "$block ")) }
        $cells.Add((Get-Span '000000' $block))
        "<one:OE><one:T><![CDATA[$($cells -join '')]]></one:T></one:OE>"
    }

    # no title element so the only bright content on the canvas is our markers
    $xml = @"
<one:Page xmlns:one="$ns" ID="$pageId">
  <one:Outline>
    <one:Position x="36" y="160" z="0"/>
    <one:OEChildren>$($lines -join '')</one:OEChildren>
  </one:Outline>
</one:Page>
"@
    $one.UpdatePageContent($xml)
    return $pageId
}

$lastBg = $null
$allRows = [DarkCalib]::Rows()
$perRow = [DarkCalib]::Levels.Length
$pairs = New-Object System.Collections.Generic.List[object]
$createdPages = New-Object System.Collections.Generic.List[string]

try {
    for ($start = 0; $start -lt $allRows.Count; $start += $RowsPerPage) {
        $count = [Math]::Min($RowsPerPage, $allRows.Count - $start)
        $rows = $allRows.GetRange($start, $count)
        $pageNumber = [int]($start / $RowsPerPage) + 1
        Write-Host "Page $pageNumber ($count rows)..."

        $pageId = New-CalibrationPage "OneMore dark calibration $pageNumber" $rows
        $createdPages.Add($pageId)

        $one.NavigateTo($pageId)
        [void][DarkCalib]::ShowWindow($hwnd, 9)
        [void][DarkCalib]::SetForegroundWindow($hwnd)
        Start-Sleep -Milliseconds $SettleMilliseconds

        $rect = New-Object DarkCalib+RECT
        [void][DarkCalib]::GetWindowRect($hwnd, [ref]$rect)
        $w = $rect.Right - $rect.Left
        $h = $rect.Bottom - $rect.Top

        $bitmap = New-Object System.Drawing.Bitmap $w, $h
        $g = [System.Drawing.Graphics]::FromImage($bitmap)
        $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bitmap.Size)
        $g.Dispose()

        try {
            # background: a point on the canvas well to the right of the outline
            $bg = $bitmap.GetPixel([int]($w * 0.85), [int]($h * 0.6))
            $lastBg = '{0:X2}{1:X2}{2:X2}' -f $bg.R, $bg.G, $bg.B
            $luma = (0.2126 * $bg.R + 0.7152 * $bg.G + 0.0722 * $bg.B) / 255
            if ($luma -gt 0.35) {
                throw "The page background looks light (luma $([Math]::Round($luma,2))); switch to a dark page."
            }

            $blobs = [DarkCalib]::FindMarkers($bitmap, $bg.R, $bg.G, $bg.B, 150)

            # The markers are the brightest blobs in the left-most and right-most columns of
            # the grid. Rows touch vertically so a marker column may merge into one tall bar;
            # use its overall extent and divide it evenly into rows.
            $cands = @($blobs | Where-Object {
                $_.W -ge 10 -and $_.H -ge 20 -and
                $_.X0 -ge ($w * 0.17) -and $_.X1 -le ($w * 0.65) -and $_.Y0 -ge ($h * 0.30)
            })
            if ($cands.Count -eq 0) { throw 'No marker blocks found.' }

            $minX = ($cands | Measure-Object X0 -Minimum).Minimum
            $maxX = ($cands | Measure-Object X1 -Maximum).Maximum
            $leftBlobs = @($cands | Where-Object { $_.X0 -le ($minX + 4) })
            $rightBlobs = @($cands | Where-Object { $_.X1 -ge ($maxX - 4) })

            $top = ($leftBlobs | Measure-Object Y0 -Minimum).Minimum
            $bottom = ($leftBlobs | Measure-Object Y1 -Maximum).Maximum
            $rowHeight = ($bottom - $top + 1) / $count
            $leftX = ($minX + (($leftBlobs | Measure-Object X1 -Maximum).Maximum)) / 2
            $rightX = ((($rightBlobs | Measure-Object X0 -Minimum).Minimum) + $maxX) / 2

            $topRight = ($rightBlobs | Measure-Object Y0 -Minimum).Minimum
            $bottomRight = ($rightBlobs | Measure-Object Y1 -Maximum).Maximum
            if ($rowHeight -lt 20 -or $rowHeight -gt 120 -or
                [Math]::Abs($top - $topRight) -gt 4 -or [Math]::Abs($bottom - $bottomRight) -gt 4) {
                throw "Unexpected marker geometry: left y=$top..$bottom, right y=$topRight..$bottomRight, row height $rowHeight"
            }

            $pitch = ($rightX - $leftX) / ($perRow + 1.0)
            for ($r = 0; $r -lt $count; $r++) {
                $y = [int][Math]::Round($top + ($rowHeight * ($r + 0.5)))
                for ($i = 0; $i -lt $perRow; $i++) {
                    $x = [int][Math]::Round($leftX + ($pitch * ($i + 1)))
                    $dark = [DarkCalib]::Sample($bitmap, $x, $y, 3)
                    $pairs.Add([pscustomobject]@{ l = $rows[$r][$i]; d = $dark })
                }
            }
        }
        catch {
            $shot = Join-Path $env:TEMP "dark-calibration-page$pageNumber.png"
            $bitmap.Save($shot)
            Write-Warning "Saved screenshot for diagnosis: $shot"
            throw
        }
        finally {
            $bitmap.Dispose()
        }
    }

    $result = [pscustomobject]@{
        captured = (Get-Date).ToString('s')
        pageBackground = $lastBg
        samples = $pairs
    }

    $result | ConvertTo-Json -Depth 4 -Compress | Set-Content -Path $OutFile -Encoding UTF8
    Write-Host "Wrote $($pairs.Count) samples to $OutFile"
}
finally {
    if (-not $Keep) {
        foreach ($id in $createdPages) {
            try { $one.DeleteHierarchy($id) } catch { Write-Warning "Could not delete page $id" }
        }
    }
}
