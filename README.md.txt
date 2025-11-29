1. Yêu Cầu Hệ Thống (Prerequisites)
Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt:

Chạy Backend:

.NET SDK 8.0 (hoặc mới hơn).

Công cụ dòng lệnh dotnet-ef (Cài bằng lệnh: dotnet tool install --global dotnet-ef).

Chạy Frontend:

Trình duyệt web hiện đại (Chrome, Edge, Firefox...).

Visual Studio Code (Khuyên dùng).

Extension "Live Server" trên VS Code (Để chạy Frontend mượt mà hơn).
2. Hướng Dẫn Chạy Backend (Server)
Backend chịu trách nhiệm xử lý dữ liệu và kết nối Database (SQLite).

Bước 1: Mở Terminal tại thư mục Backend

Tìm đến thư mục chứa file MiniOrderAPI.csproj.

Ví dụ đường dẫn: .../backend-CK-baitap/

Bước 2: Cài đặt gói thư viện (Restore) Chạy lệnh sau để tải các thư viện cần thiết: dotnet restore 
Bước 3: Khởi tạo Cơ sở dữ liệu (Database) Lệnh này sẽ tạo file MiniOrder.db dựa trên các file Migration đã có:dotnet ef database update
Bước 4: Khởi chạy Server : dotnet run
u khi chạy thành công, bạn sẽ thấy thông báo:

Now listening on: http://localhost:5115

👉 Backend đang chạy tại: http://localhost:5115 👉 Trang tài liệu API (Swagger): http://localhost:5115/swagger

🎨 3. Hướng Dẫn Chạy Frontend (Client)
Frontend là giao diện web để người dùng thao tác.

Cách 1: Chạy bằng Live Server (Khuyên dùng)

Mở thư mục dự án bằng VS Code.

Tìm đến thư mục Front-end.

Chuột phải vào file index.html (hoặc login.html).

Chọn "Open with Live Server".

Cách 2: Mở trực tiếp

Vào thư mục Front-end trong File Explorer.

Click đúp vào file index.html để mở trên trình duyệt.

4. Tài Khoản Dùng Thử

Hệ thống đã tạo sẵn 2 tài khoản mẫu để bạn kiểm tra: tk:admin , mk:123456 . tk:user ,mk:123456
Các Lỗi Thường Gặp & Cách Sửa

1. Lỗi "Failed to fetch" hoặc "Lỗi kết nối Server"

Nguyên nhân: Backend chưa chạy hoặc bị tắt.

Khắc phục: Kiểm tra lại cửa sổ Terminal chạy dotnet run, đảm bảo nó vẫn đang hoạt động.

2. Lỗi đăng nhập nhưng không chuyển trang

Khắc phục: Nhấn F12 trên trình duyệt, vào tab Console xem báo lỗi gì. Thường là do sai đường dẫn API (kiểm tra file script.js, biến API_URL phải là http://localhost:5115/api).

3. Lỗi Database khi chạy lệnh update

Khắc phục: Xóa file MiniOrder.db (nếu có) và thư mục Migrations cũ, sau đó chạy lại:

dotnet ef migrations add InitialCreate

dotnet ef database update
