# COghe — bản Android trên thiết bị thật

Build đúng mười scene `VenomOrigin01`…`VenomOrigin10` bằng Unity 6000.3.19f1:

```sh
bash Tools/build-venom-android.sh
```

- APK: `Builds/Venom/Android/COghe.apk`.
- Tên ứng dụng: **COghe**; package: `com.gravityboxlab.venom`.
- ARM64, IL2CPP Release, màn hình dọc. APK ký bằng khóa debug local cho chơi thử, không phải bản phát hành cửa hàng.
- Không thay thế ứng dụng Steel Ball Lab cũ `com.gravityboxlab.prototype`.
- Builder phục hồi tên sản phẩm, package, cấu hình build/signing và hướng màn hình của project sau khi xuất.
- Log Unity: `Artifacts/COgheAndroid/unity-build.log`.

Cài cập nhật bằng ADB, giữ dữ liệu ứng dụng đang có:

```sh
adb devices -l
adb -s "$COGHE_ANDROID_DEVICE" install -r Builds/Venom/Android/COghe.apk
adb -s "$COGHE_ANDROID_DEVICE" shell am start -W -n com.gravityboxlab.venom/com.unity3d.player.UnityPlayerGameActivity
```

`COGHE_ANDROID_DEVICE` là serial trả về từ `adb devices -l`; không lưu serial cá nhân trong repository. Nếu máy hỏi cho phép USB hoặc cài đặt, xác nhận trực tiếp trên điện thoại. Không gỡ app hoặc xóa dữ liệu để xử lý lỗi chữ ký mà chưa kiểm tra dữ liệu cần giữ.

Trong game, chạm số màn ở hàng trên; **BOSS** là màn 10. Chạm kính để chỉ đường, kéo để xoay ở màn cho phép. Màn 07, 08 và 10 khóa xoay; chọn từng phần ở hàng nút dưới khi đã phân tách.

## Lần kết nối ngày 16/09/2026

OPPO báo model `CPH2591`, Android 15 / API 35, hỗ trợ ARM64, độ phân giải 720×1612. Build thành công; APK mới nhất 33,645,477 byte (khoảng 32.1 MiB), chứa đủ `level0`…`level9` tương ứng 10 màn Origin. Package/nhãn/ARM64 đã được kiểm tra từ manifest APK.

Kết nối ADB đã được thiết bị cho phép. Lần cài đầu đã hoàn tất: xác nhận package tồn tại, Activity của COghe ở foreground và ảnh chụp màn Boss 10 trên điện thoại. Chưa đo FPS trên thiết bị; thông số phần cứng và kiểm thử Unity không tự chứng minh hiệu năng hoặc cảm giác chơi.

Bản cập nhật vật liệu Boss thay đế nút đen bằng xám bạc nhạt, sửa pháp tuyến dao và thêm cạnh thép vát đánh bóng. Đã build macOS và APK.

Bản mới nhất thêm phản hồi mặt chạm/đích, các dấu hướng dẫn và nắp dày màn 09.
38/38 kiểm thử Origin đạt; kiểm tra hình cuối 2/2 đạt. macOS và APK đã build lại.
Bản APK tiếp theo giữ biểu tượng xoay màn 03 đứng yên. Đã cài cập nhật thành công
lên OPPO lúc 19:57 ngày 16/09/2026 bằng `adb install -r`, giữ dữ liệu ứng dụng.
Đã mở COghe, xác nhận Activity ở foreground và màn 01 hiển thị đúng trên thiết bị.
Ảnh kiểm tra: `Artifacts/COgheAndroid/oppo-latest.png`; thông tin APK và SHA-256:
`Artifacts/COgheAndroid/build-record.json`.
