# WatchStore.Web — Ứng dụng quản lý công việc

Ứng dụng web quản lý task cá nhân, có đăng ký/đăng nhập và phân quyền.
Mỗi người dùng chỉ thấy và thao tác được trên công việc của chính mình.

## Công nghệ

- ASP.NET Core 10 MVC
- Entity Framework Core 10 + SQL Server LocalDB
- Cookie Authentication (không dùng ASP.NET Core Identity)
- Bootstrap 5

## Kiến trúc

Phân 4 tầng, mỗi tầng chỉ biết tầng ngay dưới:

Controller → Service → Repository → DbContext → SQL Server

| Tầng | Trách nhiệm |
|---|---|
| Controllers | nhận request, đọc claim, chọn View |
| Services | quy tắc nghiệp vụ, hash mật khẩu, ràng buộc an toàn |
| Repositories | truy vấn EF Core |
| Entities / Data | ánh xạ 2 bảng Users và Tasks |

Dữ liệu ra View luôn đi qua ViewModel, không trả entity trực tiếp.

## Cơ sở dữ liệu

Hai bảng, quan hệ 1-N với cascade delete.

**Users** — Id, Username (unique), PasswordHash, Role, IsActive, CreatedAt
**Tasks** — Id, Title, Description, Status, DueDate, CreatedAt, UserId (FK)

## Chạy dự án

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Chuỗi kết nối nằm ở `appsettings.json`, mặc định dùng LocalDB.

## Tài khoản mặc định

Tài khoản admin được tạo tự động lúc khởi động nếu DB chưa có admin nào:

- Tên đăng nhập: `admin`
- Mật khẩu: `Admin@123`

**Lưu ý bảo mật**: mật khẩu này viết cứng trong `Data/DbSeeder.cs` cho mục
đích demo. Trong môi trường thật phải đọc từ biến môi trường hoặc user
secrets, và bắt đổi mật khẩu ở lần đăng nhập đầu.

## Các biện pháp bảo mật đã áp dụng

| Nguy cơ | Cách xử lý |
|---|---|
| Lộ mật khẩu | Hash PBKDF2 qua `PasswordHasher<User>`, không lưu bản rõ |
| Xem trộm dữ liệu người khác | Mọi truy vấn task đều kèm `UserId` trong câu `WHERE`; `ITaskRepository` cố ý **không có** hàm `GetByIdAsync(taskId)` trơ trọi |
| Leo thang đặc quyền | `RegisterViewModel` không có property `Role`; Service gán cứng `Role = "User"` |
| CSRF | `[ValidateAntiForgeryToken]` trên mọi POST, token do tag helper tự chèn |
| Open redirect | `Url.IsLocalUrl(returnUrl)` trước khi chuyển hướng |
| Dò tên tài khoản | Đăng nhập sai báo chung một câu, không phân biệt sai tên hay sai mật khẩu |
| Truy cập trái phép | Trả `404` thay vì `403` để không tiết lộ id nào tồn tại |
| Mất quyền quản trị | Không cho tự hạ quyền/tự khóa, không cho hạ quyền admin cuối cùng |

Validation chạy hai tầng: `data-val-*` ở trình duyệt để phản hồi nhanh,
`ModelState.IsValid` ở server là lá chắn thật.

## Giới hạn hiện tại

- Chưa có quên mật khẩu, đổi mật khẩu
- Chưa phân trang danh sách
- Admin không xem được task của người khác (cố ý, để giữ một đường
  truy cập task duy nhất)