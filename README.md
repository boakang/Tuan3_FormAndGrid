# FormAndGrid (FS10901 Demo)

Project demo ASP.NET MVC (.NET Framework) cho màn hình quản lý lô hàng FS10901.

## 1) Công nghệ chính
- ASP.NET MVC 5
- Entity Framework 6
- SQL Server
- JavaScript thuần cho giao diện demo

Các thành phần đã cài/enable để chạy project local:
- .NET Framework 4.8 (target của project)
- .NET SDK (dùng lệnh `dotnet restore`, `dotnet build`)
- Local IIS (thay cho IIS Express), gồm các feature:
   - IIS-WebServerRole
   - IIS-WebServer
   - IIS-ManagementConsole
   - IIS-ISAPIExtensions
   - IIS-ISAPIFilter
   - IIS-ASPNET45
   - IIS-NetFxExtensibility45
- NuGet packages chính:
   - Microsoft.AspNet.Mvc 5.2.9
   - Microsoft.AspNet.Razor 3.2.9
   - Microsoft.AspNet.WebPages 3.2.9
   - Microsoft.Web.Infrastructure 1.0.0
   - EntityFramework 6.4.4
   - Newtonsoft.Json 13.0.3

## 2) Cấu trúc thư mục

```text
FormAndGrid/
|-- HQSoft.sln                                   // Solution Visual Studio
|-- HQSoft.csproj                                // Cấu hình project, package và danh sách file compile/content
|-- Web.config                                   // Cấu hình app (connection string, EF, ASP.NET)
|-- Global.asax                                  // Entry ASP.NET application
|-- Global.asax.cs                               // Application_Start, đăng ký route
|-- README.md                                    // Tài liệu mô tả project
|-- SETUP_GUIDE.md                               // Hướng dẫn setup/chạy local
|-- .gitignore                                   // Bỏ qua file build/cache khi commit
|-- packages.config                              // File package cũ (để tương thích)
|-- note.txt                                     // Ghi chú nghiệp vụ/database
|-- HQSOFT-HD_FormAndGrid - Copy (1).pdf         // Tài liệu tham khảo
|-- mo ta in_inventory.pdf                       // Tài liệu bảng IN_Inventory
|-- mo ta sys_company.pdf                        // Tài liệu bảng SYS_Company
|
|-- App_Start/
|   `-- RouteConfig.cs                           // Định nghĩa route, default vào FS10901
|
|-- Controllers/
|   `-- FS10901Controller.cs                     // API/logic: Load Header-Detail, Save, Delete
|
|-- Models/
|   `-- DemoEdmxPlaceholders.cs                  // DbContext + entity mapping + gọi stored procedure
|
|-- Views/
|   |-- Web.config                               // Cấu hình Razor cho Views
|   |-- _ViewStart.cshtml                        // Cấu hình layout mặc định
|   `-- FS10901/
|       |-- index.cshtml                         // Trang chính màn hình FS10901
|       `-- body.cshtml                          // Form + grid hiển thị dữ liệu
|
|-- scripts/
|   |-- setup-local-iis.ps1                      // Script setup Local IIS
|   |-- smoke-test-local.ps1                     // Script test nhanh Save/Delete
|   `-- screen/
|       `-- FS10901.js                           // Frontend logic: load/save/delete/tính tổng/validate
|
|-- Properties/
|   `-- AssemblyInfo.cs                          // Metadata assembly .NET Framework
|
|-- bin/                                         // Output build (không phải source)
|-- obj/                                         // File trung gian build (không phải source)
|-- bin_test*/                                   // Output test tạm (không phải source)
`-- bin_deploy_*/                                // Output deploy tạm (không phải source)
```

## 3) Database đang dùng trong flow demo
- Bảng: `FS_Batch_Huy`, `FS_BatchDetail_Huy`.
- Stored procedure: `FS10901_pgBatch_Huy`, `FS10901_pgBatchDetail_Huy`.
- Bảng tham chiếu gián tiếp qua SP: `IN_Inventory`.

## 4) Chạy nhanh
1. Mở PowerShell (Run as Administrator) tại thư mục project.
2. Chạy setup local IIS:
   - `powershell -ExecutionPolicy Bypass -File .\scripts\setup-local-iis.ps1`
3. Mở màn hình:
   - `http://localhost:8090/FS10901`
4. (Tùy chọn) smoke test:
   - `powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test-local.ps1`
