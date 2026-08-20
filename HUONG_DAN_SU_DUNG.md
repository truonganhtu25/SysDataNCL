# 📱 iPhone System Data Cleaner — Hướng dẫn sử dụng

**Phiên bản:** 1.0 | **Hỗ trợ:** Jailbreak Rootless (Dopamine, palera1n) | **Windows x64**

---

## ⚡ Yêu cầu

| Thiết bị | Yêu cầu |
|---|---|
| iPhone | Đã jailbreak bằng **Dopamine** hoặc **palera1n** |
| iPhone | Đã cài **OpenSSH** (qua Sileo) |
| Máy tính | Windows 10/11 x64 |
| Mạng | iPhone và máy tính cùng mạng Wi-Fi |

---

## 🍎 PHẦN 1: Chuẩn bị trên iPhone

### Bước 1 — Cài OpenSSH

1. Mở **Sileo** trên iPhone đã jailbreak
2. Vào tab **Sources** → nhấn **Edit** (góc phải trên) → **Add**
3. Nhập địa chỉ source:
   ```
   https://apt.procurs.us
   ```
4. Nhấn **Add Source** → chờ tải xong
5. Vào **Search** → gõ **"OpenSSH"**
6. Chọn **OpenSSH** từ source `apt.procurs.us` → **Get** → **Confirm**
7. Respring nếu Sileo yêu cầu

> ✅ Sau khi cài xong, SSH server tự động chạy trên cổng 22. iPhone của bạn đã sẵn sàng nhận kết nối.

---

### Bước 2 — Lấy địa chỉ IP của iPhone

1. Vào **Cài đặt** ⚙️ → **Wi-Fi**
2. Nhấn vào tên mạng Wi-Fi đang kết nối (biểu tượng ℹ️)
3. Tìm dòng **Địa chỉ IP** (hoặc IP Address)
4. Ghi lại địa chỉ IP (dạng: `192.168.x.xxx`)

---

### Bước 3 — (Khuyến nghị) Đổi mật khẩu root

> ⚠️ Mật khẩu mặc định `alpine` rất phổ biến — nên đổi để bảo mật thiết bị

1. Cài **NewTerm** từ Sileo (tìm "NewTerm")
2. Mở NewTerm → gõ lệnh:
   ```bash
   su
   ```
3. Nhập mật khẩu hiện tại: `alpine`
4. Đổi mật khẩu root:
   ```bash
   passwd
   ```
5. Nhập mật khẩu mới 2 lần (nhớ kỹ mật khẩu này!)
6. Đổi luôn mật khẩu user mobile:
   ```bash
   passwd mobile
   ```

Bác nào dùng Dopamine thì cái mật khẩu lúc mới jb nó bắt thiết lập là mật khẩu root đấy, không cần cài NewTerm nữa
---

## 💻 PHẦN 2: Sử dụng tool trên Windows

### Bước 4 — Mở tool

1. Kết nối máy tính **cùng mạng Wi-Fi** với iPhone
2. Mở file **`iPhoneSystemCleaner.exe`** (không cần cài đặt, chạy luôn)
3. Nếu Windows Defender cảnh báo → nhấn **More info** → **Run anyway**

---

### Bước 5 — Kết nối SSH

Tại tab **"🔌 Kết nối"**:

| Trường | Giá trị |
|---|---|
| Địa chỉ IP iPhone | IP từ Bước 2 (vd: `192.168.1.100`) |
| Cổng SSH | `22` (mặc định) |
| Tên đăng nhập | `root` |
| Mật khẩu Root | `alpine` (hoặc mật khẩu đã đổi) |

Nhấn **🔌 Kết nối SSH**

> ✅ Nếu kết nối thành công, tool hiển thị thông tin thiết bị (tên iPhone, iOS version, dung lượng ổ đĩa)

**Lỗi thường gặp:**
- `Sai tên đăng nhập hoặc mật khẩu` → Kiểm tra lại mật khẩu root
- `Không thể kết nối` → Kiểm tra iPhone và PC cùng mạng Wi-Fi, OpenSSH đã cài
- `Connection refused` → Thử respring iPhone, kiểm tra OpenSSH có đang chạy không

---

### Bước 6 — Quét hệ thống

