param(
    [int]$Port = 8090,
    [string]$BranchID = "HQSOFT"
)

$ErrorActionPreference = "Stop"
$base = "http://localhost:$Port"

Write-Host "Checking page..."
$page = Invoke-WebRequest -Uri "$base/FS10901" -UseBasicParsing
Write-Host "FS10901 status: $($page.StatusCode)"

$batch = "INDEMO" + (Get-Date -Format "MMddHHmmss")
$order = (Get-Date).ToString("yyyy-MM-dd") + "T00:00:00"

$header = '{"CpnyID":"' + $BranchID + '","BatchID":"' + $batch + '","OrderDay":"' + $order + '","TotalNumer":1,"TotalVolume":1,"TotalAmount":1}'
$details = '[{"CpnyID":"' + $BranchID + '","BatchID":"' + $batch + '","InventoryID":"00001","Number":1,"Volume":1,"Price":1,"Tax":0,"Amount":1}]'

$saveBody = 'header=' + [uri]::EscapeDataString($header) + '&details=' + [uri]::EscapeDataString($details) + '&deleted=%5B%5D'
$save = Invoke-WebRequest -Uri "$base/FS10901/Save" -Method POST -Body $saveBody -ContentType "application/x-www-form-urlencoded" -UseBasicParsing
Write-Host "Save response: $($save.Content)"

$deleteBody = "branchID=$BranchID&batchID=$batch"
$del = Invoke-WebRequest -Uri "$base/FS10901/DeleteData" -Method POST -Body $deleteBody -ContentType "application/x-www-form-urlencoded" -UseBasicParsing
Write-Host "Delete response: $($del.Content)"

Write-Host "Smoke test completed."
