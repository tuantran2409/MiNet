# 📝 QUY TẮC PHÁT TRIỂN & BÀN GIAO DỰ ÁN MiNet

Để dự án luôn giữ được tiêu chuẩn **"Out-of-the-box" (Tải về là chạy được ngay)**, tất cả các nhà phát triển tham gia vui lòng tuân thủ các nguyên tắc sau đây:

## 1. Quản lý Cơ sở dữ liệu (Database & Migrations)
Tính năng tự động cập nhật và khởi tạo Database đã được tích hợp sẵn. Để tránh làm hỏng luồng làm việc:
- **TUYỆT ĐỐI KHÔNG XÓA** các file trong thư mục `MiNet.Data/Migrations`. Nếu bạn xóa các file này, hệ thống sẽ không thể đồng bộ Database giữa các máy tính khác nhau.
- **Dùng lệnh Entity Framework Core**: Khi thay đổi Model, hãy dùng lệnh `Add-Migration <Tên_Mới>` để tạo kịch bản cập nhật.
- **Giữ nguyên dòng `MigrateAsync()`**: Không được xóa hoặc vô hiệu hóa dòng `await dbContext.Database.MigrateAsync();` trong file `MiNet/Program.cs`. 
- **Seed Data**: Nếu có tính năng mới yêu cầu dữ liệu mẫu, hãy cập nhật vào `DbInitializer.cs`.

## 2. Cấu hình Môi trường (Environment Settings)
- **Chuỗi kết nối (Connection String)**: Đừng sửa trực tiếp `DefaultConnection` trong `appsettings.json` nếu nó làm ảnh hưởng tới người khác. 
- **Khuyến khích**: Sử dụng file `appsettings.Development.json` để ghi đè các cấu hình riêng trên máy cá nhân của bạn. File này sẽ không gây xung đột khi bạn chia sẻ code.

## 3. Quy trình làm việc với Git
- **Làm việc trên Nhánh (Branching)**: Hãy tạo nhánh mới cho mỗi tính năng (ví dụ: `feature/new-login-ui`).
- **Pull Requests (PR)**: Sau khi hoàn thành, hãy tạo PR trên GitHub. Code sẽ được Review trước khi được gộp vào nhánh chính (`master` hoặc `main`).
- **Commit Message**: Vui lòng viết ghi chú commit rõ ràng (ví dụ: `feat: cập nhật logic đăng bài`, `fix: sửa lỗi hiển thị header`).

## 4. Kiểm tra trước khi Push
- Trước khi đẩy code lên, hãy đảm bảo dự án chạy thành công bằng cách nhấn **F5** (hoặc `dotnet run`). Dự án phải tự động cập nhật được Database và không báo lỗi schema.

---
**Mục tiêu**: Bất kỳ ai thực hiện `git pull` bản code mới nhất về đều phải chạy được ngay lập tức mà không cần bất kỳ bước cấu hình thủ công nào khác.

*Cảm ơn sự hợp tác của bạn!*
