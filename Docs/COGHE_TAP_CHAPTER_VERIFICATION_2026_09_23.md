# COghe — chapter 01–10 đã kiểm chứng — 23/09/2026

**Đã hoàn thành 5 màn tiếp theo để đủ 10 màn thử chung:** 382/382 PlayMode, 8/8 EditMode; app Mac qua 10/10, mỗi màn đủ 32 hạt trong một cơ thể, không error/exception. Boss 10 mở Nhà. Đây là nghiệm thu kỹ thuật trên Mac; chưa có novice playtest hoặc kiểm chứng điện thoại.

Mrk cho phép mở rộng ngay qua event `185e1e5d67f04a4c4d00919d9960accc7afb7cc4b3ef749ef9cb45155dfcf8d0`. Yêu cầu mới thay điều kiện chờ thử người mới trước khi làm 06–10.

## Nội dung

| Màn | Bài học/câu đố |
| --- | --- |
| 06 · Đón bạn trở về | Một phần giữ tải, phần kia lắp B và C; cửa chốt cho cả hai về nhập |
| 07 · Bốn điểm dừng | Mỗi chạm đi một chốt; chốt 2 nhả khóa nguồn, chốt 4 nối bộ truyền |
| 08 · Chia việc đổi khớp | Phân vai giữ P và vận hành selector/nguồn |
| 09 · Rút rồi nối | Rút A để lắp B, đưa A trở về mới nối cả bộ truyền |
| 10 · Cỗ máy chung | Tổng hợp selector, nguồn, C và hai phần; không hướng dẫn lời giải; thắng mở Nhà |

01–05 giữ ID/thiết kế và đã được kiểm tra lại. Cả mười màn khóa xoay. Origin và save cũ vẫn được giữ; lượt này chưa chuyển toàn Origin hay tạo nhánh xoay tùy chọn. [Hồ sơ từng màn và cách tái tạo](COGHE_TAP_CAMPAIGN.md).

## Build cho người thử

Worktree: `/Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview`.
Unity **6000.3.19f1**. Chạy tại thư mục đó:

```bash
# Build APK đúng 10 màn mới
bash Tools/build-venom-android.sh --tap

# Hoặc tạo lại app Mac
bash Tools/build-venom.sh --tap
```

APK đầu ra khi bạn build: `Builds/COgheTapChapter/Android/COghe.apk`. **Chưa build APK/cài điện thoại trong lượt này**; Android builder đã biên dịch, shell wrapper đã kiểm tra cú pháp. Không truyền `--tap` vẫn build Origin. Không bật `COGHE_BENCHMARK` hoặc proof flags khi đưa user chơi.

App Mac có sẵn: `Builds/COgheTapChapter/macOS/COghe.app`.
Bản nén: `Builds/COgheTapChapter/COGHE_TAP_10_LEVELS_MACOS_2026_09_23.zip` (108,712,924 bytes).
SHA256: `72bfed91ec163ab5e24d657a9cd70c28a4500ebae3727064a050073011b9242c`.

Nếu build bằng Unity Build Profiles thủ công: dùng menu `Gravity Box → COghe → Use Tap Campaign for Player Build` để chỉ enable 01–10 và đặt 01 trước. Menu đó đổi danh sách Editor; enable lại các scene Origin/legacy trước full regression. Các lệnh `--tap` tự chọn scene, không cần đổi danh sách regression.

## Bằng chứng trên source bàn giao

| Kiểm tra | Kết quả | Artifact dưới `Artifacts/COgheTapChapter` |
| --- | --- | --- |
| Full PlayMode, Unity exit 0 | **382/382**, 0 fail/skip; 295.246s | `playmode-03/TestResults.xml`, `Editor.log`, `run.json` |
| Full EditMode, Unity exit 0 | **8/8**, 0 fail/skip | `editmode-02/TestResults.xml`, `Editor.log`, `run.json` |
| Build đúng 10 scene | Unity exit 0; explicit success marker | `build-03/Editor.log` |
| Player input + FixedUpdate bình thường | **10/10**, 32 escaped/1 fragment mỗi màn, 0 errors; process exit 0 | `player-release/20260923T055147618Z/run.json` |
| Hình và video từ framebuffer | 540×900, 776 frames có timestamps, 171.232s, không âm thanh; decode preview và fast-start đã kiểm tra | `COGHE_TAP_10_LEVELS_RELEASE.mp4`, `VIDEO_CONTAINER_CHECK.json` |
| Identity/cấu hình | GUID của scene/definition 01–05 giữ nguyên; mọi definition có 10 scene, khóa xoay, chỉ 10 là Boss | `ORIGINAL_IDENTITY_CHECK.json` và definitions |

