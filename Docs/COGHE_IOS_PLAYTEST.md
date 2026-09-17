# COghe — build lên iPhone thật

Bản này dùng đúng **10 màn Origin với Day Lab**, scene 01–10 hiện có. Không sinh lại level. Tên hiển thị là **COghe**, bundle ID iOS `com.gravityboxlab.venom`; tách khỏi ứng dụng bi sắt cũ `com.gravityboxlab.prototype`.

## Công cụ

- Unity 6000.3.19f1 và iOS Build Support cùng phiên bản.
- Xcode đầy đủ. Script dùng `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer` riêng cho lệnh build; không đổi `xcode-select` toàn máy.
- iPhone đã ghép đôi với Mac, bật Developer Mode, được đăng ký trong development provisioning profile còn hạn; chứng chỉ tương ứng nằm trong Keychain.
- Giữ điện thoại mở khoá khi cài và mở game.

## Xuất project

Đóng Unity đang mở project rồi chạy:

```sh
bash Tools/build-venom-ios.sh
```

Kết quả ở `Builds/Venom/iOS/Unity-iPhone.xcodeproj`. Mở bằng Xcode, chọn team ký và iPhone rồi Run nếu chưa cung cấp thông tin ký cho script.

Trong Unity có menu **Gravity Box → COghe → Build iPhone Xcode project**. Dùng platform iOS khi chạy menu này.

## Build, ký, cài và mở tự động

Cấp các biến môi trường cục bộ trước khi chạy script:

- `COGHE_IOS_TEAM`: Apple development team ID.
- `COGHE_IOS_PROFILE` (tuỳ chọn): tên hoặc UUID của development provisioning profile **quản lý thủ công** phù hợp bundle ID và điện thoại. Bỏ trống để dùng Automatic Signing của Xcode.
- `COGHE_IOS_DEVICE`: identifier của iPhone từ `xcrun devicectl list devices`, hoặc UDID. Script đọc UDID phần cứng để chọn đúng destination của Xcode.

Khi có team, script build Xcode cấu hình Release. Khi có thêm device, script cài rồi mở `com.gravityboxlab.venom`. Automatic Signing cần tài khoản Xcode có quyền Certificates, Identifiers & Profiles; quyền Admin trên App Store Connect tự nó chưa chứng minh có quyền ký. Chế độ tự động cho phép Xcode tạo/cập nhật profile và đăng ký thiết bị được chỉ định.

Ở chế độ manual, profile chỉ được gắn vào target ứng dụng; UnityFramework ký bằng cùng team và không gắn provisioning profile riêng. Không ghi team, thiết bị, chứng chỉ hoặc private key vào repository.

Sau khi đã xuất Unity, có thể chạy lại với `--native-only` để tiếp tục build/ký/cài mà không xuất lại C#. Chế độ này dùng project hiện có; khi đổi manual profile, cần xuất lại hoặc chỉnh signing trong Xcode trước.

App native ở `Builds/Venom/iOS-DerivedData/Build/Products/Release-iphoneos/COghe.app`. Đây là bản cài trực tiếp để test, không phải gói phân phối App Store/TestFlight.

## Cấu hình và log

- IL2CPP, Release, iOS Device SDK, màn hình dọc. Không bật Script Debugging hoặc kết nối Profiler tự động.
- Giữ nguyên hạt mô phỏng, lực, vật liệu và logic puzzle. Builder phục hồi các PlayerSettings tạm sau khi xuất.
- Log Unity: `Artifacts/COgheIOS/unity-export.log`.
- Log Xcode: `Artifacts/COgheIOS/xcode-build.log`.
- Builder: `Assets/_Game/Editor/COgheIOSBuilder.cs`.

Sau khi cài, chọn màn qua dãy 01–09/BOSS trên cùng. Chạm chỉ đường; kéo xoay ở các màn cho phép. Dùng nút Nhìn gần để kiểm tra bám, biến dạng và vật liệu. Theo dõi riêng độ mượt, nhiệt máy, vùng notch/home indicator và cảm giác chạm; việc build thành công chưa phải kết quả đo hiệu năng dài hạn.

## Kiểm tra ngày 16/09/2026

Unity đã xuất thành công 10 scene. Xcode 26.6 đã biên dịch bản Release ARM64 thành công khi kiểm tra không ký; app khoảng 130 MB trên đĩa. Đây chưa phải kích thước tải từ App Store.

Bước ký/cài thiết bị đang chờ tài khoản Apple Developer có quyền ký: Xcode báo thiếu tài khoản hợp lệ cho team đã chọn và chứng chỉ development đang lưu không hợp lệ. Chưa xác nhận game chạy trên iPhone; bản không ký không cài được lên thiết bị. Sau khi sửa tài khoản, chạy lại script với `--native-only` để tiếp tục.
