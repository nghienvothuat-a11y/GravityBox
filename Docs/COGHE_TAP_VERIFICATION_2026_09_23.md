# COghe — bàn giao prototype chạm cơ quan 01–05

Đã hoàn thành triển khai và kiểm chứng kỹ thuật **chặng A/B** của kế hoạch được
Mrk yêu cầu ngày 23/09/2026. Đây là năm màn prototype riêng, chưa phải rollout
10 màn/chuyển campaign ở chặng C. Cần kết quả người mới trước bước đó.

## Bản mở để chơi

- App: `Builds/COgheTap/macOS/COghe.app`.
- Bản nén: `Builds/COgheTap/COGHE_TAP_MACOS_2026_09_23_FINAL.zip` (108,316,536 byte).
- Khai báo target tối thiểu macOS 12; đã chạy trên macOS 26.5.1; universal Apple Silicon + Intel, Development/Mono/Metal.
- Mở bình thường, không thêm cờ kiểm thử. Menu của app gồm 01–05.
- In-game brand là COghe. Giữ product/bundle identity `Venom` /
  `com.Gravity-Box-Lab.Venom` để dùng namespace save hiện có; app nằm ở đường dẫn riêng.

```bash
open -n "/Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview/Builds/COgheTap/macOS/COghe.app"
```

## Hành vi đã có

- Một chạm giao trọn hành trình: tự tới, đẩy/kéo bằng lực hữu hạn, dừng tại chốt
  thật và kết thúc tác vụ. Chạm sau khi xong để đi chiều ngược; không xếp hàng tap spam.
- Bánh răng → trục → thanh răng → cửa; cửa hồi ở 01–03, nắp sàn có chốt giữ ở 05.
  Chuyển động/animation đọc trạng thái mô phỏng, không quyết định thắng bằng thời lượng clip.
- Phân chia qua dao thật, bảo toàn 32 hạt; chọn phần khác vẫn giữ nhiệm vụ ở A.
  Màn 05 cần hai phần để vận hành; sau mở cửa có thể nhả A, nhập và thoát.
- Năm màn khóa xoay vật lý, đích bấm nhìn thấy được. Hướng dẫn thay đổi theo tiến
  triển và làm lại; bài thứ tự 03 gợi quan sát liên kết, không chỉ toàn thứ tự giải.
- Catalog/ID riêng `coghe.tap.v1.01..05`. Scene và save ID Origin không bị gán lại.

## Kết quả cuối

| Kiểm chứng | Kết quả | Bằng chứng |
| --- | --- | --- |
| Full PlayMode | **373/373**, 0 fail/skip; 276.90 giây | [NUnit XML](../Artifacts/COgheTap/playmode-08/TestResults.xml) |
| Full EditMode | **8/8**, 0 fail/skip | [NUnit XML](../Artifacts/COgheTap/editmode-04/TestResults.xml) |
| Build macOS | Unity exit 0, BuildPipeline thành công | [Editor log](../Artifacts/COgheTap/build-04/Editor.log) |
| Player thực | **5/5** màn, từng màn 32 hạt thoát/1 cơ thể; 0 error/exception ghi nhận | [run.json](../Artifacts/COgheTap/player-release/20260923T045841568Z/run.json) |
| Hình ảnh | Đã xem framebuffer các màn, chữ A/B, hướng dẫn, mở/đóng cửa, hai phần và thắng | [Ảnh màn 05](../Artifacts/COgheTap/player-release/20260923T045841568Z/05-two-parts-open.png) |
| Diff | `git diff --check` qua; chỉ thay đổi thuộc tính năng và scene mới | [Snapshot trạng thái](../Artifacts/COgheTap/final-status.txt) |

373 PlayMode gồm **355 ca hồi quy có sẵn và 18 ca mới**. Các ca mới kiểm tra:
hai chiều/mở–đóng, dừng ổn định, tap spam, pause, retry lúc tiếp cận/đang tác động,
đổi đích, vật cản thật, cover che vùng chạm, disable/recovery, thứ tự cơ quan,
cắt/chọn phần/nhập, một cơ thể không vượt liên động, hai phần phối hợp, mất tải
giữa hành trình, tranh quyền điều khiển, thua đúng khi thoát trước nhập, catalog,
hướng dẫn và target trong khung 720×1280 / 720×1612.

Player cuối chạy **04:58:41–04:59:52 UTC**, Mac16,10 / Apple M4, macOS 26.5.1
(25F80), viewport 540×900. Replay dùng `InputSystem` mouse press/release qua
đường input của game và `FixedUpdate` bình thường. Không đặt vật/cơ thể vào trạng
thái thắng. Harness tắt auto-next, mở pause ban đầu nếu cần và tắt ghi save trong
lượt QA để không làm bẩn tiến trình người dùng.