PlayMode gồm 355 ca hồi quy cũ, 18 ca A/B, 9 ca mới: toàn bộ 27 ca TapCampaign qua. Đã kiểm tra giải trọn 06–10, đảo/cycle hết bốn chốt, bỏ lỡ nguồn rồi sửa, bấm dồn, pause/cancel/retry giữa hành trình, chạm mọi chốt ở 720×1280 và 720×1612, nguồn bị khóa nếu bỏ tải, một cơ thể không lách bài 08/10, thu hồi phần giữ/hợp thể, Boss không hint và mở Nhà một lần. Các kiểm tra cơ quan bị cản/không xuyên vật, mất tải giữa hành trình và thoát trước khi nhập vẫn qua trong 18 ca A/B.

Player release: **05:51:47–05:54:38 UTC**, Mac16,10 / Apple M4 / macOS 26.5.1; Mono Development universal. Build GUID **`91afe91551bf45c9aea0ed7ab35c14ef`**. Replay dùng mouse press/release qua InputSystem; từng màn được tải trong harness, không thay vị trí mô/cửa hoặc ép thắng. Chọn catalog và bảo toàn ID có test riêng; không gọi replay này là buổi người mới tự chơi hay phép đo FPS.

## Nhận diện source và các sửa lỗi đã kiểm chứng

Branch `codex/venom-macos-preview`, base HEAD `8b7f92a871cd2d1facc83ccea114521056b5cda9`; **thay đổi A/B+C chưa commit/push**. Snapshot SHA256 của Assets/Packages/ProjectSettings trước build 03, PlayMode 03 và EditMode 02 trùng nhau: `build-03-source.json`, `test-03-source.json`, `editmode-02-source.json`. Đã so lại bytes cuối với snapshot.

`SOURCE_CHANGESET.tar.gz` chứa phần thay đổi so với base; không chứa hai ProjectSettings asset đã có trước nhiệm vụ. `RELEASE_MANIFEST.json` ghi hashes và nguồn bằng chứng. `BEFORE_STAGE_C.tar.gz` giữ trạng thái A/B trước chặng C. Các app/evidence A/B và Origin trước vẫn còn ở đường dẫn cũ.

Các lượt đầu bắt lỗi cần nguồn quá sát spawn và vùng chạm dao lấn tay nắm selector tại chốt 3. Đã dời cần nguồn, thu vùng chạm dao riêng 08/10, nâng nhãn số và tách nhãn bị đè; không nới điều kiện thắng hoặc teleport mô. Một lượt player đầu kết thúc mà không có report cuối; nguyên nhân chưa xác định, không tính là pass. Hai lượt trực tiếp sau đó đều hoàn thành 10/10, exit 0. Chi tiết lịch sử ở work log workspace.

Unity tự đổi `_SrcBlend` của ba material maze cũ khi chạy regression; diff được lưu và bytes được khôi phục, không đưa thay đổi ngoài phạm vi vào bàn giao. Runner có thể ghi `success:false` do heuristic bắt lỗi handshake licensing lúc đầu; kết luận ở đây dựa trên exit code thật, XML, log compile và marker build thành công.

## Bước thử người mới

Dùng [phiếu quan sát 10 màn](COGHE_TAP_PLAYTEST.md), không cho người mới xem video lời giải trước buổi thử. Chưa có số liệu mục tiêu 80%, độ hiểu, retention, touch/safe area trên điện thoại hoặc frame-time/nhiệt máy thật. Source/build sẵn sàng cho vòng test đó; không coi replay Mac là bằng chứng thay thế.
