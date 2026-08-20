# iPhone System Data Cleaner

Một công cụ chuyên nghiệp để dọn dẹp dữ liệu rác, bộ nhớ đệm và các tệp tin tạm thời trên iPhone (dành cho máy đã Jailbreak). Giúp giải phóng không gian lưu trữ "System Data" (Dữ liệu Hệ thống) một cách an toàn và dễ dàng qua kết nối SSH.

## Tính Năng Chính
- **Quét Dọn Nhanh Chóng**: Quét các thư mục rác (Caches, Logs, Temp Files...) chỉ với một cú nhấp chuột.
- **Phân Loại An Toàn**: Mỗi thư mục đều được phân loại theo mức độ an toàn (An toàn, Cẩn thận, Nguy hiểm).
- **Sao Lưu Trước Khi Xóa**: Tính năng backup trước khi xóa để có thể khôi phục lại (Restore) khi cần thiết.
- **Giao Diện Hiện Đại**: Hỗ trợ Dark Mode, Sidebar điều hướng mượt mà, thao tác trực quan.
- **Tính năng chọn nhiều mục**: Bấm Shift + Click để chọn nhiều mục liên tiếp. Sắp xếp thông minh theo dung lượng, mức độ rủi ro, hoặc tên.

## Yêu Cầu Hệ Thống
1. **Máy tính Windows**: (Win 10/11) chạy phần mềm `SysDataNCL.exe`.
2. **iPhone**: Đã Jailbreak (hỗ trợ cả Rootful và Rootless).
3. **OpenSSH**: Điện thoại phải cài đặt sẵn tweak OpenSSH (thường có sẵn khi Jailbreak).

## Hướng Dẫn Sử Dụng
1. Kết nối iPhone và máy tính vào cùng một mạng Wi-Fi.
2. Mở ứng dụng **SysDataNCL.exe** trên máy tính.
3. Chuyển sang thẻ **Kết nối**. Nhập địa chỉ IP của iPhone (Vào Cài đặt > Wi-Fi > bấm vào biểu tượng "i" để xem IP).
4. Nhập Tên đăng nhập (thường là `mobile` hoặc `root`) và Mật khẩu (thường là `alpine` nếu bạn chưa đổi). Bấm **Kết nối SSH**.
5. Sau khi kết nối thành công, sang thẻ **Quét hệ thống** và chờ vài giây.
6. Sang thẻ **Kết quả & Xóa**, chọn các mục rác muốn xóa và bấm nút xóa.

## Khôi Phục (Restore)
Nếu bạn có đánh dấu "Sao lưu vào máy tính trước khi xóa", toàn bộ dữ liệu đã xóa sẽ được lưu ở máy tính. Bạn có thể khôi phục lại bất kỳ lúc nào ở thẻ **Khôi phục**.
