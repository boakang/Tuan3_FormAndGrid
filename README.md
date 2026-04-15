# FormAndGrid (FS10901 Demo)

Project demo ASP.NET MVC cho màn hình quản lý lô hàng FS10901.

## 1) Mục tiêu

- Cung cấp màn hình demo để thao tác dữ liệu lô hàng theo mô hình Header-Detail.
- Cho phép load, thêm/sửa, lưu và xóa dữ liệu lô.
- Dùng SQL Server thật với Entity Framework 6.

## 2) Công nghệ và môi trường

- ASP.NET MVC 5
- Entity Framework 6
- SQL Server
- JavaScript thuần (không dùng framework frontend)

Yêu cầu chạy local:

- Windows có Local IIS
- .NET Framework 4.8 (theo target project)
- .NET SDK (dùng cho restore/build trong script)

IIS features được script setup bật:

- IIS-WebServerRole
- IIS-WebServer
- IIS-ManagementConsole
- IIS-ISAPIExtensions
- IIS-ISAPIFilter
- IIS-ASPNET45
- IIS-NetFxExtensibility45
- IIS-HttpErrors
- IIS-StaticContent
- IIS-DefaultDocument
- IIS-RequestFiltering

## 3) Cấu trúc thư mục

```text
FormAndGrid/
|-- HQSoft.sln                         // Solution để mở/build toàn bộ project
|-- HQSoft.csproj                      // Cấu hình project, package, target framework
|-- Web.config                         // Connection string và cấu hình web/EF
|-- Global.asax                        // Khai báo entry ASP.NET application
|-- Global.asax.cs                     // Application_Start, đăng ký route
|-- README.md                          // Tài liệu tổng quan dự án
|-- SETUP_GUIDE.md                     // Tài liệu setup chi tiết (nếu dùng)
|-- packages.config                    // Danh sách package kiểu cũ để tương thích
|-- note.txt                           // Ghi chú nghiệp vụ và SQL tham khảo
|-- App_Start/
|   `-- RouteConfig.cs                 // Cấu hình URL route cho MVC
|-- Controllers/
|   `-- FS10901Controller.cs           // API load/save/delete và validate backend
|-- Models/
|   `-- DemoEdmxPlaceholders.cs        // DbContext EF6, entity mapping, gọi stored procedure
|-- Views/
|   `-- FS10901/
|       |-- index.cshtml               // Trang entry load script và render body
|       `-- body.cshtml                // HTML giao diện form + grid
|-- scripts/
|   |-- setup-local-iis.ps1            // Setup Local IIS và deploy DLL vào bin
|   |-- smoke-test-local.ps1           // Test nhanh luồng Save/Delete qua HTTP
|   `-- screen/
|       `-- FS10901.js                 // Logic frontend: load combo, load data, save, delete, tính tổng
|-- Properties/
|   `-- AssemblyInfo.cs                // Metadata assembly .NET Framework
|-- bin/                               // Output runtime/build chính
|-- obj/                               // Output trung gian khi build
|-- bin_test*/                         // Output test tạm
`-- bin_deploy_*/                      // Output deploy tạm từ script
```

## 4) Route và màn hình

- Route chính: `/FS10901/{action}/{id}`
- Default route cũng trỏ về controller FS10901.
- Màn hình demo: `http://localhost:8090/FS10901`

## 5) Luồng dữ liệu thực tế

### 5.1 Header/Detail

- Bảng header: `FS_Batch_Huy`
- Bảng detail: `FS_BatchDetail_Huy`

Load dữ liệu header/detail đang dùng stored procedure:

- `FS10901_pgBatch_Huy`
- `FS10901_pgBatchDetail_Huy`

### 5.2 Danh sách dropdown

- Danh sách BranchID (CpnyID) lấy trực tiếp từ bảng `SYS_Company` (Status = 'AC').
- Danh sách InventoryID lấy trực tiếp từ bảng `IN_Inventory` (Status = 'AC').
- Danh sách BatchID lấy theo branch từ bảng `FS_Batch_Huy`.

## 6) Chức năng màn hình

1. Load dữ liệu theo BranchID + BatchID.
2. Cho phép để trống BatchID khi save để tự sinh mã lô.
3. Thêm/xóa dòng detail.
4. Lưu header + detail vào database.
5. Xóa cả lô (xóa detail trước, sau đó xóa header).

## 7) Validate hiện có

Backend:

- BranchID bắt buộc.
- OrderDay không được nhỏ hơn ngày hiện tại theo giờ Việt Nam (UTC+07:00).
- Không cho phép InventoryID trùng trong cùng lô.
- InventoryID phải tồn tại và có Status = 'AC' trong `IN_Inventory`.

Frontend:

- Chặn save khi thiếu BranchID.
- Chặn save khi không có dòng detail hợp lệ.
- Chặn save khi trùng InventoryID trên grid.
- Tự tính tổng Number/Volume/Amount.

## 8) Công thức tính toán

Amount mỗi dòng:

Amount = (Number x Volume x Price) + (Number x Volume x Price x Tax / 100)

Header:

- TotalNumber = tổng Number các dòng detail
- TotalVolume = tổng Volume các dòng detail
- TotalAmount = tổng Amount các dòng detail

## 9) Cách chạy nhanh

1. Mở PowerShell với quyền Administrator tại thư mục project.
2. Chạy setup Local IIS:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup-local-iis.ps1
```

3. Mở màn hình:

```text
http://localhost:8090/FS10901
```

4. Smoke test (tùy chọn):

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test-local.ps1
```

## 10) API chính của màn hình

GET:

- `/FS10901/GetCpnyList`
- `/FS10901/GetInventoryList`
- `/FS10901/GetBatchList?branchID=...`
- `/FS10901/GetFS_Batch_Huy?branchID=...&batchID=...`
- `/FS10901/GetFS_BatchDetail_Huy?branchID=...&batchID=...`

POST:

- `/FS10901/Save`
- `/FS10901/DeleteData`

## 11) Ghi chú vận hành

- Timestamp khi lưu (`Crtd_DateTime`, `LUpd_DateTime`) đang dùng giờ Việt Nam (UTC+07:00).
- Nếu đổi database/server, cập nhật connection string `Product_eSales_2026Entities` trong `Web.config`.

## 12) Giao diện
![giao diện](https://github.com/boakang/Tuan3_FormAndGrid/blob/main/img/Screenshot%202026-04-15%20154953.png)
