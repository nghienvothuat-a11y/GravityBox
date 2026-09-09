# APK Android — Campaign 100

Build ngày 09/09/2026 bằng Unity 6000.3.19f1. File để cài thử:

`Builds/Android/GravityBox-Campaign100.apk`

## Nội dung và cấu hình

- Scene nhúng: `Assets/_Game/Scenes/Campaign.unity`.
- Catalog: `gravity-box-campaign-v1`, 100 màn C001–C100 và 10 boss; Physics Lab 23 màn vẫn truy cập được từ Campaign.
- Development build, IL2CPP, ABI duy nhất `arm64-v8a`, portrait.
- Package: `com.gravityboxlab.prototype`, version `0.1.0` / code 1.
- Min SDK: API 26 (Android 8.0); target/compile SDK: API 36.
- Kích thước thật: 59.272.201 bytes.
- SHA-256: `fac50d77fceb3b65b1a5dfe11e993c269e7f93fbf8523db15f680e1c11f88d3c`.

[Tóm tắt đọc bằng máy](build-summary.json).

## Kiểm tra gói

- Unity build log kết thúc bằng `GRAVITY BOX CAMPAIGN BUILD SUCCESS` và ghi rõ target Android, output `GravityBox.apk`.
- Log mở `Assets/_Game/Scenes/Campaign.unity` trong bước Building scenes.
- Dữ liệu APK chứa `gravity-box-campaign-v1`, `Hành trình 100 màn`, chuỗi ID `campaign-001`…`campaign-100` và asset C100.
- `unzip -t` không phát hiện lỗi; `GravityBox.apk` và `GravityBox-Campaign100.apk` byte-identical.
- `aapt dump badging` xác nhận package/version/min/target SDK; APK chỉ chứa thư mục native `lib/arm64-v8a`.
- `apksigner verify` đạt với APK Signature Scheme v2 và chứng thư `Android Debug`.

Không có thiết bị trong `adb devices` ở lượt build, nên chưa cài/chạy, đo FPS, nhiệt, bộ nhớ hoặc đánh giá touch/readability trên Android thật.

## Cài để test

Thiết bị phải là ARM64 và chạy Android 8.0 trở lên. Bật quyền cài ứng dụng không rõ nguồn rồi mở APK, hoặc dùng:

```bash
adb install -r Builds/Android/GravityBox-Campaign100.apk
```

Nếu máy đã cài một build cùng package nhưng dùng chữ ký khác, gỡ `com.gravityboxlab.prototype` trước rồi cài lại. Việc gỡ app sẽ xóa tiến độ local của build đó.