1. Nhấn tab **"🔍 Quét hệ thống"**
2. Nhấn **"🔍 Bắt đầu quét"**
3. Chờ tool quét qua **12 nhóm file** (thường mất 1-3 phút tùy dung lượng)
4. Theo dõi log trong ô terminal màu xanh lá bên dưới
5. Sau khi hoàn tất, tool tự chuyển sang tab Kết quả

---

### Bước 7 — Chọn & Xóa

Tại tab **"🗂️ Kết quả & Xóa"**:

1. Xem danh sách 14 nhóm file với **kích thước** và **mức độ an toàn**:
   - 🟢 **An toàn** — xóa không ảnh hưởng hệ thống
   - 🟡 **Cẩn thận** — hệ thống sẽ rebuild lại, có thể chậm hơn một lúc
   - 🔴 **Rủi ro** — kiểm tra kỹ trước khi xóa

2. Tick vào các nhóm muốn xóa (mặc định tick tất cả nhóm có dữ liệu)

3. Nhấn **"🗑️ Xóa đã chọn"**

4. Xác nhận trong hộp thoại cảnh báo → **Yes**

5. Chờ tool xóa và xem log real-time

6. Kết quả: dung lượng đã được giải phóng

---

## 📋 14 Nhóm file được quét

| # | Nhóm | Mô tả | An toàn |
|---|---|---|---|
| 1 | 💥 Crash Reports | Báo cáo lỗi ứng dụng | ✅ |
| 2 | 📋 System Logs | Log hệ thống | ✅ |
| 3 | 🗑️ Temp Files | File tạm thời | ✅ |
| 4 | 🔋 Battery Archives | Thống kê pin cũ | ✅ |
| 5 | 🌐 Safari Cache | Cache trình duyệt | ✅ |
| 6 | 🧊 Stuck App Cache | Cache bị "kẹt" từ app đã xóa | ✅ |
| 7 | 🩺 Diagnostic Logs | Log chẩn đoán hệ thống | ✅ |
| 8 | 🔍 Spotlight Cache | Index tìm kiếm (tự rebuild) | ⚠️ |
| 9 | 📦 OTA Update Cache | File cập nhật iOS tải về | ✅ |
| 10 | 📱 Sileo/Cydia Cache | Cache package manager | ✅ |
| 11 | 🗂️ App Cache | Cache từng ứng dụng (chọn lọc) | ⚠️ |
| 12 | 🏚️ App Leftover Data | Dữ liệu app đã gỡ còn sót | 🔴 |
| 13 | 🧠 Apple AI & ML Data | Dữ liệu huấn luyện AI của Apple | 🔴 |
| 14 | 🖼️ Wallpaper & Posters Cache | Cache hình nền/Lockscreen | ⚠️ |

---

## ❓ Câu hỏi thường gặp

**Q: Tool có xóa dữ liệu ứng dụng (ảnh, tin nhắn) không?**  
A: Không, tool chỉ xóa file cache/log tạm thời. Nhưng với nhóm 🔴 "App Leftover Data", cần kiểm tra kỹ.

**Q: Sau khi xóa iPhone có bị lỗi không?**  
A: Các nhóm ✅ "An toàn" không gây lỗi. Nhóm ⚠️ có thể khiến app cần tải lại cache.

**Q: Tôi cần chạy lại jailbreak không?**  
A: Không, tool không ảnh hưởng đến trạng thái jailbreak.

**Q: Cổng SSH mặc định là gì?**  
A: Mặc định là 22. Một số jailbreak (palera1n) có thể dùng cổng 2222.

**Q: Tool có lưu mật khẩu của tôi không?**  
A: Không, mật khẩu chỉ dùng trong session hiện tại, không lưu vào ổ đĩa.

---

## 🔒 Lưu ý bảo mật

> ⚠️ Chỉ sử dụng tool này khi **không kết nối Wi-Fi công cộng**
> 
> ⚠️ Tắt SSH sau khi dùng xong: Gỡ OpenSSH khỏi Sileo hoặc dùng NewTerm gõ `launchctl unload /Library/LaunchDaemons/com.openssh.sshd.plist`

---

*iPhone System Data Cleaner © 2026 | Dành cho thiết bị jailbreak*
