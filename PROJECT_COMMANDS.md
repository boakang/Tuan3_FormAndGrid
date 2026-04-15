# Project Commands

## Stop local service

```powershell
Import-Module WebAdministration
Stop-WebSite -Name "HQSoftDemo"
Stop-WebAppPool -Name "HQSoftDemoPool"
Get-Process w3wp,iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
```

## Restore and build

```powershell
cd "d:\ky2_2526\thực tập\hqsoft\Tuan3\FormAndGrid"
dotnet restore .\HQSoft.sln
dotnet build .\HQSoft.sln -c Debug
```

## Run local IIS

```powershell
Import-Module WebAdministration
Start-WebAppPool -Name "HQSoftDemoPool"
Start-WebSite -Name "HQSoftDemo"
```

## Open app

```text
http://localhost:8090/FS10901
```
