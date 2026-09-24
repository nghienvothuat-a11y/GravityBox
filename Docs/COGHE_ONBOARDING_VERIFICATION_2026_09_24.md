# COghe onboarding pilot — kiểm chứng 24/09/2026

**Mốc bản thử10 màn đã sẵn sàng.** Toàn bộ60 màn mới chỉ có bảng đề xuất; chưa áp thứ tự đó vào campaign production. Chương xoay/trơn và tách/hợp sẽ tiếp tục sau phiên người mới theo plan.

## Bản nguồn và artifact

- Worktree `REPOS/GravityBox-macos-preview`, branch `codex/venom-macos-preview`, base HEAD `0f4a48e45095e4067f2cf87b624284914d729b90` **cộng thay đổi chưa commit**. Có cả công việc campaign60 từ lượt trước. Không gán kết quả này cho HEAD sạch.
- Unity6000.3.19f1. Manifest `Artifacts/COgheOnboarding/HANDOFF_MANIFEST.json` liên kết snapshotSHA256 của7526 file Assets/Packages/ProjectSettings/Tools. Các file đầu vào vẫn khớp sau test và cả hai build.
- Mac: `Builds/COgheOnboarding/macOS/COghe Learn.app`, Development; buildGUID `2a67191a3c1b4864a2b52e4b44438e7e`. Checksum từng file tại `Artifacts/COgheOnboarding/mac-bundle-sha256.json`.
- Android: `Builds/COgheOnboarding/Android/COghe-Learn.apk`, 33,512,821byte; ARM64/IL2CPP Release, test signing. App `COghe Learn`, package `com.gravityboxlab.venom.onboarding`, version0.1.0(1), minSDK26, target36. Kiểm tra bằng `aapt dump badging`; kết quả tại `Artifacts/COgheOnboarding/apk-badging.txt`.
- APK SHA256: `ef2cd4e3c276ea5b79987f035b4f73e56fa354b55f8dd46a9f4bbf3c18386065`.
- Build logs: `Artifacts/COgheOnboarding/build-mac-handoff.log`, `build-android-handoff.log`; cả hai có success marker và process exit0.
- Buzz CLI từ chối uploadAPK với lỗi `unsupported file type: application/zip`. Artifact local vẫn đầy đủ; không có link tải APK từ relay.

## Kiểm chứng đã đạt

| Bằng chứng | Kết quả | Phạm vi |
| --- | --- | --- |
| Full PlayMode |421/421,0 failed/skipped | XML `Artifacts/COgheOnboarding/playmode-handoff.xml`, 2026-09-24 12:04:14Z đến 2026-09-24 12:10:08Z |
| Full EditMode |8/8,0 failed/skipped | XML `Artifacts/COgheOnboarding/editmode-handoff.xml`, 2026-09-24 12:11:00Z |
| Mac540×960 |10/10,32 hạt thoát /1 cơ thể mỗi bài,0 runtime error | `Artifacts/COgheOnboarding/player-handoff-standard/20260924T120634457Z/run.json` |
| Mac360×806 |10/10,32 hạt thoát /1 cơ thể mỗi bài,0 runtime error | `Artifacts/COgheOnboarding/player-handoff-tall/20260924T120412766Z/run.json` |
| Geometry bản thử |10/10 scene giữ nguyên transform/body/collider/joint so nguồn | `Artifacts/COgheOnboarding/PILOT_GEOMETRY_AUDIT.json` |
| Production |240/240 scene/definition/meta khớp baseline đầu lượt | Source slots01–60, không regenerate nguồn |
| Catalog đề xuất |60 slot duy nhất, tiên quyết đứng trước, Boss mỗi10 | `Docs/LevelDesign/COghe/ONBOARDING_PROGRESSION_DRAFT.md` |

Hai kích thước player giữ đúng tỷ lệ720×1280 và720×1612, chạy trên Mac16,10/AppleM4. Replay dùng FixedUpdate bình thường và InputSystem mouse press/release qua input gameplay; lệnh buông và đổi scene qua API công khai. Không teleport, sửa vận tốc hay ép thắng. Các report có điểm dự định/điểm lệnh thực nhận. Đây là bằng chứng khả giải và hiển thị, không phải thử nghiệm với người mới.

18 ca onboarding bổ sung bao gồm10 lời giải, lời giải thùng ở tỷ lệ dài, điểm đích thật của thùng/mặt kính, trạng thái bám/chốt, bỏ qua/xem lại/pause/retry, Boss không lộ đáp án, thứ tự/chặn vượt catalog, và cách ly save theo ID. Lượt full dùng Metal; `-nographics` không phù hợp các ca render texture. Ba material kính bị suite cũ thay đổi được trả đúng byte baseline sau kiểm traSHA256; source input đã được đối chiếu lại. `cases.jsonl` cộng dồn các lượt; XML cuối mới là số liệu nghiệm thu.

## Các chi tiết đã sửa qua kiểm chứng

- Ngón tay/vòng nhấn giữ đúng vị trí trên canvas co giãn; ma trận vẽ dùng tọa độ HUD thống nhất.
- Điểm leo bài02 nằm trên mặt kính thật, không trúng mái ở góc camera hiện tại.
- Gợi ý đích chỉ dịu đi khi có lệnh đích và chuyển động có tiến triển. Rung lúc mới bám không làm mất hướng dẫn chạm lần hai.
- Bài thùng ở tỷ lệ dài từng không nhận điểm đích ngay trên đường nối tam giác dưới lỗ. Dời điểm chỉ dẫn8mm theoZ, giữ geometry/physics; trace cuối nhận x=.3/z=.008 và bài qua bằng3 chạm. Hai ca hồi quy khóa hành vi này.
- Đánh số trên HUD, chọn màn và bảng gắn vật thể đồng bộ; ID nội dung và save production giữ nguyên. Pilot có namespace save và package Android riêng.

## Việc cần người chơi/thiết bị thật

Chưa có điện thoại kết nối để cài/chạy APK. Chưa chạy native HUD bằng chạm thật, notch/safe area thực tế, hiệu năng/nhiệt15–20 phút hoặc phiên5–8 người mới. Dịch vụ Computer Use trong phiên này không có native pipe; không lấy replay làm bằng chứng click native HUD. Pause/retry/help được kiểm chứng ở trạng thái/API và layout; thao tác nút thật vẫn nằm trong phiếu thử.

Dùng [phiếu quan sát và tiêu chí](COGHE_ONBOARDING_PILOT_2026_09_24.md) để đo ít nhất80% tự qua ba bài đầu, phân biệt một chạm với tay nắm→đích→buông, rồi hiệu chỉnh. Catalog60, bài xoay/trọng lực, bài tách/hợp và bước đệm ba vai trò chưa được nghiệm thu bởi pilot này.

Mốc tiếp theo có thể bắt đầu bằng cài APK riêng và thử với5–8 người chưa xem lời giải. Source của lượt này chưa commit/push.