[Video liên tục](../Artifacts/COgheTap/COGHE_TAP_PLAYTHROUGH_RELEASE.mp4):
71.17 giây, 308 frame mã hóa từ framebuffer, giữ khoảng thời gian đo giữa các frame, không âm thanh.
Đây là replay tự động, không phải người mới chơi và không phải video đo FPS.
Đã kiểm tra giải mã ảnh preview và vị trí `moov` trước `mdat` để phát trực tiếp.

## Các lỗi đã được phát hiện rồi sửa

- Camera nhìn cửa cạnh mỏng và spawn đè cơ quan; sửa bố trí/camera.
- Vùng chọn bị dao che và nhãn A/B chìm trong mặt bánh răng; sửa vị trí/vùng chọn/nhãn.
- Nhát cắt tiếp trong một stroke hủy lệnh vừa giao; dao ở prototype nâng lại sau
  khi mô thực sự tách, không ép chia hoặc khóa nhập.
- Nhập quá sớm trong phòng luyện khi ra lệnh nhanh; đặt lại dao giữa các mẫu route
  và giữ kiểm tra hai lệnh liên tiếp. Giả thuyết từ bố cục graph được kiểm chứng
  bằng ca tái hiện trước/sau; không thay solver.
- Player thật phát hiện kẹt leo cửa bên ở màn 05 dù các ca mô phỏng đã qua.
  Đổi thành nắp sàn nâng bằng thanh răng; player cuối đã thoát được.
- Rà video phát hiện bánh răng đầu ra chưa quay theo chiều hồi cửa; bổ sung truyền động ngược theo vận tốc thanh răng và chỉ qua các bánh răng còn ăn khớp, có test riêng.
- HUD vẫn hiện catalog 40 màn và lời nhắc bánh răng sau khi đã mở cửa; sửa menu
  theo scene sequence và hướng dẫn theo trạng thái thực.

Các lượt lỗi giữ trong `Artifacts/COgheTap/playmode-01..04` và `player-repro-01`;
không dùng những lượt đó để chứng nhận bản cuối.

## Nhận diện source và artifact

- Worktree: `REPOS/GravityBox-macos-preview`, branch `codex/venom-macos-preview`.
- Base HEAD: `8b7f92a871cd2d1facc83ccea114521056b5cda9`; thay đổi hiện chưa commit/push.
- Unity **6000.3.19f1**, URP 17.3.0, Input System 1.17.0, Test Framework 1.6.0.
- Build GUID: `67032bef31644cd6bc10850dcc557cfd`.
- [Release manifest](../Artifacts/COgheTap/RELEASE_MANIFEST.json),
  [hash từng file app](../Artifacts/COgheTap/APP_SHA256_MANIFEST.json),
  [snapshot test](../Artifacts/COgheTap/test-08-source.json),
  [snapshot build](../Artifacts/COgheTap/build-04-source.json),
  [source changeset](../Artifacts/COgheTap/SOURCE_CHANGESET.tar.gz).
- SHA-256 ZIP: `155798376401226315f611dc1e1979441a4327d83ec9ee9fb06fe659d588e0d0`.
- App content digest: `ed2744a4834b7c1d1fea2e5ad71d7a8f15b7937993b7efbcce727047f8fea09f`
  (SHA-256 của JSON map path→file hash, khóa sort, separator `,`/`:`).

Snapshot test→build cuối giống nhau hoàn toàn cho `Assets`, `Packages` và `ProjectSettings`. Material drift ở các vòng trước đã được khôi phục; code/scene của lượt test cuối và build bàn giao trùng nhau.
Hai ProjectSettings asset có sẵn nhưng chưa tracked được giữ nguyên và không
đưa vào source changeset của tác vụ này.

## Phần chưa được chứng nhận

- Chưa thử với người mới; chưa có căn cứ nói đạt mục tiêu 80% hoặc tăng retention.
- Chưa build/chạy trên điện thoại thật, chưa chứng nhận touch, background/resume,
  FPS, nhiệt và pin trên Android/iOS. Portrait test và Mac không thay thế bước này.
- Lượt player QA tắt ghi save; chưa thực hiện quit/relaunch với save người chơi thật.
  Save key/backend không sửa, và ID mới cùng namespace cũ được kiểm tra ở source.
- Màn 04 cố ý là phòng luyện có thể bỏ qua; cần quan sát việc bỏ qua có gây mắc ở 05.
- Chặng C (10 màn, cơ quan nhiều vị trí, biến thể xoay thưa và chuyển campaign)
  chưa triển khai; tiếp tục sau bằng chứng người mới theo kế hoạch đã duyệt.

[Phiếu thử người mới](COGHE_TAP_PLAYTEST.md) đã sẵn sàng cho bước tiếp theo.
