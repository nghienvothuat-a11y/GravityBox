# COghe — build lên iPhone / iPad thật

Bản hiện tại dùng **50 màn Spatial với Product UI**, cùng catalog của Android/Mac.
Không sinh lại level. Tên hiển thị là **COghe**, bundle ID iOS
`com.gravityboxlab.venom`; tách khỏi ứng dụng bi sắt cũ `com.gravityboxlab.prototype`.
Lệnh không có `--spatial` vẫn xuất catalog Origin lịch sử.

## Công cụ

- Unity 6000.3.19f1 và iOS Build Support cùng phiên bản.
- Xcode đầy đủ. Script dùng `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer` riêng cho lệnh build; không đổi `xcode-select` toàn máy.
- iPhone/iPad chạy iOS/iPadOS **15 trở lên**, đã ghép đôi với Mac, bật Developer Mode,
  được đăng ký trong development provisioning profile còn hạn; chứng chỉ tương ứng
  nằm trong Keychain. Tài khoản Xcode phải có team ký hợp lệ, kể cả Personal Team.
- Giữ điện thoại mở khoá khi cài và mở game.

## Xuất project

Đóng Unity đang mở project rồi chạy:

```sh
bash Tools/build-venom-ios.sh --spatial
```

Kết quả ở `Builds/SpatialLab/iOS/Unity-iPhone.xcworkspace`. Mở **workspace** bằng
Xcode để nạp cả CocoaPods, chọn team ký và thiết bị rồi Run nếu chưa cung cấp
thông tin ký cho script. Nếu export không có CocoaPods, dùng `.xcodeproj`.

Trong Unity có menu **Gravity Box → COghe → Spatial pilot → Build iPhone-iPad Xcode
project · 50 levels**. Dùng platform iOS khi chạy menu này. Script tự khôi phục các
SDK đã ghim phiên bản qua `Tools/setup-google-sdks.py` trước khi xuất.

## Build, ký, cài và mở tự động

Cấp các biến môi trường cục bộ trước khi chạy script:

- `COGHE_IOS_TEAM`: Apple development team ID.
- `COGHE_IOS_PROFILE` (tuỳ chọn): tên hoặc UUID của development provisioning profile **quản lý thủ công** phù hợp bundle ID và điện thoại. Bỏ trống để dùng Automatic Signing của Xcode.
- `COGHE_IOS_DEVICE`: identifier của iPhone/iPad từ `xcrun devicectl list devices`, hoặc UDID. Script đọc UDID phần cứng để chọn đúng destination của Xcode.

Khi có team, script build Xcode cấu hình Release. Khi có thêm device, script cài rồi mở `com.gravityboxlab.venom`. Automatic Signing cần tài khoản Xcode có quyền Certificates, Identifiers & Profiles; quyền Admin trên App Store Connect tự nó chưa chứng minh có quyền ký. Chế độ tự động cho phép Xcode tạo/cập nhật profile và đăng ký thiết bị được chỉ định.

Ở chế độ manual, profile chỉ được gắn vào target ứng dụng; UnityFramework ký bằng cùng team và không gắn provisioning profile riêng. Không ghi team, thiết bị, chứng chỉ hoặc private key vào repository.

Sau khi đã xuất Unity, chạy `bash Tools/build-venom-ios.sh --spatial --native-only`
để tiếp tục build/ký/cài mà không xuất lại C#. Chế độ này dùng project hiện có;
khi code thay đổi phải xuất lại để có bản mới. Khi đổi manual profile, cần xuất lại
hoặc chỉnh signing trong Xcode trước.

App native ở `Builds/SpatialLab/iOS-DerivedData/Build/Products/Release-iphoneos/COghe.app`.
Đây là bản cài trực tiếp để test, không phải gói phân phối App Store/TestFlight.

## Cấu hình và log

- IL2CPP, Release, iOS Device SDK, màn hình dọc. Không bật Script Debugging hoặc kết nối Profiler tự động.
- Giữ nguyên hạt mô phỏng, lực, vật liệu và logic puzzle. Builder phục hồi các PlayerSettings tạm sau khi xuất.
- Log Unity: `Artifacts/COgheIOS/unity-export.log`.
- Log Xcode: `Artifacts/COgheIOS/xcode-build.log`.
- Builder: `Assets/_Game/Editor/COgheIOSBuilder.cs`.

Sau khi cài, bấm Play ở Main Menu. Product chơi lần lượt; chạm chỉ đường, kéo đổi
góc camera và dùng hai ngón để zoom. Màn thắng chờ người chơi bấm dòng chuyển màn.
Theo dõi riêng độ mượt, nhiệt máy, safe area và cảm giác chạm; build thành công
chưa phải kết quả đo hiệu năng dài hạn.

Google services hiện chỉ khởi động provider thật trên Android. Bản iOS này phục vụ
test gameplay/UI, chưa xác nhận Firebase/AdMob thật hoặc IAP trên iOS. Khi iOS AdMob
app ID chưa cấu hình, export test tạm dùng sample ID chính thức của Google rồi phục
hồi asset; `COGHE_STORE=1` yêu cầu cấu hình iOS thật. Không đưa thông tin ký vào Git.

## Kiểm tra thiết bị — 04/10/2026, Spatial 50

Đã build Release ARM64 bằng Unity 6000.3.19f1 và Xcode 26.6, ký bằng Personal
Team, cài và mở thành công trên iPad thế hệ 8 chạy iPadOS 26.7.1. Thiết bị cần
bật Developer Mode và xác nhận tin cậy developer profile lần đầu trong Cài đặt.

Xác minh chữ ký thành công; app tiếp tục chạy sau 30 giây. Ảnh chụp trực tiếp từ
iPad hiển thị đoạn giới thiệu và nút Skip. Chưa kiểm thử hết 50 màn hoặc đo hiệu
năng dài hạn trên iPad. Bản này gồm giới hạn zoom mới và màn thắng chờ người chơi
chọn dòng chuyển màn. Build từ HEAD `dee018ca` cùng các thay đổi vật liệu/metadata
có sẵn trong working tree; không phải bản checkout sạch.

Bằng chứng cục bộ (không đưa thông tin thiết bị/chữ ký vào Git):
`Artifacts/COgheIOS/build-result.json`, `launch.json`, `running-process.json`
và `ipad-first-launch.png`.

## Mốc lịch sử — 16/09/2026, Origin

Unity đã xuất thành công 10 scene. Xcode 26.6 đã biên dịch bản Release ARM64 thành công khi kiểm tra không ký; app khoảng 130 MB trên đĩa. Đây chưa phải kích thước tải từ App Store.

Bước ký/cài thiết bị đang chờ tài khoản Apple Developer có quyền ký: Xcode báo thiếu tài khoản hợp lệ cho team đã chọn và chứng chỉ development đang lưu không hợp lệ. Chưa xác nhận game chạy trên iPhone; bản không ký không cài được lên thiết bị. Sau khi sửa tài khoản, chạy lại script với `--native-only` để tiếp tục.
