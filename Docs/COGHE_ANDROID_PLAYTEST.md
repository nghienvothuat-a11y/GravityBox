# COghe — bản Android trên thiết bị thật

## NewGraphic — Spatial 60 màn (07/10/2026)

```sh
bash Tools/build-venom-android.sh --spatial
```

Unity: **Gravity Box → COghe → Spatial pilot → Build Android test APK · 60 levels**.
Đóng gói theo `VenomCampaignBuilder.SpatialOrder`: 60 màn trong năm chương,
mỗi chương 12 màn, gồm các màn Spatial, Spatial Plus và K01–K10. Thứ tự hiện hành
ở `Assets/_Game/Editor/COgheSpatialCampaignBuilder.cs`; thiếu scene thì dừng build.
APK: `Builds/SpatialLab/Android/COghe-Spatial.apk`. App **COghe**, package
`com.gravityboxlab.venom`; ARM64, IL2CPP Release, portrait, ký debug local để test.
Hộp đứng yên, chạm giao việc, kéo đổi góc camera; Product chơi lần lượt từ Play.
Bản test có công cụ chọn màn trong Pause; `COGHE_STORE=1` loại công cụ này khỏi build.
Icon ứng dụng dùng phương án C đã duyệt, được áp dụng tự động trước khi build.
Menu/script không có `--spatial` vẫn dùng catalog cũ, không phải bộ Spatial.

Cài bằng `adb install -r Builds/SpatialLab/Android/COghe-Spatial.apk`, rồi mở app
như lệnh bên dưới. Không cần gỡ app hay xoá dữ liệu. Build thành công chỉ chứng minh
APK được tạo; cần kiểm tra trên thiết bị để kết luận input, gameplay hoặc FPS.

## Hướng dẫn và lịch sử catalog cũ

### Xác minh icon C trên OPPO — 07/10/2026

Build Spatial 60 thành công bằng Unity 6000.3.19f1, IL2CPP ARM64 Release. APK có
60 scene, chữ ký kiểm tra hợp lệ và đủ sáu kích thước launcher icon. Ảnh nguồn
giữ nguyên phương án C đã duyệt; Android exporter thu nhỏ thành PNG cho thiết bị.
Cài thành công trên OPPO CPH2591 / Android 15, xác nhận icon mới trong trình cài
đặt và Main Menu có sinh vật 3D chuyển động. Không thấy Unity/AndroidRuntime error
trong mẫu log khởi động; chưa chạy lại toàn bộ campaign hoặc benchmark FPS.

Bằng chứng cục bộ: `Artifacts/COgheAndroid/icon-c-build-record.json`,
`icon-c-from-apk.png` và `unity-build.log`. APK 53,810,499 byte (51.3 MiB),
SHA-256 `0ea99e52e2125bbd701d15f808fe4eeb3c911315b03ba8f8763eda7f80a1a3a4`.

Build đúng hai mươi scene `VenomOrigin01`…`VenomOrigin20` bằng Unity 6000.3.19f1:

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

Trong game, chọn tab **01–10 / 11–20**, rồi chạm số màn ở hàng trên; **BOSS** là màn 10 và 20. Chạm kính để chỉ đường, kéo để xoay ở màn cho phép. Màn 07, 08 và 10 khóa xoay; chọn từng phần ở hàng nút dưới khi đã phân tách.

## Đợt tối ưu ngày 17/09/2026

Đo tự động từng màn 11–20 trên OPPO, bằng APK ARM64 IL2CPP Release có instrumentation tùy chọn. Bản chơi cuối đã cài lên OPPO bằng ADB, kiểm tra SHA-256 khớp và mở game thành công; giữ dữ liệu ứng dụng. Bản chơi bình thường không tự chạy benchmark. Xem [số đo, cách tái hiện và giới hạn](COGHE_MOBILE_PERFORMANCE.md).

## Lịch sử kết nối ngày 16/09/2026

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
