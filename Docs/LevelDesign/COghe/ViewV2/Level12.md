# COghe V2 — 12 · Dưới chiếc nắp

Trạng thái triển khai ngày 28/09/2026: **Gameplay đã qua kiểm chứng tự động trên Mac; art đã rà ảnh Unity**. Điện thoại và người chơi mới chưa nghiệm thu.

[Hồ sơ thiết kế đã được giao triển khai](ExpansionConcept/LEVEL_12.md) giữ đủ mục tiêu, lời giải, phục hồi, cơ quan và ngân sách. Bản gốc trong thư mục này là lịch sử thiết kế, các câu “chưa dựng” ở đó mô tả thời điểm gửi concept.

## Bản dựng

Nắp mở đầu dốc rộng từ sàn trên xuống hành lang thấp.

- ID: `coghe.view.v2.12`; scene `Assets/_Game/Venom/ViewCampaign/COgheView12.unity`.
- Builder: `Assets/_Game/Editor/COgheViewExpansionBuilder.cs` và các partial Discovery/Branch/Cooperation.
- Giữ 32 hạt, 120 Hz; camera yaw, pinch/Toàn cảnh, chạm giao tác vụ; hộp đứng yên.
- Bộ art V2 dùng chung: sứ kem, vách xanh, cơ quan amber, ống cyan, dao bạc và trạng thái mint.

## Kiểm chứng

- `COgheViewExpansionTests.View12FullSolution`: đường giải qua picking màn hình, đủ 32 hạt và một cơ thể. **Đạt trên candidate 22**.
- `COgheViewExpansionTests.View12RecoveryAndPortrait`: Retry/pause/lệnh lặp, quyền điều khiển, catalog/save key, hai tỷ lệ portrait. **Đạt trên candidate 22**.
- Replay touch native candidate 22, cả tăng tốc và tốc độ thực: **đạt**, 32 hạt thoát trong một cơ thể, không lỗi runtime được ghi nhận. [Ảnh đầu màn](../../../Verification/COgheViewExpansion/Images/12-start.png).
- Điện thoại và người chơi mới: chưa đo/chưa thử.

## Ca sai và phục hồi đã chạy

- `View12LidReversesAndReopensWithoutReset`: đạt.

[Báo cáo, source/build manifest, XML đầy đủ và giới hạn](../../../Verification/COgheViewExpansion/README.md). Kết quả này là replay tác giả; không thay cho đánh giá độ khó của người chơi mới.
