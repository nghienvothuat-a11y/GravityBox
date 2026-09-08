# Nhật ký phát triển Gravity Box

## Baseline ngày 08 tháng 09 năm 2026

Workspace ban đầu `/Users/mrk/GravityBox` trống, chưa có Unity project hoặc Git repository. Đã đọc đầy đủ GDD Word do người dùng cung cấp; nội dung prompt mẫu được giữ dưới dạng nguồn tham khảo, không dùng để giới hạn yêu cầu hiện tại thành riêng M1.

## Mốc và kết quả

| Mốc | Thay đổi | Kiểm chứng | Trạng thái |
| --- | --- | --- | --- |
| M0 | Unity 6000.3.19f1, URP 17.3, Input System 1.17, asmdef, portrait, project settings | Unity import/compile và build | Đã thực hiện |
| M1 | Camera cố định, acrylic shell, mouse/touch, assisted/free/quarter-turn, tuning asset | Rotation cap, 24 orientation; integration mouse/touch và reset button | Đạt automated; feel cần người chơi |
| M2 | Ball world-space, explicit force provider, ramp, exit capture | Gravity acceleration; zero-G không steering; render native | Đạt kiểm thử |
| M3 | Session state, reset registry, fail, transition cancellation | 100 resets; exit/fail event một lần; reset khi completing | Đạt kiểm thử |
| M4 | Catalog/definition/prefab, persistent Gameplay scene | Load 16 definition đúng profile; advance tới Finished; replay | Đạt kiểm thử |
| M5 | Plate, signal door, one-way gate, spring/impulse, kill volume | Trigger, channel prerequisite, cooldown reset, collision-ignore reset | Đạt kiểm thử |
| M6 | Zero-G profile, launch velocity, momentum trail | 120 physics ticks không mất momentum; root rotation không đổi velocity khi không contact | Đạt kiểm thử |
| M7 | 10 gravity + 6 zero-G layouts, solution records | 16/16 physics-only solve routes được replay; spawn validator | Khả giải kỹ thuật; cần cân bằng qua playtest |
| M8 | HUD, level selector, pause/debug, audio, safe area, ball occlusion cue | Native screenshot; automated UI input; kiểm tra shader trong build | Một phần; chưa nghiệm thu thiết bị thật |
| M9 | macOS player, Android APK, iOS Simulator export, CLI commands | macOS và Android build thành công; iOS xem phần nền tảng | Chưa đạt GDD device/playtest gate |

## Test suite

7 Edit Mode + 19 Play Mode = **26 bài test**, lượt đầy đủ sau sửa input đều pass. Lượt chạy đầy đủ cuối: 08/09/2026, Play Mode 04:08:03–04:08:21 UTC. Phần hình ảnh được kiểm tra thêm bằng build và ảnh native.

Edit Mode bao gồm reset registry deduplication, terminal transition, pause gate, gravity profiles, 24 canonical rotations, instance signal isolation và catalog integrity.

Play Mode bao gồm mouse drag, fast drag giữa frame, one-finger touch, vuốt chỉ có begin/end vẫn giữ đoạn di chuyển cuối, UI reset không xoay hộp, gravity acceleration, zero-G drift và rotation invariance, speed limit, 100 resets, launch-velocity restore, exit/hazard, plate-door prerequisite, one-way ignore pair, bumper cooldown, load mọi level, cancel pending next, toàn bộ progression và replay solve routes.

Kết quả XML/log nằm tại `Artifacts/editmode-results.xml`, `Artifacts/playmode-results.xml`, `Artifacts/editmode.log`, `Artifacts/playmode.log`. Chạy lại bằng `bash Tools/verify.sh` khi Editor không giữ project lock.

Các phép reset kiểm tra state khôi phục, không tuyên bố bitwise deterministic trên mọi PhysX/platform. Bài khả giải điều khiển rotation intent quanh world Z; chứng minh có đường vật lý tới exit, chưa chứng minh người mới thực hiện dễ dàng bằng camera-relative drag. Chi tiết trong [SOLVABILITY.md](SOLVABILITY.md).

## Lỗi đã tìm và sửa

