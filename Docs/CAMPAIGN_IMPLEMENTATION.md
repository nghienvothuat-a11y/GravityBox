# Campaign 100 — bản triển khai prototype

Ngày 09/09/2026. Nội dung nằm riêng trong `Assets/_Game/Campaign`: 100 LevelDefinition, 100 prefab và 100 LevelDesignProfile. Scene chạy là `Assets/_Game/Scenes/Campaign.unity`; scene `Gameplay.unity` và catalog Lab giữ 23 thí nghiệm trước.

## Nội dung và phân công

10 chương × 10 màn; boss ở C010, C020, …, C100. 55 màn dễ/luyện/nghỉ do **gpt-5.6-sol** triển khai; 35 màn khó và 10 boss do **gpt-6-astra** triển khai. Model được ghi trong từng profile. Ma trận chi tiết: [100 màn](CAMPAIGN_LEVEL_MATRIX.md); nội dung author riêng: [Sol](CAMPAIGN_EASY_IMPLEMENTATION.md), [Astra khó](CAMPAIGN_HARD_IMPLEMENTATION.md), [Astra boss](CAMPAIGN_BOSS_IMPLEMENTATION.md).

Có các hình cơ bản, cửa trượt trọng lực, cầu bản lề/chốt tiếp xúc, lồng độc lập, phối hợp hai bi, cam cóc nhớ, con lắc, khoảng bay và sân hứng, nước, thủy ngân và mạng đường ba chiều. Màn nước/thủy ngân giữ một bi và vật cản tĩnh trong miền chữ nhật khớp force model. Các boss kết hợp cơ cấu đã học; ánh sáng quan sát trạng thái thật đã đạt, không dùng tín hiệu ánh sáng để mở collider hoặc khóa điều kiện thắng.

## Quyền sở hữu và tính ổn định

- `CampaignBuilder` là điểm author/build; các builder Easy/Hard/Boss chỉ tạo dữ liệu Editor. Runtime không sinh lại mê cung hoặc âm thầm thay collider.
- `GeometryAssetScope` tách mesh campaign theo C001…C100. Tài nguyên Lab có thể được tham chiếu để tái sử dụng nhưng không bị ghi đè khi tạo campaign. Vật liệu quan sát campaign có asset riêng.
- `LevelDefinition` có ID ổn định và tham chiếu `LevelDesignProfile`. Profile lưu chương, vai trò, boss, kỹ năng, ngân sách độ khó, nội dung gốc, recovery và phiên bản; không dùng số thứ tự để khóa save.
- `CampaignProgress` lưu JSON phiên bản qua `ICampaignProgressStorage`; bản chơi dùng PlayerPrefs, kiểm thử dùng bộ nhớ riêng. JSON lỗi được phục hồi an toàn, schema tương lai không bị ghi đè. Hoàn thành chỉ ghi sau khi mọi bi thoát thật.
- `LevelManager` sở hữu load/reset, roster bi, props tách khỏi gốc, lực, sự kiện và chuyển catalog. Khi mọi bi đã thoát, màn kế tự tải sau 1,15 giây; C100 giữ nguyên ở trạng thái hoàn thành thay vì quay lại C001. Replay boss tạm giữ lượt chuyển và tiếp tục đếm sau khi replay kết thúc.
- `BossReplay` giữ pose quan sát của hộp/props/bi trong 450 mẫu 30 Hz. Playback tạo bản sao chỉ có renderer, không có collider/Rigidbody; không điều khiển lại cơ cấu hoặc giả lập một đường giải. Có thể xem khoảng 15 giây cuối sau khi thắng; chưa có xuất video/chia sẻ.

Giữ bi thép đường kính 30 mm, khối lượng khoảng 111 g, 120 Hz và trọng lực 9,81 m/s². Joint, lực tiếp xúc và trọng lực tạo chuyển động. Chốt/cóc là ràng buộc giữ lý tưởng sau khi cơ cấu đã thực sự tới nấc, không phải mô hình đàn hồi đầy đủ của răng kim loại. Assist chung trong 40 mm cuối là hỗ trợ gameplay đã thống nhất, không phải lực tự nhiên. Nước/thủy ngân là mô hình lực xấp xỉ hiện có, không phải fluid solver đầy đủ.

## Cách test bản Mac

Mở `Builds/macOS/Gravity Box.app`. `LEVELS` mở danh sách theo chương, mỗi trang 10 màn. Tất cả 100 màn được chọn trực tiếp để thuận tiện test. Nhãn `BOSS` và dấu hoàn thành hiện trong danh sách. Nút chuyển catalog mở 23 màn Lab. Kéo trong vùng hộp để xoay, R reset, P/Esc pause. Nút −/+ phóng quan sát quanh vị trí hiện tại của bi; góc nhìn giữ ổn định khi đang nghiêng. Sàn phẳng khô mờ đi khi mặt dưới hướng camera, collider giữ nguyên. Sau khi thắng boss, `XEM LẠI` phát lại chuyển động vừa xảy ra.

CSV playtest local ghi ID/version/chapter/boss/role/ngân sách độ khó, thời gian, reset, drag, số bi thoát và các sự kiện pause/abandon/zoom/replay. Không gửi dữ liệu ra mạng. Thư mục là `Application.persistentDataPath/Playtests`.

## Kiểm chứng và giới hạn

Kết quả cụ thể được ghi trong [Verification/Campaign100](Verification/Campaign100/README.md). Phân biệt ba phạm vi: audit collider và spawn; mô phỏng lifecycle/miệng thoát; route input thật và fixture cơ cấu có giới hạn. Đặt bi ở miệng lỗ trong test chỉ kiểm tra đoạn thoát, không chứng minh giải được mê cung từ spawn.

100 prefab là nội dung đã author để playtest, không đồng nghĩa 100 đường giải đã được chạy end-to-end. Các màn phối hợp dài C040/C098/C100 cần đặc biệt kiểm tra cách quay lại, liên kết giữa các module và khả năng nhìn thấy bi trên điện thoại. Zoom giúp quan sát nhưng không thay thế nghiệm thu readability ở kích thước thiết bị thật. Không có cam kết đã đạt độ khó cảm nhận hoặc khả năng viral.

`DifficultyBudget` là trần authoring từ kế hoạch. `HasAuthorRating=false`, `PlaytestCalibrated=false` tránh trình bày vector chưa chấm và ngân sách chưa hiệu chỉnh như dữ liệu thực nghiệm. Cần thu median/P80 thời gian, reset, bỏ cuộc, hiểu cơ chế và recovery của từng cohort để chỉnh thứ tự/nội dung trước phát hành sản phẩm. Prototype chưa dựng Challenge/Mastery riêng và không dùng chúng để bù 100 màn.

## Tái tạo và build

Dùng Unity **6000.3.19f1**. Menu **Gravity Box → Campaign → Generate 100 Levels** tạo lại từ JSON và các builder; commit/save thay đổi author trước khi regenerate. CLI `-executeMethod GravityBox.Editor.CampaignBuilder.GenerateAndAudit` chạy author và audit. `bash Tools/verify.sh` chạy EditMode và PlayMode; `bash Tools/build.sh macOS` build campaign kèm Lab. APK/iOS không được build trong lượt này; lệnh mobile cũ vẫn phục vụ Lab cho đến đợt tích hợp mobile riêng.
