# COghe V2 — 18 · Rẽ nhánh

Trạng thái triển khai ngày 28/09/2026: **Gameplay đã qua kiểm chứng tự động trên Mac; art đã rà ảnh Unity**. Điện thoại và người chơi mới chưa nghiệm thu.

[Hồ sơ thiết kế đã được giao triển khai](ExpansionConcept/LEVEL_18.md) giữ đủ mục tiêu, lời giải, phục hồi, cơ quan và ngân sách. Bản gốc trong thư mục này là lịch sử thiết kế, các câu “chưa dựng” ở đó mô tả thời điểm gửi concept.

## Bản dựng

Y có hai nhánh, cửa chọn đặt ngoài miệng ống. Bộ chọn khóa trong lúc có mô đang chuyển.

- ID: `coghe.view.v2.18`; scene `Assets/_Game/Venom/ViewCampaign/COgheView18.unity`.
- Builder: `Assets/_Game/Editor/COgheViewExpansionBuilder.cs` và các partial Discovery/Branch/Cooperation.
- Giữ 32 hạt, 120 Hz; camera yaw, pinch/Toàn cảnh, chạm giao tác vụ; hộp đứng yên.
- Bộ art V2 dùng chung: sứ kem, vách xanh, cơ quan amber, ống cyan, dao bạc và trạng thái mint.

## Kiểm chứng

Điều chỉnh sau replay thời gian thực: B nằm tại z=.26, tay nắm hướng về bệ
trống phía sau miệng ống, điểm đứng z=.345. Vị trí trước đặt điểm đứng ở z=.235
khiến mô vừa ra ống đi ngược vào thành ống. Sửa bố cục này giữ nguyên thứ tự
nhánh phụ → B → quay về → đổi nhánh; không đổi lực, solver hoặc lời giải tác giả.

- `COgheViewExpansionTests.View18FullSolution`: đường giải qua picking màn hình, đủ 32 hạt và một cơ thể. **Đạt trên candidate 22**.
- `COgheViewExpansionTests.View18RecoveryAndPortrait`: Retry/pause/lệnh lặp, quyền điều khiển, catalog/save key, hai tỷ lệ portrait. **Đạt trên candidate 22**.
- Replay touch native candidate 22, cả tăng tốc và tốc độ thực: **đạt**, 32 hạt thoát trong một cơ thể, không lỗi runtime được ghi nhận. [Ảnh đầu màn](../../../Verification/COgheViewExpansion/Images/18-start.png).
- Điện thoại và người chơi mới: chưa đo/chưa thử.

## Ca sai và phục hồi đã chạy

- `View18WrongBranchReturnsAndOccupiedTubeLocksSelector`: đạt.

[Báo cáo, source/build manifest, XML đầy đủ và giới hạn](../../../Verification/COgheViewExpansion/README.md). Kết quả này là replay tác giả; không thay cho đánh giá độ khó của người chơi mới.