- asmdef test ban đầu khai báo runner trùng với TestAssemblies; đã bỏ tham chiếu trùng.
- Tên SessionState xung đột UnityEditor trong tests; đã dùng alias tường minh.
- Input integration trong headless Editor bị focus routing chặn; harness tạm đặt IgnoreFocus/AllDeviceInputAlwaysGoesToGameView và phục hồi trong teardown. Không đổi focus policy của game để làm test pass.
- Mouse polling có thể mất thao tác đã hoàn tất giữa hai frame; chuyển capture press/release sang InputAction events và có regression test.
- Touch kết thúc vẫn có thể mang vị trí mới; áp delta cuối trước EndDrag, có regression test cho gesture không có moved sample.
- HUD fixed width có thể tràn trên điện thoại dài; CanvasScaler hiện match theo chiều ngang, phần cao bổ sung phân phối giữa các anchor.
- Bóng có thể bị vách đặc che; thêm một silhouette pass chỉ hiện phần bị occlude, dùng cùng mesh sphere và không thêm collider/force. Camera framing radius nâng lên 5.3 để tính tới corner khi xoay tự do.
- Xcode Simulator lần đầu lỗi link do Unity export x86_64 nhưng compile arm64. BuildIOSSimulator hiện chọn ARM64 tường minh, đồng thời phục hồi setting sau export.

## Nền tảng và giới hạn

**macOS:** development player build thành công, chạy native trên Apple M5. Đã xem L01/L03/L14, level selector đủ 16 màn, reset keyboard và UI, pause/diagnostics, lưu ảnh bằng F12. Chuột/touch được kiểm tra end-to-end bằng Input System events; macro kéo của công cụ điều khiển desktop không cho một cử chỉ kéo liên tục đáng tin cậy, nên không dùng macro đó để tuyên bố feel đã đạt.

**Android:** APK development ARM64/IL2CPP được tạo tại `Builds/Android/GravityBox.apk`, khoảng 53 MB ở bản cuối. ADB không có thiết bị kết nối; SDK không có system image emulator. Chưa cài/chạy hoặc profile Android thật.

**iOS:** Xcode build ARM64 thành công và đã cài/chạy trên iPhone 17 Simulator với iOS 26.5. Đã kiểm tra bố cục portrait/safe area và chạm nút Reset. Export simulator tắt MSAA để tránh mismatch attachment 1/4 samples của Metal; asset baseline được phục hồi về MSAA 4 sau export. Không còn lỗi render attachment ở bản đã sửa. URP còn một warning fallback shadow depth 16-bit từ package trên simulator, không chặn gameplay. Chưa có iPhone thật hoặc signing được xác minh. Script tái lập: `Tools/run-ios-simulator.sh`.

Không đo FPS/CPU/GPU p95/GC allocations trên Android/iPhone thật; chưa có thermal/battery test hoặc dữ liệu từ người mới. M8/M9 vì vậy chưa được đánh dấu hoàn tất theo Definition of Done toàn phần của GDD.

## Tuning hiện tại

| Tham số | Giá trị |
| --- | --- |
| Fixed timestep | 1/60 giây |
| Target FPS | 60 |
| Base gravity | 9.81 m/s² |
| Zero-G damping | 0 linear, 0 angular |
| Ball mass/radius | 1 kg / 0.27 m |
| Max speed / angular speed | 12 m/s / 35 rad/s |
| Max depenetration speed | 3 m/s |
| Rotation sensitivity | 200° / chiều ngắn màn hình |
| Rotation cap / smoothing | 100°/s / 0.09 giây |
| Rotation backlog / snap cone | 14° / 10° |
| Completion / failure feedback | 0.8 / 0.7 giây |

## Việc tiếp theo có ưu tiên

1. Chơi touch trên một Android tầm trung và một iPhone thật, profile 15–20 phút ở 30/60 FPS và thao tác nhanh.
2. Ghi route từ thao tác người chơi, làm rõ L06/L10/L14 và giảm khả năng giải do quay ngẫu nhiên; thu phản hồi zero-G trước khi thêm lực mới.
3. Tuning tiếng động/impact; haptics, sensitivity settings, localization/accessibility chưa triển khai đầy đủ.
4. Xác nhận quyết định physical-root hoặc thử nhánh logical gravity nếu có contact instability trên thiết bị.
5. Qua GO gate mới triển khai save schema, loading bất đồng bộ nếu cần, material profiles và Phase 2. Kế hoạch chi tiết ở [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md).
