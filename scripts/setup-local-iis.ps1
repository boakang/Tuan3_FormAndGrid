param(
    [string]$SiteName = "HQSoftDemo",
    [string]$AppPoolName = "HQSoftDemoPool",
    [int]$Port = 8090,
    [string]$ProjectPath = ""
)

$ErrorActionPreference = "Stop"

if (-not $ProjectPath) {
    $ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

Write-Host "ProjectPath: $ProjectPath"

$features = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-ManagementConsole",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter",
    "IIS-ASPNET45",
    "IIS-NetFxExtensibility45",
    "IIS-HttpErrors",
    "IIS-StaticContent",
    "IIS-DefaultDocument",
    "IIS-RequestFiltering"
)

foreach ($f in $features) {
    Enable-WindowsOptionalFeature -Online -FeatureName $f -All -NoRestart | Out-Null
}

Import-Module WebAdministration

if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-WebAppPool -Name $AppPoolName | Out-Null
}

Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value "v4.0"
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedPipelineMode -Value "Integrated"

if (Test-Path "IIS:\Sites\$SiteName") {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $ProjectPath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName

    Remove-WebBinding -Name $SiteName -Protocol http -Port $Port -ErrorAction SilentlyContinue
    New-WebBinding -Name $SiteName -Protocol http -Port $Port -IPAddress "*" | Out-Null
}
else {
    New-Website -Name $SiteName -PhysicalPath $ProjectPath -ApplicationPool $AppPoolName -Port $Port | Out-Null
}

icacls $ProjectPath /grant "IIS_IUSRS:(OI)(CI)(RX)" /T | Out-Null

Push-Location $ProjectPath

# Restore packages first to make sure required runtime assemblies exist in NuGet cache.
dotnet restore .\HQSoft.sln | Out-Null

# Build to isolated output to avoid bin lock issues.
$out = "bin_deploy_" + (Get-Date -Format "HHmmssfff") + "\\"
dotnet build .\HQSoft.sln -c Debug /p:OutDir=$out | Out-Null

# Deploy main app DLL to runtime bin.
Copy-Item (".\\" + $out + "HQSoft.dll") ".\\bin\\HQSoft.dll" -Force
if (Test-Path (".\\" + $out + "HQSoft.pdb")) {
    Copy-Item (".\\" + $out + "HQSoft.pdb") ".\\bin\\HQSoft.pdb" -Force
}

# Ensure MVC runtime dependencies are in bin for Local IIS.
$pkg = Join-Path $env:USERPROFILE ".nuget\\packages"
$runtimeDlls = @(
    "microsoft.aspnet.razor\\3.2.9\\lib\\net45\\System.Web.Razor.dll",
    "microsoft.aspnet.webpages\\3.2.9\\lib\\net45\\System.Web.Helpers.dll",
    "microsoft.aspnet.webpages\\3.2.9\\lib\\net45\\System.Web.WebPages.dll",
    "microsoft.aspnet.webpages\\3.2.9\\lib\\net45\\System.Web.WebPages.Deployment.dll",
    "microsoft.aspnet.webpages\\3.2.9\\lib\\net45\\System.Web.WebPages.Razor.dll",
    "microsoft.web.infrastructure\\1.0.0\\lib\\net40\\Microsoft.Web.Infrastructure.dll"
)

foreach ($rel in $runtimeDlls) {
    $src = Join-Path $pkg $rel
    if (Test-Path $src) {
        Copy-Item $src ".\\bin" -Force
    }
}

Pop-Location

Restart-WebAppPool -Name $AppPoolName
Start-WebSite -Name $SiteName

Write-Host "DONE"
Write-Host "Demo URL: http://localhost:$Port/FS10901"
